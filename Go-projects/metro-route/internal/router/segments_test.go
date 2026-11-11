package router

import (
	"testing"

	"github.com/nastasyaR10/projects/metro-route/internal/models"
)

func tinyGraph() *models.Graph {
	return &models.Graph{
		Stations: map[string]models.Station{
			"a": {ID: "a", Name: "A"},
			"b": {ID: "b", Name: "B"},
			"c": {ID: "c", Name: "C"},
		},
		AdjacencyList: map[string][]models.GraphEdge{},
	}
}

func TestBuildSegments_Empty(t *testing.T) {
	g := tinyGraph()
	segs := buildSegments(g, nil, nil)
	if segs != nil {
		t.Fatalf("expected nil, got %+v", segs)
	}
}

func TestBuildSegments_SingleStation(t *testing.T) {
	g := tinyGraph()
	segs := buildSegments(g, []string{"a"}, map[string]models.GraphEdge{})
	if len(segs) != 1 {
		t.Fatalf("expected 1 segment, got %d", len(segs))
	}
	if len(segs[0].Stations) != 1 || segs[0].Stations[0].ID != "a" {
		t.Fatalf("unexpected stations: %+v", segs[0].Stations)
	}
	if segs[0].TimeMin != 0 {
		t.Fatalf("expected 0 min, got %d", segs[0].TimeMin)
	}
	if segs[0].IsTransfer {
		t.Fatal("should not be transfer")
	}
}

func TestBuildSegments_OneLine(t *testing.T) {
	g := tinyGraph()
	prevEdge := map[string]models.GraphEdge{
		"b": {To: "b", WeightMin: 3},
		"c": {To: "c", WeightMin: 4},
	}
	segs := buildSegments(g, []string{"a", "b", "c"}, prevEdge)
	if len(segs) != 1 {
		t.Fatalf("expected 1 segment, got %d", len(segs))
	}
	if len(segs[0].Stations) != 3 {
		t.Fatalf("expected 3 stations, got %d", len(segs[0].Stations))
	}
	if segs[0].TimeMin != 7 {
		t.Fatalf("expected 7 min, got %d", segs[0].TimeMin)
	}
}

func TestBuildSegments_WithTransfer(t *testing.T) {
	g := tinyGraph()
	prevEdge := map[string]models.GraphEdge{
		"b": {To: "b", WeightMin: 3},
		"c": {To: "c", WeightMin: 5, IsTransfer: true},
	}
	segs := buildSegments(g, []string{"a", "b", "c"}, prevEdge)

	if len(segs) != 3 {
		t.Fatalf("expected 3 segments, got %d", len(segs))
	}
	if segs[0].IsTransfer {
		t.Fatal("segment 0 should not be transfer")
	}
	if segs[0].TimeMin != 3 {
		t.Fatalf("segment 0 time: got %d, want 3", segs[0].TimeMin)
	}
	if !segs[1].IsTransfer {
		t.Fatal("segment 1 should be transfer")
	}
	if segs[1].TimeMin != 5 {
		t.Fatalf("segment 1 time: got %d, want 5", segs[1].TimeMin)
	}
	if segs[2].IsTransfer {
		t.Fatal("segment 2 should not be transfer")
	}
}

func TestBuildSegments_TwoTransfersInARow(t *testing.T) {
	g := tinyGraph()
	g.Stations["d"] = models.Station{ID: "d", Name: "D"}
	prevEdge := map[string]models.GraphEdge{
		"b": {To: "b", WeightMin: 2},
		"c": {To: "c", WeightMin: 3, IsTransfer: true},
		"d": {To: "d", WeightMin: 4, IsTransfer: true},
	}
	segs := buildSegments(g, []string{"a", "b", "c", "d"}, prevEdge)
	if len(segs) != 5 {
		t.Fatalf("expected 5 segments, got %d", len(segs))
	}
}

func TestBuildSegments_MissingPrevEdge(t *testing.T) {
	g := tinyGraph()
	segs := buildSegments(g, []string{"a", "b"}, map[string]models.GraphEdge{})
	if len(segs) != 1 {
		t.Fatalf("expected 1 segment, got %d", len(segs))
	}
	if len(segs[0].Stations) != 2 {
		t.Fatalf("expected 2 stations, got %d", len(segs[0].Stations))
	}
}

func TestBuildSegments_SumMatchesTotal(t *testing.T) {
	g := tinyGraph()
	prevEdge := map[string]models.GraphEdge{
		"b": {To: "b", WeightMin: 3},
		"c": {To: "c", WeightMin: 5, IsTransfer: true},
	}
	segs := buildSegments(g, []string{"a", "b", "c"}, prevEdge)
	sum := 0
	for _, s := range segs {
		sum += s.TimeMin
	}
	if sum != 8 {
		t.Fatalf("sum of segments = %d, want 8", sum)
	}
}
