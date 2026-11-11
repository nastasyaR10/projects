package telegram

import (
	"context"
	"fmt"
	"log/slog"
	"sort"
	"strings"
	"time"

	"github.com/go-telegram/bot"
	"github.com/go-telegram/bot/models"

	appmodels "github.com/nastasyaR10/projects/metro-route/internal/models"
	"github.com/nastasyaR10/projects/metro-route/internal/router"
	"github.com/nastasyaR10/projects/metro-route/internal/storage"
)

type Handler struct {
	graph  *appmodels.Graph
	states *StateStore
	db     *storage.RequestStore
}

func NewHandler(g *appmodels.Graph, states *StateStore, db *storage.RequestStore) *Handler {
	return &Handler{graph: g, states: states, db: db}
}

func (h *Handler) Handle(ctx context.Context, b *bot.Bot, update *models.Update) {
	if update.CallbackQuery != nil {
		h.handleCallback(ctx, b, update.CallbackQuery)
		return
	}
	if update.Message == nil {
		return
	}
	h.handleMessage(ctx, b, update.Message)
}

func (h *Handler) handleMessage(ctx context.Context, b *bot.Bot, msg *models.Message) {
	chatID := msg.Chat.ID
	text := strings.TrimSpace(msg.Text)
	slog.Debug("message received", "chatID", chatID, "text", text)

	if text == "/history" {
		h.handleHistory(ctx, b, chatID)
		return
	}

	st, ok := h.states.Get(chatID)

	if text == "/start" || !ok {
		var answer string
		if text == "/start" {
			answer = "Привет! Я помогу построить маршрут в московском метро\n\nКак это работает:\n" +
				"1. Введите название станции отправления, затем выбери одну из предложенных вариантов.\n" +
				"2. Аналогично выберите станцию назначения.\n" +
				"3. Далее выберите параметр по которому хочешь оптимизировать маршрут (по времени, количеству пересадок или загруженности транспорта).\n\n"
		}
		h.states.Set(chatID, UserState{Step: StepChooseFrom})
		h.send(ctx, b, chatID, answer+"Введите название станции отправления:")
		return
	}

	switch st.Step {
	case StepChooseFrom:
		h.suggestStations(ctx, b, chatID, text, true)
	case StepChooseTo:
		h.suggestStations(ctx, b, chatID, text, false)
	case StepChooseMode:
		slog.Debug("text on StepChooseMode, prompting to use keyboard", "chatID", chatID, "text", text)
		h.send(ctx, b, chatID, "Выберите режим кнопкой выше")
	default:
		h.send(ctx, b, chatID, "Нажмите /start, чтобы начать заново.\nНажмите /history, чтобы увидеть историю маршрутов.")
	}
}

func (h *Handler) suggestStations(ctx context.Context, b *bot.Bot, chatID int64, query string, isFrom bool) {
	matches := h.findStationsByPrefix(query, 10)
	if len(matches) == 0 {
		h.send(ctx, b, chatID, "Станция не найдена. Попробуйте другое название:")
		return
	}

	st, ok := h.states.Get(chatID)
	if !ok {
		slog.Error("nil state in suggestStations", "bug", true, "chatID", chatID, "isFrom", isFrom)
		h.send(ctx, b, chatID, "Что-то пошло не так. Нажмите /start, чтобы начать заново.\nНажмите /history, чтобы увидеть историю маршрутов.")
		return
	}

	prefix := "from"
	if !isFrom {
		prefix = "to"
	}

	var rows [][]models.InlineKeyboardButton
	for _, s := range matches {
		if !isFrom && s.ID == st.FromID {
			continue
		}
		label := fmt.Sprintf("%s %s (%s)", lineEmoji(s.LineID), s.Name, lineShortName(s.LineID))
		rows = append(rows, []models.InlineKeyboardButton{
			{Text: label, CallbackData: prefix + ":" + s.ID},
		})
	}
	if len(rows) == 0 {
		h.send(ctx, b, chatID, "Нет подходящих станций. Попробуйте другое название:")
		return
	}

	h.sendWithKeyboard(ctx, b, chatID, "Выберите станцию:", &models.InlineKeyboardMarkup{InlineKeyboard: rows})
}

