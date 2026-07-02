#pragma once
#include "../core/Screen.h"
#include "../model/Sequence.h"

// Load screen (SPEC §7.1 "Load"): lists shipped presets + saved sequences; tap to load into the
// scratch buffer and open the editor.
class LoadScreen : public Screen {
public:
  void enter() override;
  void onTap(int x, int y) override;
private:
  void refresh();
  static const int MAXN = 40;
  char _names[MAXN][SEQ_NAME_LEN];
  bool _isPreset[MAXN];
  int  _n = 0;
  int  _scroll = 0;
};
extern LoadScreen loadScreen;
