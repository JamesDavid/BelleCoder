#pragma once
// hal/Touch.h — touch read + synthetic-tap injection. Real finger taps and serial-injected
// taps ('T x y') feed the same queue so the UI can't tell them apart (GridBot/PIO_DEBUG loop).
#include <stdint.h>

struct TouchPoint { int16_t x, y; bool pressed; };

class Touch {
public:
  void begin();
  // Returns true and fills p on a fresh press edge (debounced). Consumes injected taps first.
  bool poll(TouchPoint& p);
  // Continuous read: current pressed state + position (no debounce), for drawing (SPEC draw-a-dance).
  bool raw(TouchPoint& p);
  void injectTap(int16_t x, int16_t y);     // from the serial debug console
  void injectDraw(int16_t x, int16_t y, bool up);   // serial-driven draw point

private:
  bool     _wasDown = false;
  uint32_t _lastMs  = 0;
  // one-slot injected-tap mailbox
  volatile bool    _hasInject = false;
  volatile int16_t _injX = 0, _injY = 0;
  // injected-draw state (serial-driven strokes)
  volatile bool    _drawActive = false, _drawUp = true;
  volatile int16_t _drawX = 0, _drawY = 0;
};

extern Touch touch;
