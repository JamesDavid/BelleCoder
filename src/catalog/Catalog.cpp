#include "Catalog.h"
#include <stdio.h>

namespace {
  // Song titles, UI index 1..10 -> BLEToyAudioPhraseSong 251..260 (docs/BLE_PROTOCOL.md).
  const char* SONGS[] = {
    "Tale/Time A", "Be Our Guest A", "Tale/Time B", "Be Our Guest B",
    "Something There A", "Something There B",
    "The Rose Waltz", "Fairytale Allegro", "The Fleur-de-lis", "The Beastly Bourree",
  };
  // Preset dances 1..12: the 10 extracted Hasbro Connect dances + 2 BelleCoder originals.
  const char* DANCES[] = {
    "Chip", "Book", "Cogsworth", "Dress", "Horse", "Lumiere",
    "Mirror", "Rose", "Snowflake", "Babette", "YMCA", "Spin-Drop",
  };
}

namespace catalog {

int songCount()  { return sizeof(SONGS)  / sizeof(SONGS[0]); }
int danceCount() { return sizeof(DANCES) / sizeof(DANCES[0]); }

const char* songName(int i1)  { return (i1>=1 && i1<=songCount())  ? SONGS[i1-1]  : "?"; }
const char* danceName(int i1) { return (i1>=1 && i1<=danceCount()) ? DANCES[i1-1] : "?"; }

void label(MoveId m, int16_t idx, char* out, int n) {
  switch (m) {
    case PLAY_SONG:   snprintf(out, n, "%s", songName(idx));  break;
    case PLAY_DANCE:  snprintf(out, n, "%s", danceName(idx)); break;
    case PLAY_PHRASE: snprintf(out, n, "Phrase #%d", idx);    break;  // enriched in M2
    default:          snprintf(out, n, "#%d", idx);           break;
  }
}

} // namespace catalog
