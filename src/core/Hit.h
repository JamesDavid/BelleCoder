#pragma once
// core/Hit.h — trivial rect hit-test used by every screen's onTap.
struct Rect {
  int x, y, w, h;
  bool hit(int px, int py) const { return px >= x && px < x + w && py >= y && py < y + h; }
};
