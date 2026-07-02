#include "EditorScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../core/Nav.h"
#include "../catalog/Catalog.h"
#include "../services/Storage.h"
#include <Arduino.h>
#include <stdio.h>

EditorScreen editorScreen;

static const int TOP = 30, ROWH = 28, VIS = 5;

// bottom toolbar (6 compact buttons)
static Rect tb(int i) { int w = SCREEN_W/6; return { i*w+2, SCREEN_H-30, w-4, 26 }; }

// stepper widgets
static Rect sMinus() { return { 24,  120, 60, 60 }; }
static Rect sPlus()  { return { SCREEN_W-84, 120, 60, 60 }; }
static Rect sOk()    { return { SCREEN_W/2+8, 196, 100, 36 }; }
static Rect sCancel(){ return { SCREEN_W/2-108, 196, 100, 36 }; }

void EditorScreen::ensureVisible() {
  if (app.st.sel < _scroll) _scroll = app.st.sel;
  if (app.st.sel >= _scroll + VIS) _scroll = app.st.sel - VIS + 1;
  if (_scroll < 0) _scroll = 0;
}

// ---- label helper: friendly param text (song/dance names via Catalog when loaded) ----
static void stepLabel(const Step& s, char* out, int n) {
  const MoveInfo& mi = moveInfo(s.move);
  if (s.move == PLAY_SONG || s.move == PLAY_DANCE || s.move == PLAY_PHRASE) {
    char nm[28]; catalog::label(s.move, s.p1, nm, sizeof nm);
    snprintf(out, n, "%s", nm);
  } else if (mi.hasParam) {
    snprintf(out, n, "%s  %d%s", mi.label, s.p1, mi.pUnit);
  } else {
    snprintf(out, n, "%s", mi.label);
  }
  // show a loop badge inline when this step repeats (SPEC §5 REPEAT)
  if (s.repeatGrp >= 2) {
    int len = strlen(out);
    snprintf(out+len, n-len, "  (x%d)", s.repeatGrp);
  }
}

void EditorScreen::drawList() {
  auto& seq = app.st.scratch;
  canvas.fillRect(0, TOP, SCREEN_W, VIS*ROWH+4, Theme::BG);
  if (seq.empty()) {
    canvas.text("No steps yet.", SCREEN_W/2, 78, Theme::TEXT_DIM, 2, Align::C);
    canvas.text("Tap  + Add  to place a move.", SCREEN_W/2, 104, Theme::TEXT_DIM, 1, Align::C);
    return;
  }
  for (int r = 0; r < VIS; r++) {
    int i = _scroll + r; if (i >= seq.count) break;
    int y = TOP + r*ROWH;
    Step s = seq.steps[i];
    bool sel = (i == app.st.sel);
    canvas.card(6, y+2, SCREEN_W-12, ROWH-4, sel?Theme::CARD_SEL:Theme::CARD, sel);
    canvas.moveGlyph(s.move, 22, y+ROWH/2, 8, catColor(catOf(s.move)));
    char line[40], body[32]; stepLabel(s, body, sizeof body);
    snprintf(line, sizeof line, "%2d. %s", i+1, body);
    canvas.text(line, 40, y+ROWH/2-4, s.repeatGrp>=2?Theme::GOLD:Theme::TEXT, 1, Align::L);
  }
  // scroll hint
  if (seq.count > VIS) {
    char sc[12]; snprintf(sc,sizeof sc,"%d-%d/%d", _scroll+1, min(_scroll+VIS,(int)seq.count), seq.count);
    canvas.text(sc, SCREEN_W-6, TOP+VIS*ROWH+2, Theme::TEXT_DIM, 1, Align::R);
  }
}

void EditorScreen::enter() {
  if (_mode == Mode::Stepper) { drawStepper(); return; }  // pending param edit from palette
  ensureVisible();
  canvas.clear(Theme::BG);
  char t[40]; snprintf(t, sizeof t, "%s  %d/%d", app.st.scratch.name, app.st.scratch.count, MAX_STEPS);
  canvas.title(t);
  drawList();
  // toolbar: Back + Add Del Up Dn Run
  nav::drawBack();
  canvas.button(tb(1).x,tb(1).y,tb(1).w,tb(1).h,"+Add",Theme::C_MOVE,Theme::TEXT);
  canvas.button(tb(2).x,tb(2).y,tb(2).w,tb(2).h,"Del", Theme::RED, Theme::TEXT);
  canvas.button(tb(3).x,tb(3).y,tb(3).w,tb(3).h,"Up",  Theme::CARD,Theme::TEXT);
  canvas.button(tb(4).x,tb(4).y,tb(4).w,tb(4).h,"Dn",  Theme::CARD,Theme::TEXT);
  canvas.button(tb(5).x,tb(5).y,tb(5).w,tb(5).h,"Run", Theme::GREEN,Theme::TEXT);
}

