#include "HomeScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../pal/pal.h"
#include <Arduino.h>

HomeScreen homeScreen;

// button grid rects (2 cols x 3 rows in the body)
static Rect btnAt(int col, int row) {
  const int m = 10, top = 66, gap = 8;
  const int bw = (SCREEN_W - m*2 - gap) / 2;
  const int bh = 44;
  return { m + col*(bw+gap), top + row*(bh+gap), bw, bh };
}

void HomeScreen::enter() {
  auto& S = app.st;
  canvas.clear(Theme::BG);
  canvas.title("BelleCoder");
  // enchanted-rose motif dot
  canvas.fillCircle(16, 13, 5, Theme::ROSE);

  // status band
  canvas.fillRound(10, 32, SCREEN_W-20, 28, 6, Theme::BG_ALT);
  bool sim = pal::simulated();
  canvas.fillCircle(24, 46, 5, sim ? Theme::GOLD : Theme::GREEN);
  char st[48];
  snprintf(st, sizeof st, sim ? "SIMULATE  (no doll linked)" : "Belle linked");
  canvas.text(st, 38, 40, sim ? Theme::GOLD : Theme::GREEN, 1, Align::L);
  if (S.tier == Tier::Advanced) {
    char im[32]; snprintf(im, sizeof im, "IMU: %s", S.imuLabel);
    canvas.text(im, SCREEN_W-14, 40, Theme::TEXT_DIM, 1, Align::R);
  }

  // buttons
  auto b = [&](int c,int r,const char* label,uint16_t fill){ Rect q=btnAt(c,r); canvas.button(q.x,q.y,q.w,q.h,label,fill,Theme::TEXT); };
  b(0,0, "New",      Theme::C_MOVE);
  b(1,0, "Load",     Theme::CARD);
  b(0,1, "Editor",   Theme::C_DANCE);
  b(1,1, "Run",      Theme::GREEN);
  if (S.imuPresent) b(0,2, "Dance to Code", Theme::C_SPIN);   // hidden entirely when absent (SPEC §9)
  b(1,2, "Settings", Theme::CARD);

  // footer
  char f[40]; snprintf(f, sizeof f, "\"%s\"  %d steps", S.scratch.name, S.scratch.count);
  canvas.text(f, SCREEN_W/2, SCREEN_H-14, Theme::TEXT_DIM, 1, Align::C);
}

void HomeScreen::onTap(int x, int y) {
  auto& S = app.st;
  if (btnAt(0,0).hit(x,y)) { S.scratch.clear(); S.scratch.setName("My Dance"); S.sel=0; app.go(ScreenId::Editor); return; }
  if (btnAt(1,0).hit(x,y)) { app.go(ScreenId::Load); return; }
  if (btnAt(0,1).hit(x,y)) { app.go(ScreenId::Editor); return; }
  if (btnAt(1,1).hit(x,y)) { app.go(ScreenId::Run); return; }
  if (S.imuPresent && btnAt(0,2).hit(x,y)) { app.go(ScreenId::Dance); return; }
  if (btnAt(1,2).hit(x,y)) { app.go(ScreenId::Settings); return; }
}
