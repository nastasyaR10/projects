package api

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"net/http"
	"net/http/httptest"
	"strings"
	"sync"
	"testing"
	"time"

	"github.com/nastasyaR10/projects/metro-route/internal/models"
	"github.com/nastasyaR10/projects/metro-route/internal/storage"
)

func testGraph() *models.Graph {
	return &models.Graph{
		Stations: map[string]models.Station{
			"a": {ID: "a", Name: "Station A", LineID: "line1"},
			"b": {ID: "b", Name: "Station B", LineID: "line1"},
			"c": {ID: "c", Name: "Station C", LineID: "line1"},
			"d": {ID: "d", Name: "Station D", LineID: "line2"},
		},
		AdjacencyList: map[string][]models.GraphEdge{
			"a": {{To: "b", WeightMin: 3}},
			"b": {{To: "a", WeightMin: 3}, {To: "c", WeightMin: 3}},
			"c": {{To: "b", WeightMin: 3}},
			"d": {},
		},
	}
}

type fakeStore struct {
	mu   sync.Mutex
	reqs []*storage.Request
	err  error
	next int64
}

func (f *fakeStore) SaveRequest(_ context.Context, userID int64, from, to, mode string) error {
	if f.err != nil {
		return f.err
	}
	f.mu.Lock()
	defer f.mu.Unlock()
	f.next++
	f.reqs = append(f.reqs, &storage.Request{
		ID:          f.next,
		UserID:      userID,
		FromStation: from,
		ToStation:   to,
		Mode:        mode,
		CreatedAt:   time.Now(),
	})
	return nil
}

func (f *fakeStore) ListByUser(_ context.Context, userID int64, limit int) ([]*storage.Request, error) {
	if f.err != nil {
		return nil, f.err
	}
	f.mu.Lock()
	defer f.mu.Unlock()
	out := make([]*storage.Request, 0, limit)
	for i := len(f.reqs) - 1; i >= 0 && len(out) < limit; i-- {
		if f.reqs[i].UserID == userID {
			out = append(out, f.reqs[i])
		}
	}
	return out, nil
}

func (f *fakeStore) count() int {
	f.mu.Lock()
	defer f.mu.Unlock()
	return len(f.reqs)
}

func newTestHandler(store RequestStore) http.Handler {
	return NewHandler(testGraph(), store).Routes()
}

func doRequest(t *testing.T, h http.Handler, method, path string, body any, headers map[string]string) *httptest.ResponseRecorder {
	t.Helper()
	var buf bytes.Buffer
	if body != nil {
		if err := json.NewEncoder(&buf).Encode(body); err != nil {
			t.Fatalf("encode body: %v", err)
		}
	}
	req := httptest.NewRequest(method, path, &buf)
	if body != nil {
		req.Header.Set("Content-Type", "application/json")
	}
	for k, v := range headers {
		req.Header.Set(k, v)
	}
	rec := httptest.NewRecorder()
	h.ServeHTTP(rec, req)
	return rec
}

func decodeJSON[T any](t *testing.T, rec *httptest.ResponseRecorder) T {
	t.Helper()
	var v T
	if err := json.Unmarshal(rec.Body.Bytes(), &v); err != nil {
		t.Fatalf("decode response: %v\nbody=%s", err, rec.Body.String())
	}
	return v
}

func TestHandleRoute_HappyPath(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodPost, "/route",
		RouteRequest{From: "a", To: "c", Mode: "fast"}, nil)

	if rec.Code != http.StatusOK {
		t.Fatalf("status: got %d, want 200, body=%s", rec.Code, rec.Body.String())
	}
	if ct := rec.Header().Get("Content-Type"); !strings.HasPrefix(ct, "application/json") {
		t.Fatalf("Content-Type: got %q, want application/json", ct)
	}

	resp := decodeJSON[RouteResponse](t, rec)
	if resp.TotalTimeMin != 6 {
		t.Fatalf("TotalTimeMin: got %d, want 6", resp.TotalTimeMin)
	}
	if resp.Mode != "fast" {
		t.Fatalf("Mode: got %q, want fast", resp.Mode)
	}
	if len(resp.Segments) == 0 {
		t.Fatal("Segments should not be empty")
	}
}

