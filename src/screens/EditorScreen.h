#pragma once
#include "../core/Screen.h"

// Sequence editor (SPEC §7.2). M0: renders the step list + toolbar and navigates to the palette;
// full add/insert/delete/reorder + param steppers land in M1.
class EditorScreen : public Screen {
public:
  void enter() override;
  void onTap(int x, int y) override;
  void drawList();
private:
  int _scroll = 0;
};
extern EditorScreen editorScreen;
