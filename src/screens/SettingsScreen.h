#pragma once
#include "../core/Screen.h"

class SettingsScreen : public Screen {
public:
  void enter() override;
  void onTap(int x, int y) override;
};
extern SettingsScreen settingsScreen;
