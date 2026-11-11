package storage

import (
	"strings"
	"testing"

	"github.com/nastasyaR10/projects/metro-route/internal/models"
)

func TestBuildGraph(t *testing.T) {
	schema, err := LoadSchema("../../data/metro.json")
	if err != nil {
		t.Fatalf("load schema: %v", err)
	}

	g, err := BuildGraph(schema)
	if err != nil {
		t.Fatalf("build graph: %v", err)
	}

	if len(g.Stations) != 70 {
		t.Fatalf("expected 70 stations, got %d", len(g.Stations))
	}
	if len(g.AdjacencyList) != 70 {
		t.Fatalf("expected 70 adjacency entries, got %d", len(g.AdjacencyList))
	}

	neighbors := g.AdjacencyList["park_kultury_s"]
	if len(neighbors) != 3 {
		t.Fatalf("expected 3 neighbors for park_kultury_s, got %d", len(neighbors))
	}

	if len(g.AdjacencyList["kropotkinskaya"]) == 0 {
		t.Fatal("reverse edge from kropotkinskaya not built")
	}

	ohotny := g.AdjacencyList["ohotny_ryad"]
	if len(ohotny) != 3 {
		t.Fatalf("expected 3 neighbors for ohotny_ryad, got %d", len(ohotny))
	}
}

func TestBuildGraph_AllTransfersBidirectional(t *testing.T) {
	schema, err := LoadSchema("../../data/metro.json")
	if err != nil {
		t.Fatalf("load schema: %v", err)
	}
	g, err := BuildGraph(schema)
	if err != nil {
		t.Fatalf("build graph: %v", err)
	}

	for _, tr := range schema.Transfers {
		foundForward := false
		for _, e := range g.AdjacencyList[tr.FromStationID] {
			if e.To == tr.ToStationID {
				if !e.IsTransfer {
					t.Errorf("%s->%s: not marked IsTransfer", tr.FromStationID, tr.ToStationID)
				}
				if e.WeightMin != tr.TransferTimeMin {
					t.Errorf("%s->%s: weight %d, want %d", tr.FromStationID, tr.ToStationID, e.WeightMin, tr.TransferTimeMin)
				}
				foundForward = true
			}
		}
		if !foundForward {
			t.Errorf("transfer edge %s->%s not found", tr.FromStationID, tr.ToStationID)
		}

		foundBackward := false
		for _, e := range g.AdjacencyList[tr.ToStationID] {
			if e.To == tr.FromStationID {
				if !e.IsTransfer {
					t.Errorf("%s->%s (reverse): not marked IsTransfer", tr.ToStationID, tr.FromStationID)
				}
				if e.WeightMin != tr.TransferTimeMin {
					t.Errorf("%s->%s (reverse): weight %d, want %d", tr.ToStationID, tr.FromStationID, e.WeightMin, tr.TransferTimeMin)
				}
				foundBackward = true
			}
		}
		if !foundBackward {
			t.Errorf("transfer edge (reverse) %s->%s not found", tr.ToStationID, tr.FromStationID)
		}
	}
}

func TestBuildGraph_TransferWeights(t *testing.T) {
	schema, err := LoadSchema("../../data/metro.json")
	if err != nil {
		t.Fatalf("load schema: %v", err)
	}
	g, err := BuildGraph(schema)
	if err != nil {
		t.Fatalf("build graph: %v", err)
	}

	cases := []struct {
		from, to string
		want     int
	}{
		{"park_kultury_s", "park_kultury_k", 2},
		{"komsomolskaya_s", "komsomolskaya_k", 2},
		{"ohotny_ryad", "teatralnaya", 6},
		{"novokuznetskaya", "tretyakovskaya_krl", 3},
		{"belorusskaya_z", "belorusskaya_k", 4},
		{"paveletskaya_z", "paveletskaya_k", 4},
		{"oktyabrskaya_k", "oktyabrskaya_krl", 3},
		{"chistye_prudy", "turgenevskaya", 3},
		{"prospekt_mira_k", "prospekt_mira_krl", 3},
	}

	for _, c := range cases {
		found := false
		for _, e := range g.AdjacencyList[c.from] {
			if e.To == c.to {
				if e.WeightMin != c.want {
					t.Errorf("%s->%s: weight %d, want %d", c.from, c.to, e.WeightMin, c.want)
				}
				if !e.IsTransfer {
					t.Errorf("%s->%s: not marked IsTransfer", c.from, c.to)
				}
				found = true
			}
		}
		if !found {
			t.Errorf("transfer edge %s->%s not found", c.from, c.to)
		}
	}
}