func TestHandleRoute_DefaultModeIsFast(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodPost, "/route",
		RouteRequest{From: "a", To: "c"}, nil)

	if rec.Code != http.StatusOK {
		t.Fatalf("status: got %d, want 200, body=%s", rec.Code, rec.Body.String())
	}
	resp := decodeJSON[RouteResponse](t, rec)
	if resp.Mode != "fast" {
		t.Fatalf("Mode: got %q, want fast", resp.Mode)
	}
}

func TestHandleRoute_FewestTransfers(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodPost, "/route",
		RouteRequest{From: "a", To: "c", Mode: "fewest_transfers"}, nil)

	if rec.Code != http.StatusOK {
		t.Fatalf("status: got %d, want 200", rec.Code)
	}
	resp := decodeJSON[RouteResponse](t, rec)
	if resp.Mode != "fewest_transfers" {
		t.Fatalf("Mode: got %q, want fewest_transfers", resp.Mode)
	}
}

func TestHandleRoute_InvalidJSON(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	req := httptest.NewRequest(http.MethodPost, "/route", strings.NewReader("{not json"))
	req.Header.Set("Content-Type", "application/json")
	rec := httptest.NewRecorder()
	h.ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Fatalf("status: got %d, want 400", rec.Code)
	}
	resp := decodeJSON[ErrorResponse](t, rec)
	if !strings.Contains(resp.Error, "invalid json") {
		t.Fatalf("error: got %q, want contains 'invalid json'", resp.Error)
	}
}

func TestHandleRoute_UnknownFieldRejected(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	body := map[string]any{"from": "a", "to": "c", "extra_field": "oops"}
	rec := doRequest(t, h, http.MethodPost, "/route", body, nil)

	if rec.Code != http.StatusBadRequest {
		t.Fatalf("status: got %d, want 400 (DisallowUnknownFields)", rec.Code)
	}
}

func TestHandleRoute_MissingFromTo(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	cases := []struct {
		name string
		body RouteRequest
	}{
		{"empty both", RouteRequest{}},
		{"missing from", RouteRequest{To: "c"}},
		{"missing to", RouteRequest{From: "a"}},
	}
	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			rec := doRequest(t, h, http.MethodPost, "/route", c.body, nil)
			if rec.Code != http.StatusBadRequest {
				t.Fatalf("status: got %d, want 400, body=%s", rec.Code, rec.Body.String())
			}
		})
	}
}

func TestHandleRoute_UnknownFrom(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodPost, "/route",
		RouteRequest{From: "ghost", To: "c"}, nil)

	if rec.Code != http.StatusNotFound {
		t.Fatalf("status: got %d, want 404, body=%s", rec.Code, rec.Body.String())
	}
	resp := decodeJSON[ErrorResponse](t, rec)
	if !strings.Contains(resp.Error, "ghost") {
		t.Fatalf("error: got %q, want mention 'ghost'", resp.Error)
	}
}

func TestHandleRoute_UnknownTo(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodPost, "/route",
		RouteRequest{From: "a", To: "ghost"}, nil)

	if rec.Code != http.StatusNotFound {
		t.Fatalf("status: got %d, want 404", rec.Code)
	}
}

func TestHandleRoute_UnknownMode(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodPost, "/route",
		RouteRequest{From: "a", To: "c", Mode: "teleport"}, nil)

	if rec.Code != http.StatusBadRequest {
		t.Fatalf("status: got %d, want 400", rec.Code)
	}
	resp := decodeJSON[ErrorResponse](t, rec)
	if !strings.Contains(resp.Error, "unknown mode") {
		t.Fatalf("error: got %q, want contains 'unknown mode'", resp.Error)
	}
}

func TestHandleRoute_NoPath(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodPost, "/route",
		RouteRequest{From: "a", To: "d"}, nil)

	if rec.Code != http.StatusUnprocessableEntity {
		t.Fatalf("status: got %d, want 422, body=%s", rec.Code, rec.Body.String())
	}
}

