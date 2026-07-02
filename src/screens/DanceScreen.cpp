#include "DanceScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../core/Nav.h"
#include "../imu/Imu.h"
#include <Arduino.h>

DanceScreen danceScreen;

static Rect btnRecord() { return { SCREEN_W/2-70, 158, 140, 46 }; }
static Rect btnStop()   { return { SCREEN_W/2-70, 200, 140, 34 }; }

void DanceScreen::drawIdle() {
  canvas.clear(Theme::BG);
  canvas.title("Dance to Code");
  canvas.text("Wave, spin & bounce the wand", SCREEN_W/2, 40, Theme::TEXT, 1, Align::C);
  canvas.text("and watch it become code!", SCREEN_W/2, 56, Theme::TEXT, 1, Align::C);
  canvas.catGlyph(MoveCat::Spin, SCREEN_W/2, 112, 30, Theme::C_SPIN);
  Rect b=btnRecord();
  canvas.button(b.x,b.y,b.w,b.h,"Record",Theme::RED,Theme::TEXT);
  nav::drawBack();
}

void DanceScreen::drawHint(LiveHint h) {
  // central live-recognition indicator
  int cx = SCREEN_W/2, cy = 96, r = 30;
  canvas.fillRect(cx-60, cy-40, 120, 80, Theme::BG);
  uint16_t c = Theme::TEXT_DIM;
  switch (h) {
    case LiveHint::Spin:       canvas.moveGlyph(TWIRL_RIGHT, cx, cy, r, Theme::C_SPIN); break;
    case LiveHint::BounceUp:   canvas.moveGlyph(STEP_FORWARD, cx, cy, r, Theme::C_ARMS); break;
    case LiveHint::BounceDown: canvas.moveGlyph(STEP_BACK, cx, cy, r, Theme::C_ARMS); break;
    case LiveHint::TiltL:      canvas.moveGlyph(HEAD_LEFT, cx, cy, r, Theme::C_HEAD); break;
    case LiveHint::TiltR:      canvas.moveGlyph(HEAD_RIGHT, cx, cy, r, Theme::C_HEAD); break;
    case LiveHint::LeanF:      canvas.moveGlyph(STEP_FORWARD, cx, cy, r, Theme::C_MOVE); break;
    case LiveHint::LeanB:      canvas.moveGlyph(STEP_BACK, cx, cy, r, Theme::C_MOVE); break;
    default:                   canvas.circle(cx, cy, 8, c); break;
  }
}

void DanceScreen::drawLive() {
  canvas.clear(Theme::BG);
  canvas.title("Recording...");
  canvas.fillCircle(16, 13, 6, Theme::RED);          // rec dot
  drawHint(_cap.hint());
  char c[24]; snprintf(c, sizeof c, "%d moves captured", _cap.committed());
  canvas.text(c, SCREEN_W/2, 148, Theme::GOLD, 1, Align::C);
  Rect s=btnStop();
  canvas.button(s.x,s.y,s.w,s.h,"Stop",Theme::RED,Theme::TEXT);
  nav::drawBack();
}

void DanceScreen::enter() {
  if (_cap.phase()==CapPhase::Recording || _cap.phase()==CapPhase::Calibrating) drawLive();
  else drawIdle();
  _lastHint = (LiveHint)255; _lastCount = -1;
}

void DanceScreen::tick(uint32_t ms) {
  if (_cap.phase()==CapPhase::Recording) {
    ImuSample s;
    if (imu::read(s)) _cap.feed(s, ms);

    // commit flash
    MoveId fm; int16_t fp;
    if (_cap.takeCommitFlash(fm, fp)) { _flashMove=fm; _flashP=fp; _flashUntil=ms+500; }

    // redraw hint on change
    if (_cap.hint() != _lastHint) { drawHint(_cap.hint()); _lastHint=_cap.hint(); }
    if (_cap.committed() != _lastCount) {
      char c[24]; snprintf(c,sizeof c,"%d moves captured", _cap.committed());
      canvas.fillRect(SCREEN_W/2-80,144,160,12,Theme::BG);
      canvas.text(c, SCREEN_W/2, 148, Theme::GOLD, 1, Align::C);
      _lastCount=_cap.committed();
    }
    // flash last committed move icon + label bottom-left
    if (ms < _flashUntil) {
      canvas.fillRound(6, 30, 120, 30, 6, catColor(catOf(_flashMove)));
      canvas.moveGlyph(_flashMove, 22, 45, 9, Theme::BG);
      canvas.text(moveInfo(_flashMove).label, 40, 41, Theme::BG, 1, Align::L);
    } else if (_flashUntil && ms >= _flashUntil) {
      canvas.fillRect(6,30,120,30,Theme::BG); _flashUntil=0;
    }
  }
}

void DanceScreen::onTap(int x, int y) {
  if (nav::backRect().hit(x,y)) {
    if (_cap.phase()==CapPhase::Recording) _cap.stop();
    app.go(ScreenId::Home); return;
  }
  if (_cap.phase()==CapPhase::Idle || _cap.phase()==CapPhase::Done) {
    if (btnRecord().hit(x,y)) {
      // brief "hold still" calibration prompt, then record
      canvas.clear(Theme::BG); canvas.title("Hold still...");
      canvas.text("Calibrating", SCREEN_W/2, 100, Theme::GOLD, 2, Align::C);
      _cap.start(app.st.sensitivity, app.st.bounceToArms);   // calibrates gyro inside
      drawLive();
    }
    return;
  }
  // recording -> Stop: hand the captured steps to the editor for review (SPEC §9)
  if (btnStop().hit(x,y)) {
    _cap.stop();
    app.st.scratch = _cap.result();
    app.st.sel = 0;
    app.go(ScreenId::Editor);
  }
}
