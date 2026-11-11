package api

import "time"

type RouteRequest struct {
	From string `json:"from"`
	To   string `json:"to"`
	Mode string `json:"mode"`
}

type RouteResponse struct {
	TotalTimeMin   int           `json:"total_time_min"`
	TransfersCount int           `json:"transfers_count"`
	Mode           string        `json:"mode"`
	Segments       []SegmentView `json:"segments"`
}

type SegmentView struct {
	Stations   []StationView `json:"stations"`
	TimeMin    int           `json:"time_min"`
	IsTransfer bool          `json:"is_transfer"`
}

type StationView struct {
	ID     string `json:"id"`
	Name   string `json:"name"`
	LineID string `json:"line_id"`
}

type HistoryItem struct {
	FromStation string    `json:"from_station"`
	ToStation   string    `json:"to_station"`
	Mode        string    `json:"mode"`
	CreatedAt   time.Time `json:"created_at"`
}

type ErrorResponse struct {
	Error string `json:"error"`
}
