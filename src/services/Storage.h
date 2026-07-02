#pragma once
// services/Storage.h — LittleFS persistence for named sequences (SPEC §6, §M2). Sequences are
// compact JSON in /seq/<name>.json; presets ship read-only in /presets/. Also converts the
// extracted {arm,l,r} preset dances into the editable Sequence model.
#include <stdint.h>
#include "../model/Sequence.h"

namespace storage {
  bool begin();                                   // mount LittleFS (format on first run)

  // saved user sequences
  bool save(const Sequence& seq);                 // -> /seq/<name>.json
  bool load(const char* name, Sequence& out);     // <- /seq/<name>.json
  bool remove(const char* name);
  int  listSaved(char names[][SEQ_NAME_LEN], int maxN);

  // read-only presets shipped in /presets/*.json (raw {arm,l,r} choreography)
  int  listPresets(char names[][SEQ_NAME_LEN], int maxN);
  bool loadPreset(const char* name, Sequence& out);   // converts to editable steps
}
