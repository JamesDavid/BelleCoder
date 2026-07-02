#include "HomeScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../pal/pal.h"
#include "../services/Ble.h"
#include "../services/Battery.h"
#include <Arduino.h>

HomeScreen homeScreen;

static Rect statusBand() { return { 10, 32, SCREEN_W-20, 28 }; }

static uint16_t bleColor() {
  switch (ble::state()) {
    case ble::State::Connected: return Theme::GREEN;
    case ble::State::Scanning:
    case ble::State::Connecting:return Theme::TEAL;
    case ble::State::Found:     return Theme::GOLD;
    case ble::State::Failed:    return Theme::RED;
    default:                    return Theme::GOLD;   // idle / simulate
  }
}

void HomeScreen::drawStatusBand() {
  Rect b = statusBand();
  canvas.fillRound(b.x, b.y, b.w, b.h, 6, Theme::BG_ALT);
  canvas.fillCircle(b.x+14, b.y+14, 5, bleColor());
  const char* txt = (ble::state()==ble::State::Idle && pal::simulated())
                    ? "SIMULATE  (tap to find Belle)" : ble::statusText();
  canvas.text(txt, b.x+28, b.y+8, bleColor(), 1, Align::L);
  _lastBleState = (uint8_t)ble::state();
}

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
  // battery / USB indicator (wand power, M8)
  {
    int bx = SCREEN_W-34, by = 8;
    canvas.rect(bx, by, 22, 11, Theme::TEXT_DIM);
    canvas.fillRect(bx+22, by+3, 2, 5, Theme::TEXT_DIM);
    if (battery::onUsb()) {
      canvas.text("USB", bx-2, by+2, Theme::TEXT_DIM, 1, Align::R);
    } else {
      int pct = battery::percent();
      uint16_t c = pct<20?Theme::RED:pct<50?Theme::GOLD:Theme::GREEN;
      canvas.fillRect(bx+2, by+2, (18*pct)/100, 7, c);
    }
  }

  // status band (BLE scan/link state; tap to find/connect)
  drawStatusBand();
  if (S.tier == Tier::Advanced) {
    char im[32]; snprintf(im, sizeof im, "IMU: %s", S.imuLabel);
    canvas.text(im, SCREEN_W-14, 62, Theme::TEXT_DIM, 1, Align::R);
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

void HomeScreen::tick(uint32_t) {
  if ((uint8_t)ble::state() != _lastBleState) drawStatusBand();  // reflect async scan/connect
}

void HomeScreen::onTap(int x, int y) {
  auto& S = app.st;
  // tap the status band to scan, then connect
  if (statusBand().hit(x,y)) {
    switch (ble::state()) {
      case ble::State::Found:     ble::connectFound(); break;
      case ble::State::Connected: ble::disconnect();   break;
      default:                    ble::startScan();    break;
    }
    drawStatusBand();
    return;
  }
  if (btnAt(0,0).hit(x,y)) { S.scratch.clear(); S.scratch.setName("My Dance"); S.sel=0; app.go(ScreenId::Editor); return; }
  if (btnAt(1,0).hit(x,y)) { app.go(ScreenId::Load); return; }
  if (btnAt(0,1).hit(x,y)) { app.go(ScreenId::Editor); return; }
  if (btnAt(1,1).hit(x,y)) { app.go(ScreenId::Run); return; }
  if (S.imuPresent && btnAt(0,2).hit(x,y)) { app.go(ScreenId::Dance); return; }
  if (btnAt(1,2).hit(x,y)) { app.go(ScreenId::Settings); return; }
}
