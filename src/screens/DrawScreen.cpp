#include "DrawScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../core/Toolbar.h"
#include "../hal/Touch.h"
#include <Arduino.h>
#include <math.h>

DrawScreen drawScreen;

static const toolbar::Btn TB[] = {
  { "Back", Theme::CARD }, { "Clear", Theme::RED }, { "Done", Theme::GREEN },
};
static Rect area() { return { 0, 28, SCREEN_W, SCREEN_H - 28 - 34 }; }

void DrawScreen::reset() { _n = 0; _drawing = false; }

void DrawScreen::enter() {
  reset();
  canvas.clear(Theme::BG);
  canvas.title("Draw a Dance");
  Rect a = area();
  canvas.round(a.x+2, a.y, a.w-4, a.h, 8, Theme::OUTLINE);
  canvas.text("Trace a path with your finger", SCREEN_W/2, a.y+a.h/2-12, Theme::TEXT_DIM, 1, Align::C);
  canvas.text("Belle will dance it!", SCREEN_W/2, a.y+a.h/2+2, Theme::TEXT_DIM, 1, Align::C);
  toolbar::draw(TB, 3);
}

void DrawScreen::addPoint(int x, int y) {
  if (_n >= MAXP) return;
  if (_n > 0) {
    int dx = x-_px[_n-1], dy = y-_py[_n-1];
    if (dx*dx + dy*dy < 25) return;                 // throttle: min ~5px spacing
    canvas.line(_px[_n-1], _py[_n-1], x, y, Theme::ROSE);   // live trace
    canvas.fillCircle(x, y, 1, Theme::GOLD);
  } else {
    // first point: wipe the hint text
    Rect a = area();
    canvas.fillRect(a.x+3, a.y+1, a.w-6, a.h-2, Theme::BG);
    canvas.round(a.x+2, a.y, a.w-4, a.h, 8, Theme::OUTLINE);
    canvas.fillCircle(x, y, 2, Theme::GOLD);
  }
  _px[_n] = x; _py[_n] = y; _n++;
}

void DrawScreen::tick(uint32_t ms) {
  TouchPoint p; touch.raw(p);
  Rect a = area();
  if (p.pressed && a.hit(p.x, p.y)) { _drawing = true; addPoint(p.x, p.y); }
  else if (_drawing && !p.pressed)  { _drawing = false; }   // pen up
}

// Resample the polyline into ~30px segments; each becomes a Step, and a heading change becomes a
// Twirl. Emits the same editable Sequence model (turns->Twirl L/R, runs->Step forward).
void DrawScreen::convertToSequence() {
  Sequence& out = app.st.scratch;
  out.clear(); out.setName("Drawn");
  if (_n < 2) return;
  const float STEP = 30.0f;
  float lx = _px[0], ly = _py[0], curH = 0; bool haveH = false;
  for (int i = 1; i < _n; i++) {
    float dx = _px[i]-lx, dy = _py[i]-ly;
    float d = sqrtf(dx*dx + dy*dy);
    if (d < STEP) continue;
    float h = atan2f(dy, dx);
    if (haveH) {
      float dh = h - curH;
      while (dh >  PI) dh -= 2*PI;
      while (dh < -PI) dh += 2*PI;
      float deg = dh * 180.0f / PI;
      if (fabsf(deg) > 25.0f && !out.full()) {
        Step t{ deg < 0 ? TWIRL_LEFT : TWIRL_RIGHT,
                (int16_t)constrain((int)lroundf(fabsf(deg)/90.0f), 1, 4), 0 };
        out.append(t);
      }
    }
    if (!out.full()) { Step s{ STEP_FORWARD, 1, 0 }; out.append(s); }
    curH = h; haveH = true; lx = _px[i]; ly = _py[i];
  }
}

void DrawScreen::onTap(int x, int y) {
  // only the toolbar reacts to taps; strokes inside the area are handled in tick()
  switch (toolbar::hit(x, y, 3)) {
    case 0: app.go(ScreenId::Play); return;
    case 1: reset(); enter(); return;
    case 2:
      convertToSequence();
      if (!app.st.scratch.empty()) { app.st.sel = 0; app.go(ScreenId::Editor); }
      return;
  }
}
