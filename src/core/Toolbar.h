#pragma once
// core/Toolbar.h — a bottom action bar laid out as N equal, non-overlapping slots.
// One shared component so no screen hand-places button rects (which is how Back overlapped
// +Add). Any screen with a multi-button bottom bar draws + hit-tests through this — no
// per-screen coordinate math, no wide back-bar colliding with a slotted row.
#include "Canvas.h"
#include "Theme.h"
#include "Hit.h"

namespace toolbar {
  constexpr int H = 26;
  constexpr int MARGIN = 4, GAP = 4;

  // slot i of n, evenly divided across the screen width — guaranteed non-overlapping.
  inline Rect slot(int i, int n) {
    int bw = (SCREEN_W - 2*MARGIN - GAP*(n-1)) / n;
    return { MARGIN + i*(bw+GAP), SCREEN_H - 30, bw, H };
  }

  struct Btn { const char* label; uint16_t fill; };

  inline void draw(const Btn* btns, int n) {
    for (int i = 0; i < n; i++) {
      Rect r = slot(i, n);
      canvas.button(r.x, r.y, r.w, r.h, btns[i].label, btns[i].fill, Theme::TEXT);
    }
  }

  // index of the tapped button, or -1 (returns -1 for taps above the bar)
  inline int hit(int x, int y, int n) {
    for (int i = 0; i < n; i++) if (slot(i, n).hit(x, y)) return i;
    return -1;
  }
}
