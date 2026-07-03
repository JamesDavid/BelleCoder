#include "SongsScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../core/Toolbar.h"
#include "../catalog/Catalog.h"
#include "../pal/pal.h"
#include <Arduino.h>

SongsScreen songsScreen;

static const int TOP = 30, ROWH = 28, VIS = 6;
static Rect up()   { return { SCREEN_W-30, TOP,             26, 26 }; }
static Rect down() { return { SCREEN_W-30, TOP+ROWH*VIS-26, 26, 26 }; }
static const toolbar::Btn TB[] = { { "Back", Theme::CARD }, { "Stop", Theme::RED } };

void SongsScreen::drawList() {
  int n = catalog::songCount();
  canvas.fillRect(0, TOP, SCREEN_W, VIS*ROWH, Theme::BG);
  for (int r = 0; r < VIS; r++) {
    int i = _scroll + r; if (i >= n) break;
    int y = TOP + r*ROWH;
    bool playing = (i == _playing);
    canvas.card(6, y+2, SCREEN_W-44, ROWH-4, playing?Theme::CARD_SEL:Theme::CARD, playing);
    canvas.moveGlyph(PLAY_SONG, 22, y+ROWH/2, 8, playing?Theme::GOLD:Theme::C_SOUND);
    char nm[28]; snprintf(nm, sizeof nm, "%s", catalog::songName(i+1));
    canvas.text(nm, 40, y+ROWH/2-4, Theme::TEXT, 1, Align::L);
    if (playing) canvas.text("\x10", SCREEN_W-52, y+ROWH/2-4, Theme::GOLD, 1, Align::R); // play glyph
  }
  if (n > VIS) {
    Rect u=up(), d=down();
    canvas.button(u.x,u.y,u.w,u.h,"^",Theme::CARD,Theme::TEXT);
    canvas.button(d.x,d.y,d.w,d.h,"v",Theme::CARD,Theme::TEXT);
  }
}

void SongsScreen::enter() {
  canvas.clear(Theme::BG);
  canvas.title("Dance-Along");
  drawList();
  toolbar::draw(TB, 2);
}

void SongsScreen::onTap(int x, int y) {
  switch (toolbar::hit(x, y, 2)) {
    case 0: app.go(ScreenId::Play); return;
    case 1: _playing = -1; pal::settle(); drawList(); return;
  }
  int n = catalog::songCount();
  if (n > VIS && up().hit(x,y))   { if(_scroll>0){_scroll--; drawList();} return; }
  if (n > VIS && down().hit(x,y)) { if(_scroll+VIS<n){_scroll++; drawList();} return; }
  if (y>=TOP && y<TOP+VIS*ROWH && x < SCREEN_W-40) {
    int i = _scroll + (y-TOP)/ROWH;
    if (i < n) { _playing = i; pal::sendMove(PLAY_SONG, i+1); drawList(); }
  }
}
