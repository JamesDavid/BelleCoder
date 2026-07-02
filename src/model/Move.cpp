#include "Move.h"

// One row per MoveId, in enum order. kidTier = shown in the simplified Kid palette
// (Move / Arms / Spin / Dance with oversized icons — SPEC §7.3).
static const MoveInfo CATALOG[MOVE_COUNT] = {
  //  id            cat            label          param  min max def unit  kid
  { STEP_FORWARD, MoveCat::Move,  "Step Fwd",    true,  1,  4,  1, "x",  true  },
  { STEP_BACK,    MoveCat::Move,  "Step Back",   true,  1,  4,  1, "x",  true  },
  { TWIRL_LEFT,   MoveCat::Spin,  "Twirl L",     true,  1,  4,  1, "x",  true  },
  { TWIRL_RIGHT,  MoveCat::Spin,  "Twirl R",     true,  1,  4,  1, "x",  true  },
  { ARM_L_UP,     MoveCat::Arms,  "L Arm Up",    false, 0,  0,  0, "",   false },
  { ARM_L_DOWN,   MoveCat::Arms,  "L Arm Dn",    false, 0,  0,  0, "",   false },
  { ARM_R_UP,     MoveCat::Arms,  "R Arm Up",    false, 0,  0,  0, "",   false },
  { ARM_R_DOWN,   MoveCat::Arms,  "R Arm Dn",    false, 0,  0,  0, "",   false },
  { ARMS_UP,      MoveCat::Arms,  "Arms Up",     false, 0,  0,  0, "",   true  },
  { ARMS_DOWN,    MoveCat::Arms,  "Arms Down",   false, 0,  0,  0, "",   true  },
  { HEAD_LEFT,    MoveCat::Head,  "Head L",      false, 0,  0,  0, "",   false },
  { HEAD_RIGHT,   MoveCat::Head,  "Head R",      false, 0,  0,  0, "",   false },
  { HEAD_CENTER,  MoveCat::Head,  "Head Mid",    false, 0,  0,  0, "",   false },
  { PLAY_SONG,    MoveCat::Sound, "Song",        true,  1, 10,  1, "#",  false },
  { PLAY_PHRASE,  MoveCat::Sound, "Phrase",      true,  1,421,  1, "#",  false },
  { PLAY_DANCE,   MoveCat::Dance, "Dance",       true,  1, 12,  1, "#",  true  },
  { WAIT,         MoveCat::Wait,  "Wait",        true, 100,3000,500,"ms", false },
  { REPEAT,       MoveCat::Repeat,"Repeat",      true,  2,  8,  2, "x",  false },
};

const MoveInfo& moveInfo(MoveId m) { return CATALOG[m < MOVE_COUNT ? m : 0]; }
MoveCat catOf(MoveId m) { return CATALOG[m < MOVE_COUNT ? m : 0].cat; }

const char* catName(MoveCat c) {
  switch (c) {
    case MoveCat::Move:   return "Move";
    case MoveCat::Spin:   return "Spin";
    case MoveCat::Arms:   return "Arms";
    case MoveCat::Head:   return "Head";
    case MoveCat::Sound:  return "Sound";
    case MoveCat::Dance:  return "Dance";
    case MoveCat::Wait:   return "Wait";
    case MoveCat::Repeat: return "Repeat";
    default:              return "?";
  }
}
