package router

import (
	"context"
	"testing"

	"github.com/nastasyaR10/projects/metro-route/internal/models"
	"github.com/nastasyaR10/projects/metro-route/internal/storage"
)

func mustLoadRealGraph(t *testing.T) *models.Graph {
	t.Helper()
	schema, err := storage.LoadSchema("../../data/metro.json")
	if err != nil {
		t.Fatalf("load schema: %v", err)
	}
	g, err := storage.BuildGraph(schema)
	if err != nil {
		t.Fatalf("build graph: %v", err)
	}
	return g
}

func checkInvariants(t *testing.T, r *models.Route, from, to string) {
	t.Helper()
	if len(r.Path) == 0 {
		t.Fatal("empty path")
	}
	if r.Path[0].ID != from {
		t.Fatalf("path starts at %q, want %q", r.Path[0].ID, from)
	}
	if r.Path[len(r.Path)-1].ID != to {
		t.Fatalf("path ends at %q, want %q", r.Path[len(r.Path)-1].ID, to)
	}

	sum := 0
	transfers := 0
	for _, seg := range r.Segments {
		sum += seg.TimeMin
		if seg.IsTransfer {
			transfers++
		}
	}
	if sum != r.TotalTimeMin {
		t.Fatalf("sum of segment times (%d) != total (%d)", sum, r.TotalTimeMin)
	}
	if transfers != r.TransfersCount {
		t.Fatalf("counted transfers (%d) != reported (%d)", transfers, r.TransfersCount)
	}
}

func TestRealData_ParkKulturyToKomsomolskayaK(t *testing.T) {
	g := mustLoadRealGraph(t)

	for _, mode := range []models.RouteMode{models.ModeFastest, models.ModeFewestTransfers} {
		r, err := ShortestPath(context.Background(), g, "park_kultury_s", "komsomolskaya_k", mode)
		if err != nil {
			t.Fatalf("mode %v: %v", mode, err)
		}
		checkInvariants(t, r, "park_kultury_s", "komsomolskaya_k")
		if r.TotalTimeMin != 10 {
			t.Errorf("mode %v: expected 10 min, got %d", mode, r.TotalTimeMin)
		}
		if r.TransfersCount != 1 {
			t.Errorf("mode %v: expected 1 transfer, got %d", mode, r.TransfersCount)
		}
	}
}

func TestRealData_ParkKulturyToKomsomolskayaS(t *testing.T) {
	g := mustLoadRealGraph(t)

	for _, mode := range []models.RouteMode{models.ModeFastest, models.ModeFewestTransfers} {
		r, err := ShortestPath(context.Background(), g, "park_kultury_s", "komsomolskaya_s", mode)
		if err != nil {
			t.Fatalf("mode %v: %v", mode, err)
		}
		checkInvariants(t, r, "park_kultury_s", "komsomolskaya_s")
		if r.TotalTimeMin != 8 {
			t.Errorf("mode %v: expected 8 min, got %d", mode, r.TotalTimeMin)
		}
		if r.TransfersCount != 0 {
			t.Errorf("mode %v: expected 0 transfers, got %d", mode, r.TransfersCount)
		}
	}
}

func TestRealData_ParkKulturyToPaveletskaya_Fastest(t *testing.T) {
	g := mustLoadRealGraph(t)

	r, err := ShortestPath(context.Background(), g, "park_kultury_s", "paveletskaya_z", models.ModeFastest)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	checkInvariants(t, r, "park_kultury_s", "paveletskaya_z")
	if r.TotalTimeMin != 11 {
		t.Errorf("expected 11 min, got %d", r.TotalTimeMin)
	}
	if r.TransfersCount != 2 {
		t.Errorf("expected 2 transfers, got %d", r.TransfersCount)
	}
}

