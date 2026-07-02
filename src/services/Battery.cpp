#include "Battery.h"
#include "../hal/Board.h"
#include <Arduino.h>

namespace {
  float    g_v = 0;
  bool     g_usb = true;
  uint32_t g_lastMs = 0;

  float sampleVolts() {
#if PIN_BATT_ADC < 0
    return -1.0f;
#else
    // ESP32 ADC1, 12-bit, ~3.3V ref with 11dB atten. Divider halves Vbat.
    uint32_t acc = 0;
    for (int i = 0; i < 8; i++) acc += analogRead(PIN_BATT_ADC);
    float raw = acc / 8.0f;
    return (raw / 4095.0f) * 3.3f * BATT_DIVIDER;
#endif
  }
}

namespace battery {

void begin() {
#if PIN_BATT_ADC >= 0
  analogReadResolution(12);
  analogSetPinAttenuation(PIN_BATT_ADC, ADC_11db);
#endif
  g_lastMs = 0;
  tick();
}

void tick() {
  uint32_t now = millis();
  if (now - g_lastMs < 3000 && g_lastMs != 0) return;
  g_lastMs = now;
  float v = sampleVolts();
  // Plausible single-cell LiPo window; otherwise treat as USB/no-battery.
  if (v > 3.0f && v < 4.35f) {
    g_usb = false;
    g_v = (g_v <= 0) ? v : g_v * 0.8f + v * 0.2f;   // smooth
  } else {
    g_usb = true;
  }
}

bool onUsb() { return g_usb; }
float volts() { return g_v; }

int percent() {
  // simple LiPo curve: 3.30V=0%, 4.20V=100%
  float p = (g_v - 3.30f) / (4.20f - 3.30f) * 100.0f;
  if (p < 0) p = 0; if (p > 100) p = 100;
  return (int)(p + 0.5f);
}

} // namespace battery
