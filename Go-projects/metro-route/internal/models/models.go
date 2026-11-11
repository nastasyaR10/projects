package models

type Station struct {
	ID     string `json:"id"`
	Name   string `json:"name"`
	LineID string `json:"line_id"`
}

type Edge struct {
	FromID        string `json:"from_id"`
	ToID          string `json:"to_id"`
	TravelTimeMin int    `json:"travel_time_min"`
}

type Transfer struct {
	FromStationID   string `json:"from_station_id"`
	ToStationID     string `json:"to_station_id"`
	TransferTimeMin int    `json:"transfer_time_min"`
}

type MetroSchema struct {
	Stations  []Station  `json:"stations"`
	Edges     []Edge     `json:"edges"`
	Transfers []Transfer `json:"transfers"`
}

type GraphEdge struct {
	To         string
	WeightMin  int
	IsTransfer bool
}

type Graph struct {
	AdjacencyList map[string][]GraphEdge
	Stations      map[string]Station
}

type RouteSegment struct {
	Stations   []Station
	TimeMin    int
	IsTransfer bool
}

type RouteMode int

const (
	ModeFastest RouteMode = iota
	ModeFewestTransfers
)

type Route struct {
	Path           []Station
	TotalTimeMin   int
	TransfersCount int
	Segments       []RouteSegment
	Mode           RouteMode
}
