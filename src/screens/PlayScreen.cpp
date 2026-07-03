#include "PlayScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../core/Nav.h"

PlayScreen playScreen;

struct Mode { ScreenId id; const char* label; uint16_t fill; bool imuOnly; };
static const Mode MODES[] = {
  { ScreenId::Draw,   "Draw a Dance", Theme::C_MOVE,  false },
  { ScreenId::Songs,  "Songs",        Theme::C_SOUND, false },
  { ScreenId::Games,  "Simon Says",   Theme::GOLD,    false },
  { ScreenId::Dance,  "Dance to Code",Theme::C_SPIN,  true  },
  { ScreenId::Mirror, "Live Mirror",  Theme::C_DANCE, true  },
};
static const int NMODES = sizeof(MODES)/sizeof(MODES[0]);

// collect the modes that are built + allowed (IMU gating)
int PlayScreen::visible(ScreenId* ids, const char** labels, uint16_t* fills) {
  int n = 0;
  for (int i = 0; i < NMODES; i++) {
    if (MODES[i].imuOnly && !app.st.imuPresent) continue;
    if (!app.screen(MODES[i].id)) continue;              // not built yet -> hide
    ids[n] = MODES[i].id; labels[n] = MODES[i].label; fills[n] = MODES[i].fill; n++;
  }
  return n;
}

static Rect grid(int i) {
  const int m=10, top=40, gap=8, bh=44;
  int bw=(SCREEN_W-2*m-gap)/2;
  return { m + (i%2)*(bw+gap), top + (i/2)*(bh+gap), bw, bh };
}

void PlayScreen::enter() {
  canvas.clear(Theme::BG);
  canvas.title("Play");
  ScreenId ids[NMODES]; const char* labels[NMODES]; uint16_t fills[NMODES];
  int n = visible(ids, labels, fills);
  for (int i = 0; i < n; i++) {
    Rect r = grid(i);
    canvas.button(r.x, r.y, r.w, r.h, labels[i], fills[i], Theme::TEXT);
  }
  nav::drawBack();
}

void PlayScreen::onTap(int x, int y) {
  if (nav::backRect().hit(x,y)) { app.go(ScreenId::Home); return; }
  ScreenId ids[NMODES]; const char* labels[NMODES]; uint16_t fills[NMODES];
  int n = visible(ids, labels, fills);
  for (int i = 0; i < n; i++) if (grid(i).hit(x,y)) { app.go(ids[i]); return; }
}
