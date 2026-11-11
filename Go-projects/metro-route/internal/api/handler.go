package api

import (
	"context"
	"encoding/json"
	"errors"
	"log/slog"
	"net/http"
	"strconv"
	"time"

	"github.com/nastasyaR10/projects/metro-route/internal/models"
	"github.com/nastasyaR10/projects/metro-route/internal/router"
	"github.com/nastasyaR10/projects/metro-route/internal/storage"
)

type RequestStore interface {
	SaveRequest(ctx context.Context, userID int64, from, to, mode string) error
	ListByUser(ctx context.Context, userID int64, limit int) ([]*storage.Request, error)
}

type Handler struct {
	graph *models.Graph
	db    RequestStore
}

func NewHandler(g *models.Graph, db RequestStore) *Handler {
	return &Handler{graph: g, db: db}
}

func (h *Handler) Routes() http.Handler {
	mux := http.NewServeMux()
	mux.HandleFunc("POST /route", h.handleRoute)
	mux.HandleFunc("GET /stations", h.handleStations)
	mux.HandleFunc("GET /history/{user_id}", h.handleHistory)
	mux.HandleFunc("GET /healthz", h.handleHealth)

	return Logging(Recovery(UserID(mux)))
}

// POST /route
func (h *Handler) handleRoute(w http.ResponseWriter, r *http.Request) {
	var req RouteRequest
	r.Body = http.MaxBytesReader(w, r.Body, 1<<20)
	dec := json.NewDecoder(r.Body)
	dec.DisallowUnknownFields()
	if err := dec.Decode(&req); err != nil {
		writeError(w, http.StatusBadRequest, "invalid json: "+err.Error())
		return
	}

	if req.From == "" || req.To == "" {
		writeError(w, http.StatusBadRequest, "from and to are required")
		return
	}
	if _, ok := h.graph.Stations[req.From]; !ok {
		writeError(w, http.StatusNotFound, "station not found: "+req.From)
		return
	}
	if _, ok := h.graph.Stations[req.To]; !ok {
		writeError(w, http.StatusNotFound, "station not found: "+req.To)
		return
	}

	mode, err := stringToMode(req.Mode)
	if err != nil {
		writeError(w, http.StatusBadRequest, err.Error())
		return
	}

	route, err := router.ShortestPath(r.Context(), h.graph, req.From, req.To, mode)
	if err != nil {
		switch {
		case errors.Is(err, router.ErrUnknownStation):
			writeError(w, http.StatusNotFound, err.Error())
		case errors.Is(err, router.ErrNoPath):
			writeError(w, http.StatusUnprocessableEntity, "no path between stations")
		case errors.Is(err, context.Canceled), errors.Is(err, context.DeadlineExceeded):
			slog.Debug("route request cancelled", "err", err)
		default:
			slog.Error("build route failed", "err", err, "from", req.From, "to", req.To)
			writeError(w, http.StatusInternalServerError, "internal error")
		}
		return
	}

	if userID := userIDFromCtx(r.Context()); userID != 0 {
		dbCtx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
		defer cancel()
		if err := h.db.SaveRequest(dbCtx, userID, req.From, req.To, modeToString(mode)); err != nil {
			slog.Error("save request failed",
				"err", err,
				"user_id", userID,
				"from", req.From,
				"to", req.To,
				"mode", modeToString(mode),
			)
		}
	}

	writeJSON(w, http.StatusOK, toRouteResponse(route))
}

// GET /stations
func (h *Handler) handleStations(w http.ResponseWriter, r *http.Request) {
	out := make([]StationView, 0, len(h.graph.Stations))
	for _, s := range h.graph.Stations {
		out = append(out, StationView{ID: s.ID, Name: s.Name, LineID: s.LineID})
	}
	writeJSON(w, http.StatusOK, out)
}

// GET /history/{user_id}
const historyDefaultLimit = 10

func (h *Handler) handleHistory(w http.ResponseWriter, r *http.Request) {
	rawID := r.PathValue("user_id")
	userID, err := strconv.ParseInt(rawID, 10, 64)
	if err != nil {
		writeError(w, http.StatusBadRequest, "user_id must be a valid int64")
		return
	}

	if headerUserID := userIDFromCtx(r.Context()); headerUserID != 0 && headerUserID != userID {
		writeError(w, http.StatusForbidden, "cannot view another user's history")
		return
	}

	limit := historyDefaultLimit
	if rawLimit := r.URL.Query().Get("limit"); rawLimit != "" {
		parsed, err := strconv.Atoi(rawLimit)
		if err != nil || parsed <= 0 || parsed > 100 {
			writeError(w, http.StatusBadRequest, "limit must be an integer in [1, 100]")
			return
		}
		limit = parsed
	}

	reqs, err := h.db.ListByUser(r.Context(), userID, limit)
	if err != nil {
		slog.Error("list history failed", "err", err, "user_id", userID)
		writeError(w, http.StatusInternalServerError, "failed to load history")
		return
	}

	out := make([]HistoryItem, 0, len(reqs))
	for _, req := range reqs {
		out = append(out, HistoryItem{
			FromStation: req.FromStation,
			ToStation:   req.ToStation,
			Mode:        req.Mode,
			CreatedAt:   req.CreatedAt,
		})
	}
	writeJSON(w, http.StatusOK, out)
}

// GET /healthz
func (h *Handler) handleHealth(w http.ResponseWriter, r *http.Request) {
	if _, err := h.db.ListByUser(r.Context(), 0, 1); err != nil {
		slog.Warn("health: db not ready", "err", err)
		writeJSON(w, http.StatusServiceUnavailable, map[string]string{"status": "degraded", "reason": "db unavailable"})
		return
	}
	writeJSON(w, http.StatusOK, map[string]string{"status": "ok"})
}

func stringToMode(s string) (models.RouteMode, error) {
	switch s {
	case "", "fast":
		return models.ModeFastest, nil
	case "fewest_transfers":
		return models.ModeFewestTransfers, nil
	default:
		return 0, errors.New("unknown mode: " + s)
	}
}

func toRouteResponse(r *models.Route) RouteResponse {
	segs := make([]SegmentView, 0, len(r.Segments))
	for _, s := range r.Segments {
		stations := make([]StationView, 0, len(s.Stations))
		for _, st := range s.Stations {
			stations = append(stations, StationView{ID: st.ID, Name: st.Name, LineID: st.LineID})
		}
		segs = append(segs, SegmentView{
			Stations:   stations,
			TimeMin:    s.TimeMin,
			IsTransfer: s.IsTransfer,
		})
	}
	return RouteResponse{
		TotalTimeMin:   r.TotalTimeMin,
		TransfersCount: r.TransfersCount,
		Mode:           modeToString(r.Mode),
		Segments:       segs,
	}
}

func modeToString(m models.RouteMode) string {
	switch m {
	case models.ModeFewestTransfers:
		return "fewest_transfers"
	case models.ModeFastest:
		return "fast"
	default:
		return "fast"
	}
}
