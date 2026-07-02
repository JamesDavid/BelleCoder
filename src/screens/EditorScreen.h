#pragma once
#include "../core/Screen.h"
#include "../model/Move.h"

// Sequence editor (SPEC §7.2): step list + toolbar (Add/Delete/Move) and an inline param
// stepper for parameterised moves. The palette hands new parameterised moves here to set the
// value before insert.
class EditorScreen : public Screen {
public:
  void enter() override;
  void onTap(int x, int y) override;

  // Called by the palette: begin the stepper for a brand-new parameterised move.
  void beginNewParam(MoveId m, int16_t initial);
  // Called by the palette: a param-less move was picked; insert it directly.
  void insertMove(MoveId m, int16_t p1);

private:
  enum class Mode { List, Stepper };
  Mode _mode = Mode::List;

  // stepper state
  MoveId  _sMove = STEP_FORWARD;
  int16_t _sVal  = 0;
  bool    _sNew  = false;      // true = inserting new; false = editing existing at _sIdx
  int     _sIdx  = 0;

  int _scroll = 0;

  void drawList();
  void drawStepper();
  void openStepperFor(int idx);
  void commitStepper();
  void ensureVisible();
};
extern EditorScreen editorScreen;
