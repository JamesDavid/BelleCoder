#pragma once
#include "../core/Screen.h"

// Non-blocking runner (SPEC §8): expands REPEAT groups to a flat list, sends each move via the
// PAL, waits durationOf()+gap, keeps touch responsive so Stop works. Highlights the live step.
class RunScreen : public Screen {
public:
  void enter() override;
  void tick(uint32_t ms) override;
  void onTap(int x, int y) override;

private:
  enum class St { Idle, Playing, Paused, Done };
  void buildFlat();
  void drawStatus();
  void fireCurrent();

  St       _st = St::Idle;
  int      _flat[128];      // expanded step indices into scratch.steps
  int      _nFlat = 0;
  int      _pos = 0;
  uint32_t _stepUntil = 0;
};
extern RunScreen runScreen;
