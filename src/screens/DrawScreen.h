#pragma once
#include "../core/Screen.h"
#include "../model/Sequence.h"

// Draw-a-Dance — trace a path on the touchscreen and Belle drives it (the original app's
// connect-dots / free-draw, using our extracted {arm,l,r} model). The polyline is resampled and
// converted to editable moves: turns -> Twirl, straight runs -> Step. Output opens in the editor.
class DrawScreen : public Screen {
public:
  void enter() override;
  void tick(uint32_t ms) override;
  void onTap(int x, int y) override;
private:
  void reset();
  void addPoint(int x, int y);
  void convertToSequence();

  static const int MAXP = 128;
  int16_t _px[MAXP], _py[MAXP];
  int     _n = 0;
  bool    _drawing = false;
  uint32_t _lastAdd = 0;
};
extern DrawScreen drawScreen;
