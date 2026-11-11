package storage

import (
	"fmt"

	"github.com/nastasyaR10/projects/metro-route/internal/models"
)

func BuildGraph(schema *models.MetroSchema) (*models.Graph, error) {
	g := &models.Graph{
		AdjacencyList: make(map[string][]models.GraphEdge),
		Stations:      make(map[string]models.Station),
	}

	for _, s := range schema.Stations {
		if _, exists := g.Stations[s.ID]; exists {
			return nil, fmt.Errorf("duplicate station id: %q", s.ID)
		}
		g.Stations[s.ID] = s
	}

	for _, e := range schema.Edges {
		if err := validateRefs(g, e.FromID, e.ToID); err != nil {
			return nil, err
		}
		g.AdjacencyList[e.FromID] = append(g.AdjacencyList[e.FromID], models.GraphEdge{
			To:         e.ToID,
			WeightMin:  e.TravelTimeMin,
			IsTransfer: false,
		})
		g.AdjacencyList[e.ToID] = append(g.AdjacencyList[e.ToID], models.GraphEdge{
			To:         e.FromID,
			WeightMin:  e.TravelTimeMin,
			IsTransfer: false,
		})
	}

	for _, t := range schema.Transfers {
		if err := validateRefs(g, t.FromStationID, t.ToStationID); err != nil {
			return nil, err
		}
		g.AdjacencyList[t.FromStationID] = append(g.AdjacencyList[t.FromStationID], models.GraphEdge{
			To:         t.ToStationID,
			WeightMin:  t.TransferTimeMin,
			IsTransfer: true,
		})
		g.AdjacencyList[t.ToStationID] = append(g.AdjacencyList[t.ToStationID], models.GraphEdge{
			To:         t.FromStationID,
			WeightMin:  t.TransferTimeMin,
			IsTransfer: true,
		})
	}

	return g, nil
}

func validateRefs(g *models.Graph, ids ...string) error {
	for _, id := range ids {
		if _, ok := g.Stations[id]; !ok {
			return fmt.Errorf("edge references unknown station: %q", id)
		}
	}
	return nil
}
