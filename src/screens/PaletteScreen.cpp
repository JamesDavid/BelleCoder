#include "PaletteScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../core/Nav.h"
#include "EditorScreen.h"
#include <Arduino.h>

PaletteScreen paletteScreen;

// M0: show categories + the moves in the active category as tappable tiles; tapping a move
// appends it to the scratch with its default param and returns to the editor. Kid tier filters
// to kidTier moves. (Param stepper for parameterised moves is added in M1.)
static const MoveCat KID_CATS[]  = { MoveCat::Move, MoveCat::Arms, MoveCat::Spin, MoveCat::Dance };
static const MoveCat ADV_CATS[]  = { MoveCat::Move, MoveCat::Spin, MoveCat::Arms, MoveCat::Head,
                                     MoveCat::Sound, MoveCat::Dance, MoveCat::Wait, MoveCat::Repeat };

static int catCount() { return app.st.tier==Tier::Kid ? 4 : 8; }
static MoveCat catAt(int i) { return app.st.tier==Tier::Kid ? KID_CATS[i] : ADV_CATS[i]; }

static Rect tabRect(int i, int n) {
  int w = SCREEN_W / n;
  return { i*w, 28, w, 24 };
}

// collect moves in the active category (respecting tier)
static int movesInCat(MoveCat c, MoveId* out) {
  int n=0;
  for (int m=0; m<MOVE_COUNT; m++) {
    if (catOf((MoveId)m) != c) continue;
    if (app.st.tier==Tier::Kid && !moveInfo((MoveId)m).kidTier) continue;
    out[n++] = (MoveId)m;
  }
  return n;
}

static Rect tileRect(int idx) {
  const int cols = (app.st.tier==Tier::Kid) ? 2 : 3;
  const int m=8, top=58, gap=8;
  int tw = (SCREEN_W - m*2 - gap*(cols-1)) / cols;
  int th = (app.st.tier==Tier::Kid) ? 66 : 50;
  int r = idx / cols, c = idx % cols;
  return { m + c*(tw+gap), top + r*(th+gap), tw, th };
}

void PaletteScreen::enter() {
  int n = catCount();
  if (_cat >= n) _cat = 0;
  canvas.clear(Theme::BG);
  canvas.title(app.st.tier==Tier::Kid ? "Pick a Move" : "Palette");

  // tabs
  for (int i=0;i<n;i++) {
    Rect t=tabRect(i,n);
    bool on = (i==_cat);
    canvas.fillRect(t.x,t.y,t.w,t.h, on?Theme::BG_ALT:Theme::BG);
    if (on) canvas.fillRect(t.x, t.y+t.h-2, t.w, 2, Theme::GOLD);
    canvas.text(catName(catAt(i)), t.x+t.w/2, t.y+8, on?Theme::GOLD:Theme::TEXT_DIM, 1, Align::C);
  }

  // move tiles
  MoveId ms[MOVE_COUNT]; int mc = movesInCat(catAt(_cat), ms);
  int big = (app.st.tier==Tier::Kid);
  for (int i=0;i<mc;i++) {
    Rect q=tileRect(i);
    canvas.fillRound(q.x,q.y,q.w,q.h,8,Theme::CARD);
    canvas.moveGlyph(ms[i], q.x+q.w/2, q.y+ (big?24:18), big?16:12, catColor(catAt(_cat)));
    canvas.text(moveInfo(ms[i]).label, q.x+q.w/2, q.y+q.h-12, Theme::TEXT, 1, Align::C);
  }
  nav::drawBack();
}

void PaletteScreen::onTap(int x, int y) {
  if (nav::backRect().hit(x,y)) { app.go(ScreenId::Editor); return; }
  int n = catCount();
  for (int i=0;i<n;i++) if (tabRect(i,n).hit(x,y)) { _cat=i; enter(); return; }

  MoveId ms[MOVE_COUNT]; int mc = movesInCat(catAt(_cat), ms);
  for (int i=0;i<mc;i++) if (tileRect(i).hit(x,y)) {
    const MoveInfo& mi = moveInfo(ms[i]);
    if (mi.hasParam) {
      // route parameterised moves through the editor's stepper to set the value first
      editorScreen.beginNewParam(ms[i], mi.pDef);
    } else {
      editorScreen.insertMove(ms[i], 0);
    }
    app.go(ScreenId::Editor);
    return;
  }
}
