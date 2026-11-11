package telegram

import "sync"

type Step string

const (
	StepChooseFrom Step = "choose_from"
	StepChooseTo   Step = "choose_to"
	StepChooseMode Step = "choose_mode"
)

type UserState struct {
	Step   Step
	FromID string
	ToID   string
}

type StateStore struct {
	mu     sync.Mutex
	states map[int64]*UserState
}

func NewStateStore() *StateStore {
	return &StateStore{states: make(map[int64]*UserState)}
}

func (s *StateStore) Get(chatID int64) (UserState, bool) {
	s.mu.Lock()
	defer s.mu.Unlock()
	st, ok := s.states[chatID]
	if !ok {
		return UserState{}, false
	}
	return *st, true
}

func (s *StateStore) Update(chatID int64, fn func(*UserState)) {
	s.mu.Lock()
	defer s.mu.Unlock()
	st, ok := s.states[chatID]
	if !ok {
		return
	}
	fn(st)
}

func (s *StateStore) Set(chatID int64, st UserState) {
	s.mu.Lock()
	defer s.mu.Unlock()
	s.states[chatID] = &st
}

func (s *StateStore) Clear(chatID int64) {
	s.mu.Lock()
	defer s.mu.Unlock()
	delete(s.states, chatID)
}
