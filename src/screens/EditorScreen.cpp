#include "EditorScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../core/Nav.h"
#include <Arduino.h>
#include <stdio.h>

EditorScreen editorScreen;

// M0: read-only list + toolbar shell. M1 wires the actions.
static Rect btnAdd() { return { 84,  SCREEN_H-30, 66, 26 }; }
static Rect btnRun() { return { 246, SCREEN_H-30, 68, 26 }; }

void EditorScreen::drawList() {
  auto& seq = app.st.scratch;
  const int top = 30, rowH = 30, visible = 6;
  canvas.fillRect(0, top, SCREEN_W, visible*rowH, Theme::BG);
  if (seq.empty()) {
    canvas.text("No steps yet.", SCREEN_W/2, 80, Theme::TEXT_DIM, 2, Align::C);
    canvas.text("Tap + Add to place a move.", SCREEN_W/2, 108, Theme::TEXT_DIM, 1, Align::C);
    return;
  }
  for (int r = 0; r < visible; r++) {
    int i = _scroll + r;
    if (i >= seq.count) break;
    int y = top + r*rowH;
    Step s = seq.steps[i];
    bool sel = (i == app.st.sel);
    canvas.card(6, y+2, SCREEN_W-12, rowH-4, sel?Theme::CARD_SEL:Theme::CARD, sel);
    canvas.moveGlyph(s.move, 24, y+rowH/2, 9, catColor(catOf(s.move)));
    char line[40];
    const MoveInfo& mi = moveInfo(s.move);
    if (mi.hasParam) snprintf(line, sizeof line, "%2d. %s  %d%s", i+1, mi.label, s.p1, mi.pUnit);
    else             snprintf(line, sizeof line, "%2d. %s", i+1, mi.label);
    canvas.text(line, 44, y+rowH/2-4, Theme::TEXT, 1, Align::L);
  }
}

void EditorScreen::enter() {
  canvas.clear(Theme::BG);
  char t[40]; snprintf(t, sizeof t, "%s  %d/%d", app.st.scratch.name, app.st.scratch.count, MAX_STEPS);
  canvas.title(t);
  drawList();
  // toolbar
  nav::drawBack();
  Rect a=btnAdd(), r=btnRun();
  canvas.button(a.x,a.y,a.w,a.h,"+ Add",Theme::C_MOVE,Theme::TEXT);
  canvas.button(r.x,r.y,r.w,r.h,"Run",Theme::GREEN,Theme::TEXT);
}

void EditorScreen::onTap(int x, int y) {
  if (nav::backRect().hit(x,y)) { app.go(ScreenId::Home); return; }
  if (btnAdd().hit(x,y)) { app.go(ScreenId::Palette); return; }
  if (btnRun().hit(x,y)) { app.go(ScreenId::Run); return; }
  // tap a row to select
  const int top=30, rowH=30, visible=6;
  if (y>=top && y<top+visible*rowH) {
    int i = _scroll + (y-top)/rowH;
    if (i < app.st.scratch.count) { app.st.sel = i; drawList(); }
  }
}
