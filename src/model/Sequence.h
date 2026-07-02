#pragma once
// model/Sequence.h — the shared dance model (SPEC §6). BOTH the tap editor and the
// motion-capture pipeline emit this exact type, so storage/editing/runner are reused.
#include <stdint.h>
#include "Move.h"

#ifndef MAX_STEPS
#define MAX_STEPS 32
#endif
#define SEQ_NAME_LEN 24

struct Step {
  MoveId   move;
  int16_t  p1;         // primary param (steps / index / ms / repeat count)
  uint8_t  repeatGrp;  // 0 = none; >0 groups consecutive steps for REPEAT (SPEC §6)
};

struct Sequence {
  char    name[SEQ_NAME_LEN];
  uint8_t count;
  Step    steps[MAX_STEPS];

  void clear()                { name[0] = 0; count = 0; }
  bool full() const           { return count >= MAX_STEPS; }
  bool empty() const          { return count == 0; }

  bool insertAt(int idx, Step s);   // insert before idx (idx==count => append)
  bool append(Step s)         { return insertAt(count, s); }
  bool removeAt(int idx);
  bool moveUp(int idx);
  bool moveDown(int idx);
  void setName(const char* n);
};
