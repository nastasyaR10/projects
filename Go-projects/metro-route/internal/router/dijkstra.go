package router

import (
	"container/heap"
	"context"
	"errors"
	"fmt"

	"github.com/nastasyaR10/projects/metro-route/internal/models"
)

var (
	ErrUnknownStation = errors.New("unknown station")
	ErrNoPath         = errors.New("no path")
)

type state struct {
	time      int
	transfers int
}

type item struct {
	stationID string
	st        state
	index     int
}

type priorityQueue struct {
	items []*item
	mode  models.RouteMode
}

func (pq priorityQueue) Len() int {
	return len(pq.items)
}

func (pq priorityQueue) Less(i, j int) bool {
	return less(pq.mode, pq.items[i].st, pq.items[j].st)
}

func (pq priorityQueue) Swap(i, j int) {
	pq.items[i], pq.items[j] = pq.items[j], pq.items[i]
	pq.items[i].index = i
	pq.items[j].index = j
}

func (pq *priorityQueue) Push(x any) {
	it := x.(*item)
	it.index = len(pq.items)
	pq.items = append(pq.items, it)
}

func (pq *priorityQueue) Pop() any {
	old := pq.items
	n := len(old)
	it := old[n-1]
	old[n-1] = nil
	it.index = -1
	pq.items = old[:n-1]
	return it
}

func less(mode models.RouteMode, a, b state) bool {
	switch mode {
	case models.ModeFewestTransfers:
		if a.transfers != b.transfers {
			return a.transfers < b.transfers
		}
		return a.time < b.time
	case models.ModeFastest:
		if a.time != b.time {
			return a.time < b.time
		}
		return a.transfers < b.transfers
	default: //ModeFastest
		if a.time != b.time {
			return a.time < b.time
		}
		return a.transfers < b.transfers
	}
}

func ShortestPath(ctx context.Context, g *models.Graph, from, to string, mode models.RouteMode) (*models.Route, error) {
	if err := ctx.Err(); err != nil {
		return nil, err
	}
	if _, ok := g.Stations[from]; !ok {
		return nil, fmt.Errorf("%w: %q", ErrUnknownStation, from)
	}
	if _, ok := g.Stations[to]; !ok {
		return nil, fmt.Errorf("%w: %q", ErrUnknownStation, to)
	}
	if from == to {
		return &models.Route{
			Path:         []models.Station{g.Stations[from]},
			TotalTimeMin: 0,
			Mode:         mode,
			Segments: []models.RouteSegment{
				{
					Stations:   []models.Station{g.Stations[from]},
					TimeMin:    0,
					IsTransfer: false,
				},
			},
		}, nil
	}

	dist := make(map[string]state, len(g.Stations))
	prev := make(map[string]string, len(g.Stations))
	prevEdge := make(map[string]models.GraphEdge, len(g.Stations))
	checked := make(map[string]bool, len(g.Stations))

	const inf = int(^uint(0) >> 1)
	for id := range g.Stations {
		dist[id] = state{time: inf, transfers: inf}
	}
	dist[from] = state{time: 0, transfers: 0}

	pq := &priorityQueue{
		items: []*item{{stationID: from, st: state{time: 0, transfers: 0}}},
		mode:  mode,
	}
	heap.Init(pq)

	for pq.Len() > 0 {
		if err := ctx.Err(); err != nil {
			return nil, err
		}
		cur := heap.Pop(pq).(*item)
		if checked[cur.stationID] {
			continue
		}
		checked[cur.stationID] = true

		if cur.stationID == to {
			break
		}

		for _, e := range g.AdjacencyList[cur.stationID] {
			if checked[e.To] {
				continue
			}
			next := state{
				time:      cur.st.time + e.WeightMin,
				transfers: cur.st.transfers + boolToInt(e.IsTransfer),
			}
			if less(mode, next, dist[e.To]) {
				dist[e.To] = next
				prev[e.To] = cur.stationID
				prevEdge[e.To] = e
				heap.Push(pq, &item{stationID: e.To, st: next})
			}
		}
	}

	if dist[to].time == inf {
		return nil, fmt.Errorf("%w: %q -> %q", ErrNoPath, from, to)
	}

	var ids []string
	for at := to; ; at = prev[at] {
		ids = append(ids, at)
		if at == from {
			break
		}
	}
	for i, j := 0, len(ids)-1; i < j; i, j = i+1, j-1 {
		ids[i], ids[j] = ids[j], ids[i]
	}

	path := make([]models.Station, 0, len(ids))
	for _, id := range ids {
		path = append(path, g.Stations[id])
	}

	segments := buildSegments(g, ids, prevEdge)
	transfers := 0
	for _, seg := range segments {
		if seg.IsTransfer {
			transfers++
		}
	}

	return &models.Route{
		Path:           path,
		TotalTimeMin:   dist[to].time,
		TransfersCount: transfers,
		Segments:       segments,
		Mode:           mode,
	}, nil
}

func boolToInt(b bool) int {
	if b {
		return 1
	}
	return 0
}

func buildSegments(g *models.Graph, ids []string, prevEdge map[string]models.GraphEdge) []models.RouteSegment {
	if len(ids) == 0 {
		return nil
	}
	if len(ids) == 1 {
		return []models.RouteSegment{
			{Stations: []models.Station{g.Stations[ids[0]]}},
		}
	}

	var segments []models.RouteSegment
	cur := models.RouteSegment{
		Stations: []models.Station{g.Stations[ids[0]]},
	}

	for i := 0; i < len(ids)-1; i++ {
		toID := ids[i+1]

		e, found := prevEdge[toID]
		if !found {
			cur.Stations = append(cur.Stations, g.Stations[toID])
			continue
		}

		if e.IsTransfer {
			if len(cur.Stations) > 0 {
				segments = append(segments, cur)
			}
			segments = append(segments, models.RouteSegment{
				Stations:   []models.Station{g.Stations[ids[i]], g.Stations[toID]},
				TimeMin:    e.WeightMin,
				IsTransfer: true,
			})
			cur = models.RouteSegment{
				Stations: []models.Station{g.Stations[toID]},
			}
		} else {
			cur.Stations = append(cur.Stations, g.Stations[toID])
			cur.TimeMin += e.WeightMin
		}
	}

	if len(cur.Stations) > 0 {
		segments = append(segments, cur)
	}

	return segments
}