func TestRealData_ParkKulturyToPaveletskaya_FewestTransfers(t *testing.T) {
	g := mustLoadRealGraph(t)

	r, err := ShortestPath(context.Background(), g, "park_kultury_s", "paveletskaya_z", models.ModeFewestTransfers)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	checkInvariants(t, r, "park_kultury_s", "paveletskaya_z")
	if r.TotalTimeMin != 14 {
		t.Errorf("expected 14 min, got %d", r.TotalTimeMin)
	}
	if r.TransfersCount != 1 {
		t.Errorf("expected 1 transfer, got %d", r.TransfersCount)
	}
}

func TestRealData_ParkKulturyToBelorusskaya_Fastest(t *testing.T) {
	g := mustLoadRealGraph(t)

	r, err := ShortestPath(context.Background(), g, "park_kultury_s", "belorusskaya_z", models.ModeFastest)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	checkInvariants(t, r, "park_kultury_s", "belorusskaya_z")
	if r.TotalTimeMin != 12 {
		t.Errorf("expected 12 min, got %d", r.TotalTimeMin)
	}
	if r.TransfersCount != 2 {
		t.Errorf("expected 2 transfers, got %d", r.TransfersCount)
	}
}

func TestRealData_ParkKulturyToBelorusskaya_FewestTransfers(t *testing.T) {
	g := mustLoadRealGraph(t)

	r, err := ShortestPath(context.Background(), g, "park_kultury_s", "belorusskaya_z", models.ModeFewestTransfers)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	checkInvariants(t, r, "park_kultury_s", "belorusskaya_z")
	if r.TotalTimeMin != 15 {
		t.Errorf("expected 15 min, got %d", r.TotalTimeMin)
	}
	if r.TransfersCount != 1 {
		t.Errorf("expected 1 transfer, got %d", r.TransfersCount)
	}
}

func TestRealData_SokolnikiToBelorusskaya_TieBreak(t *testing.T) {
	g := mustLoadRealGraph(t)

	for _, mode := range []models.RouteMode{models.ModeFastest, models.ModeFewestTransfers} {
		r, err := ShortestPath(context.Background(), g, "sokolniki_s", "belorusskaya_z", mode)
		if err != nil {
			t.Fatalf("mode %v: %v", mode, err)
		}
		checkInvariants(t, r, "sokolniki_s", "belorusskaya_z")
		if r.TotalTimeMin != 17 {
			t.Errorf("mode %v: expected 17 min, got %d", mode, r.TotalTimeMin)
		}
		if r.TransfersCount != 1 {
			t.Errorf("mode %v: expected 1 transfer, got %d", mode, r.TransfersCount)
		}
	}
}

func TestRealData_SokolnikiToMayakovskaya_SameRoute(t *testing.T) {
	g := mustLoadRealGraph(t)

	for _, mode := range []models.RouteMode{models.ModeFastest, models.ModeFewestTransfers} {
		r, err := ShortestPath(context.Background(), g, "sokolniki_s", "mayakovskaya", mode)
		if err != nil {
			t.Fatalf("mode %v: %v", mode, err)
		}
		checkInvariants(t, r, "sokolniki_s", "mayakovskaya")
		if r.TotalTimeMin != 15 {
			t.Errorf("mode %v: expected 15 min, got %d", mode, r.TotalTimeMin)
		}
		if r.TransfersCount != 1 {
			t.Errorf("mode %v: expected 1 transfer, got %d", mode, r.TransfersCount)
		}
	}
}

func TestRealData_KRLAlongLine(t *testing.T) {
	g := mustLoadRealGraph(t)

	for _, mode := range []models.RouteMode{models.ModeFastest, models.ModeFewestTransfers} {
		r, err := ShortestPath(context.Background(), g, "oktyabrskaya_krl", "prospekt_mira_krl", mode)
		if err != nil {
			t.Fatalf("mode %v: %v", mode, err)
		}
		checkInvariants(t, r, "oktyabrskaya_krl", "prospekt_mira_krl")
		if r.TotalTimeMin != 8 {
			t.Errorf("mode %v: expected 8 min, got %d", mode, r.TotalTimeMin)
		}
		if r.TransfersCount != 0 {
			t.Errorf("mode %v: expected 0 transfers, got %d", mode, r.TransfersCount)
		}
	}
}
