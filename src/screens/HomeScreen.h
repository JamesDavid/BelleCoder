#pragma once
#include "../core/Screen.h"
#include <stdint.h>

class HomeScreen : public Screen {
public:
  void enter() override;
  void tick(uint32_t ms) override;
  void onTap(int x, int y) override;
private:
  void drawStatusBand();
  uint8_t _lastBleState = 255;
};
extern HomeScreen homeScreen;
