#pragma once
// model/Move.h — the instruction set (SPEC §5). MoveId is the abstract move the UI places;
// the PAL maps each to Belle's real MotorRun/audio bytes. Catalog metadata (label, category,
// param range) drives the palette and the editor rows.
#include <stdint.h>

enum class MoveCat : uint8_t { Move, Spin, Arms, Head, Light, Sound, Dance, Wait, Repeat, COUNT };

enum MoveId : uint8_t {
  STEP_FORWARD, STEP_BACK,
  TWIRL_LEFT, TWIRL_RIGHT,
  ARM_L_UP, ARM_L_DOWN, ARM_R_UP, ARM_R_DOWN, ARMS_UP, ARMS_DOWN,
  HEAD_LEFT, HEAD_RIGHT, HEAD_CENTER,
  LED_COLOR,            // dress light -> palette colour (p1 = colour index)
  PLAY_SONG, PLAY_PHRASE, PLAY_DANCE,
  WAIT,
  REPEAT,               // advanced control block (repeat count in p1)
  MOVE_COUNT
};

struct MoveInfo {
  MoveId      id;
  MoveCat     cat;
  const char* label;    // short editor/palette label
  bool        hasParam;
  int16_t     pMin, pMax, pDef;
  const char* pUnit;    // e.g. "x", "", "ms", "#"
  bool        kidTier;  // shown in the Kid palette
};

// Catalog (definition in Move.cpp). Indexed by MoveId.
const MoveInfo& moveInfo(MoveId m);
MoveCat         catOf(MoveId m);
const char*     catName(MoveCat c);
uint16_t        catColor(MoveCat c);   // defined in Canvas.cpp (uses Theme)