void EditorScreen::drawStepper() {
  canvas.clear(Theme::BG);
  const MoveInfo& mi = moveInfo(_sMove);
  canvas.title(_sRepeat ? "Repeat Move" : _sNew ? "Add Move" : "Edit Move");
  // for repeat, glyph/label reflect the step being looped
  MoveId shown = _sRepeat ? app.st.scratch.steps[app.st.sel].move : _sMove;
  canvas.moveGlyph(shown, 40, 62, 18, catColor(catOf(shown)));
  canvas.text(_sRepeat ? moveInfo(shown).label : mi.label, SCREEN_W/2+10, 50, Theme::GOLD, 2, Align::C);

  // value display (name for song/dance/phrase; "x N" for repeat)
  char val[28];
  if (_sRepeat)                                                snprintf(val, sizeof val, "loop x%d", _sVal);
  else if (_sMove==PLAY_SONG || _sMove==PLAY_DANCE || _sMove==PLAY_PHRASE)
    catalog::label(_sMove, _sVal, val, sizeof val);
  else snprintf(val, sizeof val, "%d %s", _sVal, mi.pUnit);
  canvas.fillRound(90, 122, SCREEN_W-180, 56, 8, Theme::CARD);
  canvas.text(val, SCREEN_W/2, 142, Theme::TEXT, 2, Align::C);

  Rect m=sMinus(), p=sPlus();
  canvas.button(m.x,m.y,m.w,m.h,"-",Theme::BG_ALT,Theme::GOLD);
  canvas.button(p.x,p.y,p.w,p.h,"+",Theme::BG_ALT,Theme::GOLD);
  Rect ok=sOk(), ca=sCancel();
  canvas.button(ca.x,ca.y,ca.w,ca.h,"Cancel",Theme::CARD,Theme::TEXT);
  canvas.button(ok.x,ok.y,ok.w,ok.h,"OK",Theme::GREEN,Theme::TEXT);
}

void EditorScreen::beginNewParam(MoveId m, int16_t initial) {
  _mode = Mode::Stepper; _sNew = true; _sRepeat = false; _sMove = m; _sVal = initial;
}

void EditorScreen::beginRepeat() {
  if (app.st.scratch.empty()) { _mode = Mode::List; return; }   // nothing to loop
  _mode = Mode::Stepper; _sRepeat = true; _sNew = false; _sMove = REPEAT;
  int cur = app.st.scratch.steps[app.st.sel].repeatGrp;
  _sVal = (cur >= 2) ? cur : 2;
}

void EditorScreen::insertMove(MoveId m, int16_t p1) {
  auto& seq = app.st.scratch;
  int at = seq.empty() ? 0 : app.st.sel + 1;
  Step s{ m, p1, 0 };
  if (seq.insertAt(at, s)) app.st.sel = at;
  _mode = Mode::List;
}

void EditorScreen::openStepperFor(int idx) {
  const MoveInfo& mi = moveInfo(app.st.scratch.steps[idx].move);
  if (!mi.hasParam) return;
  _mode = Mode::Stepper; _sNew = false; _sIdx = idx;
  _sMove = app.st.scratch.steps[idx].move;
  _sVal  = app.st.scratch.steps[idx].p1;
}

void EditorScreen::commitStepper() {
  auto& seq = app.st.scratch;
  if (_sRepeat) {
    seq.steps[app.st.sel].repeatGrp = (uint8_t)(_sVal <= 1 ? 0 : _sVal);  // loop the selected step
    _mode = Mode::List; _sRepeat = false; return;
  }
  if (_sNew) {
    int at = seq.empty() ? 0 : app.st.sel + 1;
    Step s{ _sMove, _sVal, 0 };
    if (seq.insertAt(at, s)) app.st.sel = at;
  } else {
    seq.steps[_sIdx].p1 = _sVal;
  }
  _mode = Mode::List;
}

void EditorScreen::onTap(int x, int y) {
  if (_mode == Mode::Stepper) {
    const MoveInfo& mi = moveInfo(_sMove);
    int stepInc = (_sMove==WAIT) ? 100 : 1;
    if (sMinus().hit(x,y))  { int nv=_sVal-stepInc; if(nv<mi.pMin)nv=mi.pMin; _sVal=(int16_t)nv; drawStepper(); return; }
    if (sPlus().hit(x,y))   { int nv=_sVal+stepInc; if(nv>mi.pMax)nv=mi.pMax; _sVal=(int16_t)nv; drawStepper(); return; }
    if (sOk().hit(x,y))     { commitStepper(); enter(); return; }
    if (sCancel().hit(x,y)) { _mode = Mode::List; enter(); return; }
    return;
  }

  // list mode
  auto& seq = app.st.scratch;
  // tap the title bar to Save the sequence (SPEC §6: Save promotes scratch to a named file)
  if (y < 28) {
    if (!seq.empty()) {
      bool ok = storage::save(seq);
      canvas.fillRound(SCREEN_W/2-70, 92, 140, 44, 8, ok?Theme::GREEN:Theme::RED);
      canvas.text(ok?"Saved!":"Save failed", SCREEN_W/2, 106, Theme::TEXT, 2, Align::C);
      delay(700);
      enter();
    }
    return;
  }
  if (nav::backRect().hit(x,y)) { app.go(ScreenId::Home); return; }
  if (tb(1).hit(x,y)) { app.go(ScreenId::Palette); return; }
  if (tb(2).hit(x,y)) { if (!seq.empty()){ seq.removeAt(app.st.sel); if(app.st.sel>=seq.count) app.st.sel=max(0,(int)seq.count-1);} enter(); return; }
  if (tb(3).hit(x,y)) { if (seq.moveUp(app.st.sel)) { app.st.sel--; } enter(); return; }
  if (tb(4).hit(x,y)) { if (seq.moveDown(app.st.sel)) { app.st.sel++; } enter(); return; }
  if (tb(5).hit(x,y)) { app.go(ScreenId::Run); return; }

  // tap a row: select; second tap on an already-selected param row opens the stepper
  if (y>=TOP && y<TOP+VIS*ROWH) {
    int i = _scroll + (y-TOP)/ROWH;
    if (i < seq.count) {
      if (i == app.st.sel && moveInfo(seq.steps[i].move).hasParam) { openStepperFor(i); drawStepper(); }
      else { app.st.sel = i; drawList(); }
    }
  }
}
