#pragma once
#include "../core/Screen.h"

// Move palette (SPEC §7.3). M0: category tabs + move grid shell; M1 wires selection + param
// stepper and the Kid-tier reduced set.
class PaletteScreen : public Screen {
public:
  void enter() override;
  void onTap(int x, int y) override;
private:
  int _cat = 0;   // active category tab
};
extern PaletteScreen paletteScreen;
