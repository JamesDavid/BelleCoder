#include "Sequence.h"
#include <string.h>

bool Sequence::insertAt(int idx, Step s) {
  if (count >= MAX_STEPS) return false;
  if (idx < 0) idx = 0;
  if (idx > count) idx = count;
  for (int i = count; i > idx; i--) steps[i] = steps[i - 1];
  steps[idx] = s;
  count++;
  return true;
}

bool Sequence::removeAt(int idx) {
  if (idx < 0 || idx >= count) return false;
  for (int i = idx; i < count - 1; i++) steps[i] = steps[i + 1];
  count--;
  return true;
}

bool Sequence::moveUp(int idx) {
  if (idx <= 0 || idx >= count) return false;
  Step t = steps[idx]; steps[idx] = steps[idx - 1]; steps[idx - 1] = t;
  return true;
}

bool Sequence::moveDown(int idx) {
  if (idx < 0 || idx >= count - 1) return false;
  Step t = steps[idx]; steps[idx] = steps[idx + 1]; steps[idx + 1] = t;
  return true;
}

void Sequence::setName(const char* n) {
  strncpy(name, n ? n : "", SEQ_NAME_LEN - 1);
  name[SEQ_NAME_LEN - 1] = 0;
}