func TestHandleRoute_SavesHistoryWithUserID(t *testing.T) {
	store := &fakeStore{}
	h := newTestHandler(store)

	rec := doRequest(t, h, http.MethodPost, "/route",
		RouteRequest{From: "a", To: "c", Mode: "fast"},
		map[string]string{"X-User-ID": "12345"})

	if rec.Code != http.StatusOK {
		t.Fatalf("status: got %d, want 200", rec.Code)
	}
	if store.count() != 1 {
		t.Fatalf("history: got %d records, want 1", store.count())
	}
	got := store.reqs[0]
	if got.UserID != 12345 || got.FromStation != "a" || got.ToStation != "c" || got.Mode != "fast" {
		t.Fatalf("unexpected record: %+v", got)
	}
}

func TestHandleRoute_NoUserID_NoHistory(t *testing.T) {
	store := &fakeStore{}
	h := newTestHandler(store)

	rec := doRequest(t, h, http.MethodPost, "/route",
		RouteRequest{From: "a", To: "c"}, nil)

	if rec.Code != http.StatusOK {
		t.Fatalf("status: got %d, want 200", rec.Code)
	}
	if store.count() != 0 {
		t.Fatalf("history: got %d records, want 0 (anonymous)", store.count())
	}
}

func TestHandleRoute_SaveErrorDoesNotBreakResponse(t *testing.T) {
	store := &fakeStore{err: errors.New("db down")}
	h := newTestHandler(store)

	rec := doRequest(t, h, http.MethodPost, "/route",
		RouteRequest{From: "a", To: "c"},
		map[string]string{"X-User-ID": "1"})

	if rec.Code != http.StatusOK {
		t.Fatalf("status: got %d, want 200 (save error must not break response)", rec.Code)
	}
}

func TestHandleRoute_InvalidUserIDHeader(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodPost, "/route",
		RouteRequest{From: "a", To: "c"},
		map[string]string{"X-User-ID": "not-a-number"})

	if rec.Code != http.StatusBadRequest {
		t.Fatalf("status: got %d, want 400 (from middleware)", rec.Code)
	}
}

func TestHandleStations(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodGet, "/stations", nil, nil)

	if rec.Code != http.StatusOK {
		t.Fatalf("status: got %d, want 200", rec.Code)
	}
	stations := decodeJSON[[]StationView](t, rec)
	if len(stations) != 4 {
		t.Fatalf("stations: got %d, want 4", len(stations))
	}
	for _, s := range stations {
		if s.ID == "" || s.Name == "" || s.LineID == "" {
			t.Fatalf("station has empty fields: %+v", s)
		}
	}
}

func TestHandleHistory_Empty(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodGet, "/history/42", nil, nil)

	if rec.Code != http.StatusOK {
		t.Fatalf("status: got %d, want 200", rec.Code)
	}
	// Важно: пустой список должен быть [], а не null.
	if body := strings.TrimSpace(rec.Body.String()); body != "[]" {
		t.Fatalf("body: got %q, want []", body)
	}
}

func TestHandleHistory_ReturnsRecords(t *testing.T) {
	store := &fakeStore{}
	h := newTestHandler(store)

	_ = store.SaveRequest(context.Background(), 7, "a", "b", "fast")
	_ = store.SaveRequest(context.Background(), 7, "b", "c", "fewest_transfers")
	_ = store.SaveRequest(context.Background(), 8, "a", "c", "fast")

	rec := doRequest(t, h, http.MethodGet, "/history/7", nil, nil)
	if rec.Code != http.StatusOK {
		t.Fatalf("status: got %d, want 200", rec.Code)
	}
	items := decodeJSON[[]HistoryItem](t, rec)
	if len(items) != 2 {
		t.Fatalf("items: got %d, want 2", len(items))
	}
	if items[0].FromStation != "b" || items[0].ToStation != "c" {
		t.Fatalf("first item: got %+v, want b→c", items[0])
	}
	if items[1].FromStation != "a" || items[1].ToStation != "b" {
		t.Fatalf("second item: got %+v, want a→b", items[1])
	}
}

func TestHandleHistory_InvalidUserID(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodGet, "/history/not-a-number", nil, nil)
	if rec.Code != http.StatusBadRequest {
		t.Fatalf("status: got %d, want 400", rec.Code)
	}
}

