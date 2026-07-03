#include "Touch.h"
#include "Display.h"
#include <Arduino.h>

Touch touch;

void Touch::begin() {
  // XPT2046 is configured inside the LGFX device; nothing extra needed here.
}

void Touch::injectTap(int16_t x, int16_t y) {
  _injX = x; _injY = y; _hasInject = true;
}

void Touch::injectDraw(int16_t x, int16_t y, bool up) {
  _drawX = x; _drawY = y; _drawUp = up; _drawActive = true;
}

// Continuous position for drawing: injected strokes take priority, else live pressure.
bool Touch::raw(TouchPoint& p) {
  if (_drawActive) {
    p.x = _drawX; p.y = _drawY; p.pressed = !_drawUp;
    if (_drawUp) _drawActive = false;   // pen-up ends the injected stroke
    return true;
  }
  int32_t rx, ry;
  bool down = display.gfx().getTouch(&rx, &ry);
  p.x = (int16_t)rx; p.y = (int16_t)ry; p.pressed = down;
  return true;
}

bool Touch::poll(TouchPoint& p) {
  // Injected taps take priority and fire as a single press edge.
  if (_hasInject) {
    _hasInject = false;
    p.x = _injX; p.y = _injY; p.pressed = true;
    return true;
  }
  int32_t rx, ry;
  bool down = display.gfx().getTouch(&rx, &ry);
  uint32_t now = millis();
  bool edge = false;
  if (down && !_wasDown && (now - _lastMs) > 120) {
    p.x = (int16_t)rx; p.y = (int16_t)ry; p.pressed = true;
    _lastMs = now;
    edge = true;
  }
  _wasDown = down;
  return edge;
}
