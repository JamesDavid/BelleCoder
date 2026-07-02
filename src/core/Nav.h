#pragma once
// core/Nav.h — shared bottom "Back" bar + hit region used by sub-screens.
#include "Canvas.h"
#include "Theme.h"
#include "Hit.h"

namespace nav {
  inline Rect backRect() { return { 6, SCREEN_H - 30, 70, 26 }; }
  inline void drawBack() {
    Rect b = backRect();
    canvas.fillRound(b.x, b.y, b.w, b.h, 6, Theme::CARD);
    canvas.text("< Back", b.x + b.w/2, b.y + 9, Theme::TEXT, 1, Align::C);
  }
}
