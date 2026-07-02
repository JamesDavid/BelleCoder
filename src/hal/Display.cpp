#include "Display.h"
#include <Arduino.h>

Display display;

void Display::begin() {
  _lcd.init();
  _lcd.setRotation(DISPLAY_DEFAULT_ROTATION);
  _lcd.setColorDepth(16);
  _lcd.fillScreen(TFT_BLACK);
  setBacklight(100);
}

void Display::setBacklight(uint8_t pct) {
  if (pct > 100) pct = 100;
  _lcd.setBrightness((uint8_t)(pct * 255 / 100));
}

// Read the panel back one row at a time (no big heap allocation on the no-PSRAM ESP32)
// and stream it. Host decoder: scripts/dev/shot.py.
void Display::streamScreenshot() {
  const int w = SCREEN_W, h = SCREEN_H;
  static uint16_t row[SCREEN_W];   // 640 bytes, static — safe on the small heap
  Serial.printf("GBSHOT %d %d\n", w, h);
  Serial.flush();
  for (int y = 0; y < h; y++) {
    _lcd.readRect(0, y, w, 1, row);
    // stream little-endian (low byte first); host reads standard RGB565 r5g6b5
    Serial.write(reinterpret_cast<uint8_t*>(row), w * 2);
    Serial.flush();
  }
}
