package router

import (
	"context"
	"errors"
	"testing"

	"github.com/nastasyaR10/projects/metro-route/internal/models"
)

func TestShortestPath_SameStation(t *testing.T) {
	g := &models.Graph{
		Stations: map[string]models.Station{
			"a": {ID: "a", Name: "A"},
		},
		AdjacencyList: map[string][]models.GraphEdge{"a": {}},
	}

	r, err := ShortestPath(context.Background(), g, "a", "a", models.ModeFastest)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if r.TotalTimeMin != 0 {
		t.Fatalf("expected 0 min, got %d", r.TotalTimeMin)
	}
	if len(r.Path) != 1 {
		t.Fatalf("expected 1 station in path, got %d", len(r.Path))
	}
	if len(r.Segments) != 1 {
		t.Fatalf("expected 1 segment, got %d", len(r.Segments))
	}
	if r.Segments[0].IsTransfer {
		t.Fatal("segment should not be transfer")
	}
}

func TestShortestPath_UnknownStation(t *testing.T) {
	g := &models.Graph{
		Stations: map[string]models.Station{
			"a": {ID: "a", Name: "A"},
		},
		AdjacencyList: map[string][]models.GraphEdge{"a": {}},
	}

	if _, err := ShortestPath(context.Background(), g, "b", "a", models.ModeFastest); !errors.Is(err, ErrUnknownStation) {
		t.Fatalf("from=ghost: want ErrUnknownStation, got %v", err)
	}
	if _, err := ShortestPath(context.Background(), g, "a", "b", models.ModeFastest); !errors.Is(err, ErrUnknownStation) {
		t.Fatalf("to=ghost: want ErrUnknownStation, got %v", err)
	}
}

func TestShortestPath_NoPath(t *testing.T) {
	g := &models.Graph{
		AdjacencyList: map[string][]models.GraphEdge{
			"a": {},
			"b": {},
		},
		Stations: map[string]models.Station{
			"a": {ID: "a", Name: "A"},
			"b": {ID: "b", Name: "B"},
		},
	}

	if _, err := ShortestPath(context.Background(), g, "a", "b", models.ModeFastest); !errors.Is(err, ErrNoPath) {
		t.Fatalf("want ErrNoPath, got %v", err)
	}
}

func TestShortestPath_FastestPrefersFewerTransfersOnTie(t *testing.T) {
	g := &models.Graph{
		AdjacencyList: map[string][]models.GraphEdge{
			"A": {{To: "B", WeightMin: 3}, {To: "C", WeightMin: 3}},
			"B": {{To: "D", WeightMin: 3}},
			"C": {{To: "D", WeightMin: 3, IsTransfer: true}},
			"D": {},
		},
		Stations: map[string]models.Station{
			"A": {ID: "A"}, "B": {ID: "B"}, "C": {ID: "C"}, "D": {ID: "D"},
		},
	}

	r, err := ShortestPath(context.Background(), g, "A", "D", models.ModeFastest)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if r.TotalTimeMin != 6 {
		t.Fatalf("expected 6 min, got %d", r.TotalTimeMin)
	}
	if r.TransfersCount != 0 {
		t.Fatalf("expected 0 transfers on tie, got %d", r.TransfersCount)
	}
}

func TestShortestPath_FewestTransfersPrefersFasterOnTie(t *testing.T) {
	g := &models.Graph{
		AdjacencyList: map[string][]models.GraphEdge{
			"A": {{To: "B", WeightMin: 2}, {To: "C", WeightMin: 5}},
			"B": {{To: "D", WeightMin: 2}},
			"C": {{To: "D", WeightMin: 5}},
			"D": {},
		},
		Stations: map[string]models.Station{
			"A": {ID: "A"}, "B": {ID: "B"}, "C": {ID: "C"}, "D": {ID: "D"},
		},
	}

	r, err := ShortestPath(context.Background(), g, "A", "D", models.ModeFewestTransfers)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if r.TransfersCount != 0 {
		t.Fatalf("expected 0 transfers, got %d", r.TransfersCount)
	}
	if r.TotalTimeMin != 4 {
		t.Fatalf("expected 4 min on tie, got %d", r.TotalTimeMin)
	}
}
