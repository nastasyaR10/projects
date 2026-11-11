package router

import (
	"testing"

	"github.com/nastasyaR10/projects/metro-route/internal/models"
)

func TestLess_Fastest(t *testing.T) {
	cases := []struct {
		name string
		a, b state
		want bool
	}{
		{"less time", state{time: 5, transfers: 3}, state{time: 7, transfers: 0}, true},
		{"more time", state{time: 9, transfers: 0}, state{time: 7, transfers: 3}, false},
		{"same time fewer transfers", state{time: 5, transfers: 1}, state{time: 5, transfers: 2}, true},
		{"same time more transfers", state{time: 5, transfers: 2}, state{time: 5, transfers: 1}, false},
		{"equal", state{time: 5, transfers: 1}, state{time: 5, transfers: 1}, false},
	}
	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			if got := less(models.ModeFastest, c.a, c.b); got != c.want {
				t.Fatalf("less(ModeFastest, %+v, %+v) = %v, want %v", c.a, c.b, got, c.want)
			}
		})
	}
}

func TestLess_FewestTransfers(t *testing.T) {
	cases := []struct {
		name string
		a, b state
		want bool
	}{
		{"fewer transfers wins", state{time: 100, transfers: 1}, state{time: 5, transfers: 2}, true},
		{"more transfers loses", state{time: 5, transfers: 2}, state{time: 100, transfers: 1}, false},
		{"same transfers faster wins", state{time: 5, transfers: 1}, state{time: 7, transfers: 1}, true},
		{"same transfers slower loses", state{time: 7, transfers: 1}, state{time: 5, transfers: 1}, false},
		{"equal", state{time: 5, transfers: 1}, state{time: 5, transfers: 1}, false},
	}
	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			if got := less(models.ModeFewestTransfers, c.a, c.b); got != c.want {
				t.Fatalf("less(ModeFewestTransfers, %+v, %+v) = %v, want %v", c.a, c.b, got, c.want)
			}
		})
	}
}

func TestLess_DefaultFallsBackToFastest(t *testing.T) {
	unknown := models.RouteMode(99)
	if !less(unknown, state{time: 1, transfers: 5}, state{time: 2, transfers: 0}) {
		t.Fatal("unknown mode should prefer less time (fallback to fastest)")
	}
}

func TestLess_Transitivity(t *testing.T) {
	a := state{time: 1, transfers: 3}
	b := state{time: 2, transfers: 2}
	c := state{time: 3, transfers: 1}
	if !less(models.ModeFastest, a, b) || !less(models.ModeFastest, b, c) {
		t.Fatal("setup invalid: expected a<b and b<c")
	}
	if !less(models.ModeFastest, a, c) {
		t.Fatal("transitivity violated for ModeFastest")
	}
}
