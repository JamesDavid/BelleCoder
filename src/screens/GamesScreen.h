#pragma once
#include "../core/Screen.h"
#include "../model/Move.h"

// Simon Says — Belle demonstrates a growing sequence of moves; the child repeats it by tapping
// the matching tiles. On a linked doll, a necklace-button press starts each round (pal notify).
class GamesScreen : public Screen {
public:
  void enter() override;
  void tick(uint32_t ms) override;
  void onTap(int x, int y) override;
private:
  enum class Phase { Idle, Demo, Input, Over };
  void newGame();
  void addStep();
  void draw();
  void flash(int tile);

  static const int MAXLEN = 32;
  uint8_t _seq[MAXLEN];
  int     _len = 0, _inputPos = 0, _demoPos = 0;
  Phase   _phase = Phase::Idle;
  uint32_t _demoUntil = 0;
  int     _flashTile = -1; uint32_t _flashUntil = 0;
};
extern GamesScreen gamesScreen;
