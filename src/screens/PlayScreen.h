#pragma once
#include "../core/Screen.h"

// Play menu — hub for the creative/game modes (Draw, Songs, Dance-to-Code, Mirror, Simon Says).
// Keeps Home stable while grouping the richer modes; hides IMU-only and unbuilt entries.
class PlayScreen : public Screen {
public:
  void enter() override;
  void onTap(int x, int y) override;
private:
  int visible(ScreenId* ids, const char** labels, uint16_t* fills);
};
extern PlayScreen playScreen;
