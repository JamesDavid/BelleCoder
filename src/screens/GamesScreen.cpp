#include "GamesScreen.h"
#include "../core/App.h"
#include "../core/Canvas.h"
#include "../core/Theme.h"
#include "../core/Hit.h"
#include "../core/Toolbar.h"
#include "../pal/pal.h"
#include <Arduino.h>

GamesScreen gamesScreen;

// four game tiles -> four Belle moves
struct Tile { MoveId move; int16_t p1; const char* label; uint16_t col; };
static const Tile TILES[4] = {
  { TWIRL_RIGHT,  1, "Spin",  Theme::C_SPIN },
  { ARMS_UP,      0, "Arms",  Theme::C_ARMS },
  { STEP_FORWARD, 1, "Step",  Theme::C_MOVE },
  { LED_COLOR,    1, "Light", Theme::GOLD   },
};

// 2x2 tile grid that fits entirely above the bottom toolbar (no overlap)
static Rect tileRect(int i) {
  const int m=10, top=46, gap=8, tw=(SCREEN_W-2*m-gap)/2, th=74;
  return { m + (i%2)*(tw+gap), top + (i/2)*(th+gap), tw, th };
}

void GamesScreen::newGame() {
  randomSeed(millis() ^ (analogRead(34) << 3));
  _len = 0; _inputPos = 0; _demoPos = 0;
  addStep();
  _phase = Phase::Demo; _demoUntil = 0;
}

void GamesScreen::addStep() { if (_len < MAXLEN) _seq[_len++] = random(4); }

void GamesScreen::flash(int tile) { _flashTile = tile; _flashUntil = millis() + 350; }

void GamesScreen::draw() {
  canvas.clear(Theme::BG);
  canvas.title("Simon Says");
  // status line
  const char* msg = "Tap Start (or press Belle's necklace)";
  if      (_phase==Phase::Demo)  msg = "Watch Belle...";
  else if (_phase==Phase::Input) msg = "Your turn!";
  else if (_phase==Phase::Over)  msg = "Oops! Tap to play again";
  canvas.text(msg, SCREEN_W/2, 34, Theme::GOLD, 1, Align::C);
  char rd[16]; snprintf(rd, sizeof rd, "Round %d", _len>0?_len:0);
  if (_phase!=Phase::Idle) canvas.text(rd, SCREEN_W-8, 34, Theme::TEXT_DIM, 1, Align::R);

  for (int i=0;i<4;i++) {
    Rect r=tileRect(i);
    bool lit = (i==_flashTile && millis()<_flashUntil);
    canvas.fillRound(r.x,r.y,r.w,r.h,10, lit?TILES[i].col:Theme::CARD);
    canvas.moveGlyph(TILES[i].move, r.x+30, r.y+r.h/2, 14, lit?Theme::BG:TILES[i].col);
    canvas.text(TILES[i].label, r.x+r.w/2+14, r.y+r.h/2-4, Theme::TEXT, 2, Align::C);
  }
  // bottom toolbar: Back + a start/restart action — never overlaps the tiles
  const char* action = (_phase==Phase::Over) ? "Play Again" : (_phase==Phase::Idle) ? "Start" : "Restart";
  toolbar::Btn tb[2] = { { "Back", Theme::CARD }, { action, Theme::GREEN } };
  toolbar::draw(tb, 2);
}

void GamesScreen::enter() { _phase = Phase::Idle; _len = 0; _flashTile = -1; draw(); }

void GamesScreen::tick(uint32_t ms) {
  // a necklace press starts a round when idle/over (real doll)
  if ((_phase==Phase::Idle || _phase==Phase::Over) && pal::takeNecklacePress()) { newGame(); draw(); }

  if (_phase==Phase::Demo) {
    if (ms >= _demoUntil) {
      if (_demoPos < _len) {
        int t = _seq[_demoPos++];
        flash(t);
        pal::sendMove(TILES[t].move, TILES[t].p1);   // Belle actually performs it
        _demoUntil = ms + 650;
        draw();
      } else {
        _phase = Phase::Input; _inputPos = 0; draw();
      }
    }
  }
  // clear a finished flash
  if (_flashTile>=0 && ms>=_flashUntil) { _flashTile=-1; if(_phase==Phase::Input) draw(); }
}

void GamesScreen::onTap(int x, int y) {
  switch (toolbar::hit(x, y, 2)) {
    case 0: app.go(ScreenId::Play); return;
    case 1: newGame(); draw(); return;      // Start / Play Again / Restart
  }
  if (_phase==Phase::Input) {
    for (int i=0;i<4;i++) if (tileRect(i).hit(x,y)) {
      flash(i);
      pal::sendMove(TILES[i].move, TILES[i].p1);
      if (i == _seq[_inputPos]) {
        _inputPos++;
        if (_inputPos >= _len) { addStep(); _phase=Phase::Demo; _demoPos=0; _demoUntil=millis()+500; }
      } else {
        _phase = Phase::Over;
      }
      draw();
      return;
    }
  }
}