type lineInfo struct {
	emoji string
	name  string
}

var lines = map[string]lineInfo{
	"sokolnicheskaya":              {"🔴", "Сокольническая"},
	"zamoskvoretskaya":             {"🟢", "Замоскворецкая"},
	"arbatsko_pokrovskaya":         {"🔵", "Арбатско-Покровская"},
	"koltsevaya":                   {"🟤", "Кольцевая"},
	"kaluzhsko_rizhskaya":          {"🟠", "Калужско-Рижская"},
	"tagansko_krasnopresnenskaya":  {"🟣", "Таганско-Краснопресненская"},
	"kalininskaya":                 {"🟡", "Калининская"},
	"solntsevskaya":                {"🟡", "Солнцевская"},
	"rublyovo_arkhangelskaya":      {"⚫", "Рублёво-Архангельская"},
	"filevskaya":                   {"🔹", "Филёвская"},
	"serpukhovsko_timiryazevskaya": {"🩶", "Серпуховско-Тимирязевская"},
	"lyublinsko_dmitrovskaya":      {"💚", "Люблинско-Дмитровская"},
	"bolshaya_koltsevaya":          {"🩵", "Большая кольцевая"},
	"butovskaya":                   {"⬜️", "Бутовская"},
	"nekrasovskaya":                {"🩷", "Некрасовская"},
	"troitskaya":                   {"🟩", "Троицкая"},
}

func lineEmoji(lineID string) string {
	if info, ok := lines[lineID]; ok {
		return info.emoji
	}
	return "⚪"
}

func lineShortName(lineID string) string {
	if info, ok := lines[lineID]; ok {
		return info.name
	}
	return lineID
}

type modeInfo struct {
	db   string
	name string
}

var modes = map[appmodels.RouteMode]modeInfo{
	appmodels.ModeFastest:         {db: "fast", name: "быстрее"},
	appmodels.ModeFewestTransfers: {db: "fewest_transfers", name: "меньше пересадок"},
}

func modeToString(m appmodels.RouteMode) string {
	if info, ok := modes[m]; ok {
		return info.db
	}
	return "fast"
}

func modeName(dbMode string) string {
	for _, info := range modes {
		if info.db == dbMode {
			return info.name
		}
	}
	return dbMode
}

func (h *Handler) handleCallback(ctx context.Context, b *bot.Bot, cb *models.CallbackQuery) {
	if cb.Message.Message == nil {
		return
	}
	chatID := cb.Message.Message.Chat.ID
	data := cb.Data
	slog.Debug("callback received", "chatID", chatID, "data", data)

	_, _ = b.AnswerCallbackQuery(ctx, &bot.AnswerCallbackQueryParams{CallbackQueryID: cb.ID})

	if data == "restart" {
		h.states.Set(chatID, UserState{Step: StepChooseFrom})
		h.send(ctx, b, chatID, "Введите название станции отправления:")
		return
	}

	if data == "history" {
		h.handleHistory(ctx, b, chatID)
		return
	}

	st, ok := h.states.Get(chatID)
	if !ok {
		h.send(ctx, b, chatID, "Нажмите /start, чтобы начать.\nНажмите /history, чтобы увидеть историю маршрутов.")
		return
	}

	switch {
	case strings.HasPrefix(data, "from:"):
		fromID := strings.TrimPrefix(data, "from:")
		h.clearKeyboard(ctx, b, chatID, cb.Message.Message.ID)
		h.states.Update(chatID, func(st *UserState) {
			st.FromID = fromID
			st.Step = StepChooseTo
		})
		h.send(ctx, b, chatID, "Откуда: "+h.stationName(fromID)+"\nВведите название станции назначения:")

	case strings.HasPrefix(data, "to:"):
		toID := strings.TrimPrefix(data, "to:")
		h.clearKeyboard(ctx, b, chatID, cb.Message.Message.ID)
		h.states.Update(chatID, func(st *UserState) {
			st.ToID = toID
			st.Step = StepChooseMode
		})
		h.sendModeKeyboard(ctx, b, chatID)

	case strings.HasPrefix(data, "mode:"):
		modeStr := strings.TrimPrefix(data, "mode:")
		var mode appmodels.RouteMode
		switch modeStr {
		case "fast":
			mode = appmodels.ModeFastest
		case "fewer_transfers":
			mode = appmodels.ModeFewestTransfers
		case "less_crowded":
			h.send(ctx, b, chatID, "Этот режим пока в разработке. Выберите «Быстрее» или «Меньше пересадок».")
			return
		default:
			h.send(ctx, b, chatID, "Неизвестный режим. Нажмите /start, чтобы начать заново.\nНажмите /history, чтобы увидеть историю маршрутов.")
			return
		}
		h.clearKeyboard(ctx, b, chatID, cb.Message.Message.ID)
		h.buildAndSendRoute(ctx, b, chatID, st.FromID, st.ToID, mode)
		h.states.Clear(chatID)

	default:
		h.send(ctx, b, chatID, "Нажмите /start, чтобы начать.\nНажмите /history, чтобы увидеть историю маршрутов.")
	}
}

