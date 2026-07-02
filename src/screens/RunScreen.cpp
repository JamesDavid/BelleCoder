#include "RunScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../pal/pal.h"
#include <Arduino.h>

RunScreen runScreen;

static Rect btnPlay() { return { 20,  SCREEN_H-46, 84, 38 }; }
static Rect btnPause(){ return { 118, SCREEN_H-46, 84, 38 }; }
static Rect btnStop() { return { 216, SCREEN_H-46, 84, 38 }; }

// Expand REPEAT groups (repeatGrp spans) into a flat execution list (SPEC §8).
void RunScreen::buildFlat() {
  auto& seq = app.st.scratch;
  _nFlat = 0;
  for (int i = 0; i < seq.count && _nFlat < 128; ) {
    uint8_t grp = seq.steps[i].repeatGrp;
    if (grp == 0) { _flat[_nFlat++] = i; i++; continue; }
    // gather the span sharing this group id
    int j = i; while (j < seq.count && seq.steps[j].repeatGrp == grp) j++;
    int reps = 2;  // default; a REPEAT marker can override in M7
    for (int r = 0; r < reps; r++)
      for (int k = i; k < j && _nFlat < 128; k++) _flat[_nFlat++] = k;
    i = j;
  }
}

void RunScreen::enter() {
  buildFlat();
  _pos = 0; _st = St::Idle;
  canvas.clear(Theme::BG);
  canvas.title("Run");
  drawStatus();
}

void RunScreen::drawStatus() {
  auto& seq = app.st.scratch;
  canvas.fillRect(0, 28, SCREEN_W, SCREEN_H-28-52, Theme::BG);

  // progress
  char p[40];
  int shown = (_pos < _nFlat) ? _pos+1 : _nFlat;
  snprintf(p, sizeof p, "Step %d of %d", (_st==St::Idle)?0:shown, _nFlat);
  canvas.text(p, SCREEN_W/2, 38, Theme::TEXT, 2, Align::C);

  // current move card
  const char* label = "ready";
  if (_pos < _nFlat && _st != St::Idle) label = moveInfo(seq.steps[_flat[_pos]].move).label;
  canvas.fillRound(30, 66, SCREEN_W-60, 54, 8, Theme::CARD);
  if (_pos < _nFlat && _st != St::Idle) {
    Step s = seq.steps[_flat[_pos]];
    canvas.moveGlyph(s.move, 58, 93, 16, catColor(catOf(s.move)));
  }
  canvas.text(label, SCREEN_W/2+16, 84, Theme::GOLD, 2, Align::C);

  // PAL log line (what went to the doll / SIM)
  canvas.fillRound(20, 128, SCREEN_W-40, 30, 6, Theme::BG_ALT);
  canvas.text(pal::lastLog(), SCREEN_W/2, 138, Theme::TEXT_DIM, 1, Align::C);
  if (pal::simulated())
    canvas.text("SIMULATE", SCREEN_W-24, 66, Theme::GOLD, 1, Align::R);

  // transport buttons
  Rect a=btnPlay(),b=btnPause(),c=btnStop();
  canvas.button(a.x,a.y,a.w,a.h, _st==St::Playing?"||>":"Play", Theme::GREEN, Theme::TEXT);
  canvas.button(b.x,b.y,b.w,b.h, "Pause", Theme::CARD, Theme::TEXT);
  canvas.button(c.x,c.y,c.w,c.h, "Stop",  Theme::RED,  Theme::TEXT);
}

void RunScreen::fireCurrent() {
  Step s = app.st.scratch.steps[_flat[_pos]];
  pal::sendMove(s.move, s.p1);
  _stepUntil = millis() + pal::durationOf(s.move, s.p1) + app.st.globalGapMs;
}

void RunScreen::tick(uint32_t ms) {
  if (_st != St::Playing) return;
  if ((int32_t)(ms - _stepUntil) >= 0) {
    _pos++;
    if (_pos >= _nFlat) { _st = St::Done; pal::settle(); drawStatus(); return; }
    fireCurrent();
    drawStatus();
  }
}

void RunScreen::onTap(int x, int y) {
  if (btnPlay().hit(x,y)) {
    if (_nFlat == 0) return;
    if (_st == St::Done || _st == St::Idle) { _pos = 0; }
    _st = St::Playing; fireCurrent(); drawStatus(); return;
  }
  if (btnPause().hit(x,y)) { if (_st==St::Playing){ _st=St::Paused; drawStatus(); } return; }
  if (btnStop().hit(x,y))  { _st=St::Idle; _pos=0; pal::settle(); app.go(ScreenId::Home); return; }
}
