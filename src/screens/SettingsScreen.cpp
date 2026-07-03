#include "SettingsScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../core/Nav.h"
#include "../pal/pal.h"
#include <Arduino.h>

SettingsScreen settingsScreen;

// compact 32px rows so everything fits above the back bar (SPEC §7.6)
static const int RH = 32;
static Rect row(int i)   { return { 10, 30 + i*(RH+4), SCREEN_W-20, RH }; }
static Rect minusOf(int i){ return { SCREEN_W-92, 32 + i*(RH+4), 30, 28 }; }
static Rect plusOf(int i) { return { SCREEN_W-48, 32 + i*(RH+4), 30, 28 }; }
// tier toggle buttons live in row 0
static Rect kidBtn()  { Rect r=row(0); return { r.x+r.w-150, r.y+2, 70, 28 }; }
static Rect advBtn()  { Rect r=row(0); return { r.x+r.w-76,  r.y+2, 70, 28 }; }
static Rect bounceBtn(){ Rect r=row(4); return { r.x+r.w-120, r.y+2, 112, 28 }; }

void SettingsScreen::enter() {
  auto& S = app.st;
  pal::requestBelleBattery();
  canvas.clear(Theme::BG);
  canvas.title("Settings");

  // row 0 — Mode
  { Rect r=row(0); canvas.card(r.x,r.y,r.w,r.h,Theme::CARD);
    canvas.text("Mode", r.x+10, r.y+12, Theme::TEXT, 1, Align::L);
    Rect k=kidBtn(), a=advBtn();
    canvas.button(k.x,k.y,k.w,k.h,"Kid", S.tier==Tier::Kid?Theme::GOLD:Theme::BG_ALT, S.tier==Tier::Kid?Theme::BG:Theme::TEXT);
    canvas.button(a.x,a.y,a.w,a.h,"Adv", S.tier==Tier::Advanced?Theme::GOLD:Theme::BG_ALT, S.tier==Tier::Advanced?Theme::BG:Theme::TEXT);
  }
  // row 1 — Step gap
  { Rect r=row(1); canvas.card(r.x,r.y,r.w,r.h,Theme::CARD);
    char v[24]; snprintf(v,sizeof v,"Step gap  %dms", S.globalGapMs);
    canvas.text(v, r.x+10, r.y+12, Theme::TEXT, 1, Align::L);
    Rect m=minusOf(1),p=plusOf(1);
    canvas.button(m.x,m.y,m.w,m.h,"-",Theme::BG_ALT,Theme::GOLD);
    canvas.button(p.x,p.y,p.w,p.h,"+",Theme::BG_ALT,Theme::GOLD);
  }
  // row 2 — Belle sound (volume) + her battery when linked
  { Rect r=row(2); canvas.card(r.x,r.y,r.w,r.h,Theme::CARD);
    char v[32]; int bb = pal::belleBattery();
    if (bb >= 0) snprintf(v,sizeof v,"Belle %d%%   vol %d", bb, S.belleVolume);
    else         snprintf(v,sizeof v,"Belle sound   %d", S.belleVolume);
    canvas.text(v, r.x+10, r.y+12, Theme::TEXT, 1, Align::L);
    Rect m=minusOf(2),p=plusOf(2);
    canvas.button(m.x,m.y,m.w,m.h,"-",Theme::BG_ALT,Theme::GOLD);
    canvas.button(p.x,p.y,p.w,p.h,"+",Theme::BG_ALT,Theme::GOLD);
  }
  // rows 3-4 — capture (IMU only)
  if (S.imuPresent) {
    Rect r=row(3); canvas.card(r.x,r.y,r.w,r.h,Theme::CARD);
    char v[24]; snprintf(v,sizeof v,"Sensitivity  %d", S.sensitivity);
    canvas.text(v, r.x+10, r.y+12, Theme::TEXT, 1, Align::L);
    Rect m=minusOf(3),p=plusOf(3);
    canvas.button(m.x,m.y,m.w,m.h,"-",Theme::BG_ALT,Theme::GOLD);
    canvas.button(p.x,p.y,p.w,p.h,"+",Theme::BG_ALT,Theme::GOLD);
    Rect r4=row(4); canvas.card(r4.x,r4.y,r4.w,r4.h,Theme::CARD);
    canvas.text("Bounce", r4.x+10, r4.y+12, Theme::TEXT, 1, Align::L);
    Rect b=bounceBtn();
    canvas.button(b.x,b.y,b.w,b.h, S.bounceToArms?"-> Arms":"-> Step", Theme::BG_ALT, Theme::GOLD);
  } else {
    canvas.text("IMU: none (capture hidden)", 16, 148, Theme::TEXT_DIM, 1, Align::L);
    char l[40]; snprintf(l,sizeof l,"BLE: %s   %s", pal::simulated()?"SIMULATE":"linked", BELLE_FW_VERSION);
    canvas.text(l, 16, 168, Theme::TEXT_DIM, 1, Align::L);
  }
  nav::drawBack();
}

void SettingsScreen::onTap(int x, int y) {
  auto& S = app.st;
  if (nav::backRect().hit(x,y)) { app.go(ScreenId::Home); return; }
  if (kidBtn().hit(x,y)) { S.tier=Tier::Kid; enter(); return; }
  if (advBtn().hit(x,y)) { S.tier=Tier::Advanced; enter(); return; }
  if (minusOf(1).hit(x,y)) { S.globalGapMs=max(0,S.globalGapMs-50); enter(); return; }
  if (plusOf(1).hit(x,y))  { S.globalGapMs=min(1000,S.globalGapMs+50); enter(); return; }
  if (minusOf(2).hit(x,y)) { S.belleVolume=max(0,S.belleVolume-1); pal::setVolume(S.belleVolume); enter(); return; }
  if (plusOf(2).hit(x,y))  { S.belleVolume=min(5,S.belleVolume+1); pal::setVolume(S.belleVolume); enter(); return; }
  if (S.imuPresent) {
    if (minusOf(3).hit(x,y)) { S.sensitivity=max(1,S.sensitivity-1); enter(); return; }
    if (plusOf(3).hit(x,y))  { S.sensitivity=min(10,S.sensitivity+1); enter(); return; }
    if (bounceBtn().hit(x,y)){ S.bounceToArms=!S.bounceToArms; enter(); return; }
  }
}
