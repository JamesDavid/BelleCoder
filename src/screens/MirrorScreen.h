#pragma once
#include "../core/Screen.h"
#include "../model/Move.h"

// Live Mirror / Puppet — wave the wand and Belle moves in near-real-time (spin the wand -> she
// spins). Reads the IMU continuously and fires the mapped move on a throttle. IMU-gated.
class MirrorScreen : public Screen {
public:
  void enter() override;
  void tick(uint32_t ms) override;
  void onTap(int x, int y) override;
private:
  void drawLive(MoveId m, bool active);
  uint32_t _lastFire = 0;
  MoveId   _shown = MOVE_COUNT;
  bool     _active = false;
};
extern MirrorScreen mirrorScreen;
