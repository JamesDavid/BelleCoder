#pragma once
// hal/Display.h — owns the LovyanGFX device, backlight, and the serial screenshot readback
// (GridBot-style: 'S' -> framed RGB565 stream -> host PNG). SPEC §M0 debug loop.
#include "LGFX_Config.h"

class Display {
public:
  void begin();
  LGFX& gfx() { return _lcd; }
  int   width()  const { return SCREEN_W; }
  int   height() const { return SCREEN_H; }
  void  setBacklight(uint8_t pct);          // 0..100
  // Stream the whole framebuffer over Serial as: "GBSHOT <w> <h>\n" + w*h*2 bytes RGB565 LE.
  void  streamScreenshot();

private:
  LGFX _lcd;
};

extern Display display;