func (h *Handler) clearKeyboard(ctx context.Context, b *bot.Bot, chatID int64, messageID int) {
	emptyKeyboard := &models.InlineKeyboardMarkup{InlineKeyboard: [][]models.InlineKeyboardButton{}}
	_, err := b.EditMessageReplyMarkup(ctx, &bot.EditMessageReplyMarkupParams{
		ChatID:      chatID,
		MessageID:   messageID,
		ReplyMarkup: emptyKeyboard,
	})
	if err != nil {
		slog.Warn("edit reply markup failed", "err", err, "chatID", chatID, "messageID", messageID)
	}
}

func (h *Handler) sendModeKeyboard(ctx context.Context, b *bot.Bot, chatID int64) {
	kb := &models.InlineKeyboardMarkup{
		InlineKeyboard: [][]models.InlineKeyboardButton{
			{{Text: "Быстрее", CallbackData: "mode:fast"}},
			{{Text: "Меньше пересадок", CallbackData: "mode:fewer_transfers"}},
			{{Text: "Свободнее (в разработке)", CallbackData: "mode:less_crowded"}},
		},
	}
	h.sendWithKeyboard(ctx, b, chatID, "Как строим маршрут?", kb)
}

func (h *Handler) buildAndSendRoute(ctx context.Context, b *bot.Bot, chatID int64, fromID, toID string, mode appmodels.RouteMode) {
	route, err := router.ShortestPath(ctx, h.graph, fromID, toID, mode)
	if err != nil {
		slog.Error("build route failed", "err", err, "chatID", chatID, "from", fromID, "to", toID, "mode", mode)
		h.send(ctx, b, chatID, "Не удалось построить маршрут. Попробуйте ещё раз, нажмите /start.\nНажмите /history, чтобы увидеть историю маршрутов.")
		return
	}

	dbCtx, dbCancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer dbCancel()
	if err := h.db.SaveRequest(dbCtx, chatID, fromID, toID, modeToString(mode)); err != nil {
		slog.Error("save request failed", "err", err, "chatID", chatID, "from", fromID, "to", toID, "mode", mode)
	}

	kb := &models.InlineKeyboardMarkup{
		InlineKeyboard: [][]models.InlineKeyboardButton{
			{{Text: "Построить новый маршрут", CallbackData: "restart"}},
			{{Text: "История маршрутов", CallbackData: "history"}},
		},
	}
	h.sendWithKeyboard(ctx, b, chatID, formatRoute(route), kb)
}

