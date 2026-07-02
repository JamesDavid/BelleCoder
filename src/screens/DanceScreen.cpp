#include "DanceScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../core/Nav.h"
#include <Arduino.h>

DanceScreen danceScreen;

static Rect btnRecord() { return { SCREEN_W/2-70, 150, 140, 44 }; }

void DanceScreen::enter() {
  canvas.clear(Theme::BG);
  canvas.title("Dance to Code");
  canvas.text("Wave, spin & bounce the wand", SCREEN_W/2, 44, Theme::TEXT, 1, Align::C);
  canvas.text("and watch it become code!", SCREEN_W/2, 60, Theme::TEXT, 1, Align::C);
  // big spin motif
  canvas.catGlyph(MoveCat::Spin, SCREEN_W/2, 108, 30, Theme::C_SPIN);
  Rect b=btnRecord();
  canvas.button(b.x,b.y,b.w,b.h,"Record",Theme::RED,Theme::TEXT);
  nav::drawBack();
}

void DanceScreen::onTap(int x, int y) {
  if (nav::backRect().hit(x,y)) { app.go(ScreenId::Home); return; }
  // M6: start capture. For now, acknowledge.
}
