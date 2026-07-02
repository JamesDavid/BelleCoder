#pragma once
#include "../core/Screen.h"

class HomeScreen : public Screen {
public:
  void enter() override;
  void onTap(int x, int y) override;
};
extern HomeScreen homeScreen;
