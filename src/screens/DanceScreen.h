#pragma once
#include "../core/Screen.h"

// "Dance to Code" capture (SPEC §9). Shown only when an IMU is detected. M0: placeholder;
// full record/segment/classify + live feedback lands in M6.
class DanceScreen : public Screen {
public:
  void enter() override;
  void onTap(int x, int y) override;
};
extern DanceScreen danceScreen;