func TestHandleHistory_InvalidLimit(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	for _, limit := range []string{"0", "-1", "101", "abc"} {
		t.Run(limit, func(t *testing.T) {
			rec := doRequest(t, h, http.MethodGet, "/history/42?limit="+limit, nil, nil)
			if rec.Code != http.StatusBadRequest {
				t.Fatalf("status: got %d, want 400, body=%s", rec.Code, rec.Body.String())
			}
		})
	}
}

func TestHandleHistory_LimitApplied(t *testing.T) {
	store := &fakeStore{}
	h := newTestHandler(store)
	for i := 0; i < 5; i++ {
		_ = store.SaveRequest(context.Background(), 7, "a", "b", "fast")
	}

	rec := doRequest(t, h, http.MethodGet, "/history/7?limit=2", nil, nil)
	if rec.Code != http.StatusOK {
		t.Fatalf("status: got %d, want 200", rec.Code)
	}
	items := decodeJSON[[]HistoryItem](t, rec)
	if len(items) != 2 {
		t.Fatalf("items: got %d, want 2", len(items))
	}
}

func TestHandleHistory_ForbiddenAnotherUser(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodGet, "/history/7", nil,
		map[string]string{"X-User-ID": "8"})

	if rec.Code != http.StatusForbidden {
		t.Fatalf("status: got %d, want 403", rec.Code)
	}
}

func TestHandleHistory_SameUserOK(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodGet, "/history/7", nil,
		map[string]string{"X-User-ID": "7"})

	if rec.Code != http.StatusOK {
		t.Fatalf("status: got %d, want 200", rec.Code)
	}
}

func TestHandleHealth_OK(t *testing.T) {
	h := newTestHandler(&fakeStore{})

	rec := doRequest(t, h, http.MethodGet, "/healthz", nil, nil)
	if rec.Code != http.StatusOK {
		t.Fatalf("status: got %d, want 200", rec.Code)
	}
	var body map[string]string
	_ = json.Unmarshal(rec.Body.Bytes(), &body)
	if body["status"] != "ok" {
		t.Fatalf("body: got %+v, want status=ok", body)
	}
}

func TestHandleHealth_DBDown(t *testing.T) {
	h := newTestHandler(&fakeStore{err: errors.New("db down")})

	rec := doRequest(t, h, http.MethodGet, "/healthz", nil, nil)
	if rec.Code != http.StatusServiceUnavailable {
		t.Fatalf("status: got %d, want 503", rec.Code)
	}
}

func TestEndToEnd_RouteThenHistory(t *testing.T) {
	store := &fakeStore{}
	h := newTestHandler(store)

	rec := doRequest(t, h, http.MethodPost, "/route",
		RouteRequest{From: "a", To: "c", Mode: "fast"},
		map[string]string{"X-User-ID": "777"})
	if rec.Code != http.StatusOK {
		t.Fatalf("POST /route status: %d", rec.Code)
	}

	rec = doRequest(t, h, http.MethodGet, "/history/777", nil,
		map[string]string{"X-User-ID": "777"})
	if rec.Code != http.StatusOK {
		t.Fatalf("GET /history status: %d, body=%s", rec.Code, rec.Body.String())
	}
	items := decodeJSON[[]HistoryItem](t, rec)
	if len(items) != 1 {
		t.Fatalf("history: got %d, want 1", len(items))
	}
	if items[0].FromStation != "a" || items[0].ToStation != "c" || items[0].Mode != "fast" {
		t.Fatalf("unexpected history item: %+v", items[0])
	}
}

type panickingHandler struct{}

func (panickingHandler) ServeHTTP(http.ResponseWriter, *http.Request) {
	panic("boom")
}

func TestRecovery_CatchesPanic(t *testing.T) {
	h := Logging(Recovery(UserID(panickingHandler{})))

	req := httptest.NewRequest(http.MethodGet, "/boom", nil)
	rec := httptest.NewRecorder()

	h.ServeHTTP(rec, req)

	if rec.Code != http.StatusInternalServerError {
		t.Fatalf("status: got %d, want 500", rec.Code)
	}
}
