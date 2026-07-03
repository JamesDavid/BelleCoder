#include "MirrorScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../core/Nav.h"
#include "../imu/Imu.h"
#include "../pal/pal.h"
#include <Arduino.h>
#include <math.h>

MirrorScreen mirrorScreen;

void MirrorScreen::enter() {
  imu::calibrateGyro(120);
  canvas.clear(Theme::BG);
  canvas.title("Live Mirror");
  canvas.text("Wave the wand — Belle copies you!", SCREEN_W/2, 40, Theme::TEXT, 1, Align::C);
  _shown = MOVE_COUNT; _active = false;
  drawLive(MOVE_COUNT, false);
  nav::drawBack();
}

void MirrorScreen::drawLive(MoveId m, bool active) {
  int cx = SCREEN_W/2, cy = 116;
  canvas.fillRect(cx-70, cy-46, 140, 92, Theme::BG);
  if (!active || m >= MOVE_COUNT) { canvas.circle(cx, cy, 10, Theme::TEXT_DIM); return; }
  canvas.moveGlyph(m, cx, cy, 34, catColor(catOf(m)));
}

void MirrorScreen::tick(uint32_t ms) {
  ImuSample s;
  if (!imu::read(s)) return;

  // thresholds scale with sensitivity (like capture)
  float sens = constrain(app.st.sensitivity, 1, 10);
  float gyroTh = 55.0f - sens*4.0f, bounceTh = 0.55f - sens*0.035f, tiltTh = 0.45f - sens*0.02f;
  float az = s.az - 1.0f;

  MoveId m = MOVE_COUNT; bool active = true;
  if      (fabsf(s.gz) > gyroTh) m = (s.gz < 0) ? TWIRL_LEFT : TWIRL_RIGHT;
  else if (fabsf(az)   > bounceTh) m = ARMS_UP;
  else if (fabsf(s.ay) > tiltTh)   m = (s.ay < 0) ? HEAD_LEFT : HEAD_RIGHT;
  else active = false;

  if (m != _shown || active != _active) { drawLive(m, active); _shown = m; _active = active; }

  // fire the mapped move on a throttle so Belle mirrors live without flooding BLE
  if (active && (ms - _lastFire) > 200) { pal::sendMove(m, 1); _lastFire = ms; }
}

void MirrorScreen::onTap(int x, int y) {
  if (nav::backRect().hit(x,y)) { pal::settle(); app.go(ScreenId::Play); return; }
}
