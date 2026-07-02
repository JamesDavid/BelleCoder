#pragma once
#include "../core/Screen.h"
#include "../capture/Capture.h"

// "Dance to Code" (SPEC §9). Record -> live recognition feedback -> Stop -> classified steps
// drop into the scratch sequence and open the editor for review. Shown only when an IMU exists.
class DanceScreen : public Screen {
public:
  void enter() override;
  void tick(uint32_t ms) override;
  void onTap(int x, int y) override;
private:
  void drawIdle();
  void drawLive();
  void drawHint(LiveHint h);
  Capture  _cap;
  uint32_t _flashUntil = 0;
  MoveId   _flashMove = STEP_FORWARD; int16_t _flashP = 0;
  LiveHint _lastHint = LiveHint::None;
  int      _lastCount = -1;
};
extern DanceScreen danceScreen;