func (h *Handler) handleHistory(ctx context.Context, b *bot.Bot, chatID int64) {
	if err := ctx.Err(); err != nil {
		slog.Debug("handleHistory: context cancelled", "err", err, "chatID", chatID)
		return
	}

	reqs, err := h.db.ListByUser(ctx, chatID, 10)
	if err != nil {
		slog.Error("list history failed", "err", err, "chatID", chatID)
		h.send(ctx, b, chatID, "Не удалось загрузить историю. Попробуйте позже.")
		return
	}

	kb := &models.InlineKeyboardMarkup{
		InlineKeyboard: [][]models.InlineKeyboardButton{
			{{Text: "Построить новый маршрут", CallbackData: "restart"}},
		},
	}

	if len(reqs) == 0 {
		h.sendWithKeyboard(ctx, b, chatID, "История пока пуста. Постройте свой первый маршрут!", kb)
		return
	}
	h.sendWithKeyboard(ctx, b, chatID, formatHistory(reqs, h.stationName), kb)
}

func formatHistory(reqs []*storage.Request, name func(string) string) string {
	var sb strings.Builder
	sb.WriteString("Последние маршруты:\n\n")
	for _, r := range reqs {
		sb.WriteString(fmt.Sprintf("• %s → %s (%s, %s)\n",
			name(r.FromStation),
			name(r.ToStation),
			modeName(r.Mode),
			r.CreatedAt.Format("02.01.06 15:04"),
		))
	}
	return sb.String()
}

func (h *Handler) findStationsByPrefix(query string, limit int) []appmodels.Station {
	q := strings.ToLower(strings.TrimSpace(query))
	if q == "" {
		return nil
	}

	var out []appmodels.Station
	for _, s := range h.graph.Stations {
		if strings.HasPrefix(strings.ToLower(s.Name), q) {
			out = append(out, s)
		}
	}

	sort.Slice(out, func(i, j int) bool {
		return out[i].Name < out[j].Name
	})

	if len(out) > limit {
		out = out[:limit]
	}
	return out
}

func (h *Handler) stationName(id string) string {
	if s, ok := h.graph.Stations[id]; ok {
		return s.Name
	}
	return id
}

func (h *Handler) send(ctx context.Context, b *bot.Bot, chatID int64, text string) {
	if err := ctx.Err(); err != nil {
		slog.Debug("skip send, context cancelled", "err", err, "chatID", chatID)
		return
	}
	_, err := b.SendMessage(ctx, &bot.SendMessageParams{ChatID: chatID, Text: text})
	if err != nil {
		slog.Error("send message failed", "err", err, "chatID", chatID)
	}
}

func (h *Handler) sendWithKeyboard(ctx context.Context, b *bot.Bot, chatID int64, text string, kb *models.InlineKeyboardMarkup) {
	if err := ctx.Err(); err != nil {
		slog.Debug("skip sendWithKeyboard, context cancelled", "err", err, "chatID", chatID)
		return
	}
	_, err := b.SendMessage(ctx, &bot.SendMessageParams{
		ChatID:      chatID,
		Text:        text,
		ReplyMarkup: kb,
	})
	if err != nil {
		slog.Error("sendWithKeyboard failed", "err", err, "chatID", chatID)
	}
}

func formatRoute(r *appmodels.Route) string {
	var sb strings.Builder
	modeName := "быстрее"
	if r.Mode == appmodels.ModeFewestTransfers {
		modeName = "меньше пересадок"
	}
	sb.WriteString(fmt.Sprintf("Маршрут (%d мин, пересадок: %d, режим: %s):\n\n", r.TotalTimeMin, r.TransfersCount, modeName))

	for _, seg := range r.Segments {
		if seg.IsTransfer {
			sb.WriteString(fmt.Sprintf("→ пересадка (%d мин) →\n", seg.TimeMin))
			continue
		}
		if len(seg.Stations) == 0 {
			continue
		}

		sb.WriteString(lineEmoji(seg.Stations[0].LineID) + " ")
		for i, s := range seg.Stations {
			if i > 0 {
				sb.WriteString(" → ")
			}
			sb.WriteString(s.Name)
		}
		if seg.TimeMin > 0 {
			sb.WriteString(fmt.Sprintf(" (%d мин)", seg.TimeMin))
		}
		sb.WriteString("\n")
	}
	return sb.String()
}
