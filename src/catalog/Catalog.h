#pragma once
// catalog/Catalog.h — friendly names for indexed audio/dance moves (SPEC §5). Built-in name
// tables (recovered in docs/BLE_PROTOCOL.md); M2 enriches phrases from data/audio_catalog.csv
// on LittleFS. Keeps the UI showing "The Rose Waltz" instead of raw #7.
#include <stdint.h>
#include "../model/Move.h"

namespace catalog {
  void        label(MoveId m, int16_t idx, char* out, int n);   // friendly text for a step
  int         songCount();
  int         danceCount();
  const char* songName(int i1);      // 1-based
  const char* danceName(int i1);     // 1-based
}
