#include "SettingsScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../core/Nav.h"
#include "../pal/pal.h"
#include <Arduino.h>

SettingsScreen settingsScreen;

static Rect rowTier() { return { 10, 40,  SCREEN_W-20, 40 }; }
static Rect rowGap()  { return { 10, 88,  SCREEN_W-20, 40 }; }
static Rect gapMinus(){ return { SCREEN_W-96, 92, 32, 32 }; }
static Rect gapPlus() { return { SCREEN_W-52, 92, 32, 32 }; }

void SettingsScreen::enter() {
  auto& S = app.st;
  canvas.clear(Theme::BG);
  canvas.title("Settings");

  // Tier row
  Rect t = rowTier();
  canvas.card(t.x,t.y,t.w,t.h,Theme::CARD);
  canvas.text("Mode", t.x+10, t.y+14, Theme::TEXT, 1, Align::L);
  canvas.button(t.x+t.w-150, t.y+5, 70, 30, "Kid",  S.tier==Tier::Kid?Theme::GOLD:Theme::BG_ALT, S.tier==Tier::Kid?Theme::BG:Theme::TEXT);
  canvas.button(t.x+t.w-76,  t.y+5, 70, 30, "Adv",  S.tier==Tier::Advanced?Theme::GOLD:Theme::BG_ALT, S.tier==Tier::Advanced?Theme::BG:Theme::TEXT);

  // Gap row
  Rect g = rowGap();
  canvas.card(g.x,g.y,g.w,g.h,Theme::CARD);
  char gv[32]; snprintf(gv,sizeof gv,"Step gap  %dms", S.globalGapMs);
  canvas.text(gv, g.x+10, g.y+14, Theme::TEXT, 1, Align::L);
  Rect mm=gapMinus(), pp=gapPlus();
  canvas.button(mm.x,mm.y,mm.w,mm.h,"-",Theme::BG_ALT,Theme::TEXT);
  canvas.button(pp.x,pp.y,pp.w,pp.h,"+",Theme::BG_ALT,Theme::TEXT);

  // Capture-only rows: shown ONLY when an IMU is present (SPEC §9)
  if (S.imuPresent) {
    canvas.card(10,136,SCREEN_W-20,40,Theme::CARD);
    char sv[28]; snprintf(sv,sizeof sv,"Sensitivity  %d", S.sensitivity);
    canvas.text(sv, 20, 150, Theme::TEXT, 1, Align::L);
    canvas.button(SCREEN_W-96,140,32,32,"-",Theme::BG_ALT,Theme::GOLD);
    canvas.button(SCREEN_W-52,140,32,32,"+",Theme::BG_ALT,Theme::GOLD);
    // info (compact)
    char l2[40]; snprintf(l2,sizeof l2,"IMU: %s   BLE: %s", S.imuLabel, pal::simulated()?"SIM":"linked");
    canvas.text(l2, 12, 182, Theme::TEXT_DIM, 1, Align::L);
  } else {
    canvas.fillRound(10,136,SCREEN_W-20,58,6,Theme::BG_ALT);
    char l1[40]; snprintf(l1,sizeof l1,"BLE: %s", pal::simulated()?"SIMULATE":"linked");
    canvas.text(l1, 20, 144, Theme::TEXT_DIM, 1, Align::L);
    canvas.text("IMU: none (capture hidden)", 20, 162, Theme::TEXT_DIM, 1, Align::L);
    canvas.text("BelleCoder " BELLE_FW_VERSION, 20, 180, Theme::TEXT_DIM, 1, Align::L);
  }

  nav::drawBack();
}

void SettingsScreen::onTap(int x, int y) {
  auto& S = app.st;
  if (nav::backRect().hit(x,y)) { app.go(ScreenId::Home); return; }
  Rect t=rowTier();
  if (Rect{t.x+t.w-150,t.y+5,70,30}.hit(x,y)) { S.tier=Tier::Kid; enter(); return; }
  if (Rect{t.x+t.w-76, t.y+5,70,30}.hit(x,y)) { S.tier=Tier::Advanced; enter(); return; }
  if (gapMinus().hit(x,y)) { S.globalGapMs = max(0, S.globalGapMs-50); enter(); return; }
  if (gapPlus().hit(x,y))  { S.globalGapMs = min(1000, S.globalGapMs+50); enter(); return; }
  if (S.imuPresent) {
    if (Rect{SCREEN_W-96,140,32,32}.hit(x,y)) { S.sensitivity = max(1, S.sensitivity-1); enter(); return; }
    if (Rect{SCREEN_W-52,140,32,32}.hit(x,y)) { S.sensitivity = min(10, S.sensitivity+1); enter(); return; }
  }
}
