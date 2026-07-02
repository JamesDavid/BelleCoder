#include "LoadScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../core/Nav.h"
#include "../services/Storage.h"
#include <Arduino.h>
#include <string.h>

LoadScreen loadScreen;

static const int TOP = 30, ROWH = 30, VIS = 5;
static Rect up()   { return { SCREEN_W-30, TOP,            26, 26 }; }
static Rect down() { return { SCREEN_W-30, TOP+ROWH*VIS-26,26, 26 }; }

void LoadScreen::refresh() {
  _n = 0;
  char tmp[MAXN][SEQ_NAME_LEN];
  int np = storage::listPresets(tmp, MAXN);
  for (int i=0;i<np && _n<MAXN;i++){ strncpy(_names[_n], tmp[i], SEQ_NAME_LEN); _isPreset[_n]=true; _n++; }
  int ns = storage::listSaved(tmp, MAXN - _n);
  for (int i=0;i<ns && _n<MAXN;i++){ strncpy(_names[_n], tmp[i], SEQ_NAME_LEN); _isPreset[_n]=false; _n++; }
}

void LoadScreen::enter() {
  refresh();
  canvas.clear(Theme::BG);
  canvas.title("Load a Dance");
  if (_n == 0) canvas.text("Nothing saved yet.", SCREEN_W/2, 90, Theme::TEXT_DIM, 1, Align::C);

  for (int r=0;r<VIS;r++) {
    int i=_scroll+r; if (i>=_n) break;
    int y=TOP+r*ROWH;
    canvas.card(6, y+2, SCREEN_W-44, ROWH-4, Theme::CARD);
    char nm[SEQ_NAME_LEN]; strncpy(nm,_names[i],SEQ_NAME_LEN); nm[0]=toupper(nm[0]);
    canvas.text(nm, 16, y+ROWH/2-4, Theme::TEXT, 1, Align::L);
    canvas.text(_isPreset[i]?"preset":"saved", SCREEN_W-52, y+ROWH/2-4,
                _isPreset[i]?Theme::GOLD:Theme::TEAL, 1, Align::R);
  }
  if (_n > VIS) {
    Rect u=up(), d=down();
    canvas.button(u.x,u.y,u.w,u.h,"^",Theme::CARD,Theme::TEXT);
    canvas.button(d.x,d.y,d.w,d.h,"v",Theme::CARD,Theme::TEXT);
  }
  nav::drawBack();
}

void LoadScreen::onTap(int x, int y) {
  if (nav::backRect().hit(x,y)) { app.go(ScreenId::Home); return; }
  if (_n>VIS && up().hit(x,y))   { if(_scroll>0){_scroll--; enter();} return; }
  if (_n>VIS && down().hit(x,y)) { if(_scroll+VIS<_n){_scroll++; enter();} return; }

  if (y>=TOP && y<TOP+VIS*ROWH && x < SCREEN_W-40) {
    int i=_scroll+(y-TOP)/ROWH;
    if (i<_n) {
      bool ok = _isPreset[i] ? storage::loadPreset(_names[i], app.st.scratch)
                             : storage::load(_names[i], app.st.scratch);
      if (ok) { app.st.sel = 0; app.go(ScreenId::Editor); }
    }
  }
}