func TestBuildGraph_EdgeWeights(t *testing.T) {
	schema, err := LoadSchema("../../data/metro.json")
	if err != nil {
		t.Fatalf("load schema: %v", err)
	}
	g, err := BuildGraph(schema)
	if err != nil {
		t.Fatalf("build graph: %v", err)
	}

	cases := []struct {
		from, to string
		want     int
	}{
		{"vorobyovy_gory", "universitet", 5},
		{"prospekt_mira_k", "novoslobodskaya", 5},
		{"krasnogvardeyskaya", "alma_atinskaya", 4},
		{"oktyabrskaya_krl", "tretyakovskaya_krl", 2},
		{"sokolniki_s", "krasnoselskaya", 1},
		{"park_kultury_s", "frunzenskaya", 2},
	}

	for _, c := range cases {
		found := false
		for _, e := range g.AdjacencyList[c.from] {
			if e.To == c.to {
				if e.WeightMin != c.want {
					t.Errorf("%s->%s: weight %d, want %d", c.from, c.to, e.WeightMin, c.want)
				}
				if e.IsTransfer {
					t.Errorf("%s->%s: should not be transfer", c.from, c.to)
				}
				found = true
			}
		}
		if !found {
			t.Errorf("edge %s->%s not found", c.from, c.to)
		}
	}
}

func TestBuildGraph_DuplicateStationID(t *testing.T) {
	schema := &models.MetroSchema{
		Stations: []models.Station{
			{ID: "a", Name: "A"},
			{ID: "a", Name: "B"},
		},
	}
	_, err := BuildGraph(schema)
	if err == nil {
		t.Fatal("expected duplicate id error")
	}
	if !strings.Contains(err.Error(), "duplicate station id") {
		t.Fatalf("unexpected error: %v", err)
	}
}

func TestBuildGraph_EdgeUnknownFrom(t *testing.T) {
	schema := &models.MetroSchema{
		Stations: []models.Station{{ID: "a", Name: "A"}},
		Edges:    []models.Edge{{FromID: "b", ToID: "a", TravelTimeMin: 1}},
	}
	_, err := BuildGraph(schema)
	if err == nil {
		t.Fatal("expected unknown from error")
	}
}

func TestBuildGraph_EdgeUnknownTo(t *testing.T) {
	schema := &models.MetroSchema{
		Stations: []models.Station{{ID: "a", Name: "A"}},
		Edges:    []models.Edge{{FromID: "a", ToID: "b", TravelTimeMin: 1}},
	}
	_, err := BuildGraph(schema)
	if err == nil {
		t.Fatal("expected unknown to error")
	}
}

func TestBuildGraph_TransferUnknownFrom(t *testing.T) {
	schema := &models.MetroSchema{
		Stations:  []models.Station{{ID: "a", Name: "A"}},
		Transfers: []models.Transfer{{FromStationID: "b", ToStationID: "a", TransferTimeMin: 1}},
	}
	_, err := BuildGraph(schema)
	if err == nil {
		t.Fatal("expected unknown transfer from error")
	}
}

func TestBuildGraph_TransferUnknownTo(t *testing.T) {
	schema := &models.MetroSchema{
		Stations:  []models.Station{{ID: "a", Name: "A"}},
		Transfers: []models.Transfer{{FromStationID: "a", ToStationID: "b", TransferTimeMin: 1}},
	}
	_, err := BuildGraph(schema)
	if err == nil {
		t.Fatal("expected unknown transfer to error")
	}
}
