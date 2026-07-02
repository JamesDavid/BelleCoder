#pragma once
// core/Screen.h — screen interface + ids. The App manager owns one instance per id and routes
// taps/ticks to the active screen. Real finger + serial-injected taps arrive identically.
#include <stdint.h>

enum class ScreenId : uint8_t {
  Home, Editor, Palette, Run, Settings, Dance, COUNT
};

class Screen {
public:
  virtual ~Screen() {}
  virtual void enter() {}                 // full repaint on show
  virtual void tick(uint32_t /*ms*/) {}   // periodic (animation / runner)
  virtual void onTap(int /*x*/, int /*y*/) {}
};
