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
  // LED palette (index 0..7): name + 24-bit RGB (SetLED opcode 0x15).
  struct Led { const char* name; uint8_t r, g, b; };
  const Led LEDS[] = {
    {"Red",    255,  0,  0}, {"Pink",   255, 60,150}, {"Gold",   255,190, 20},
    {"Green",   30,220, 60}, {"Teal",    20,200,200}, {"Blue",    40, 90,255},
    {"Violet", 170, 70,255}, {"White",  255,255,255},
  };
}

namespace catalog {

int songCount()  { return sizeof(SONGS)  / sizeof(SONGS[0]); }
int danceCount() { return sizeof(DANCES) / sizeof(DANCES[0]); }

const char* songName(int i1)  { return (i1>=1 && i1<=songCount())  ? SONGS[i1-1]  : "?"; }
const char* danceName(int i1) { return (i1>=1 && i1<=danceCount()) ? DANCES[i1-1] : "?"; }

int ledCount() { return sizeof(LEDS)/sizeof(LEDS[0]); }
const char* ledName(int i0) { return (i0>=0 && i0<ledCount()) ? LEDS[i0].name : "?"; }
void ledRGB(int i0, uint8_t& r, uint8_t& g, uint8_t& b) {
  int i = (i0>=0 && i0<ledCount()) ? i0 : 0;
  r = LEDS[i].r; g = LEDS[i].g; b = LEDS[i].b;
}
uint16_t ledColor565(int i0) {
  uint8_t r,g,b; ledRGB(i0,r,g,b);
  return ((r&0xF8)<<8) | ((g&0xFC)<<3) | (b>>3);
}

void label(MoveId m, int16_t idx, char* out, int n) {
  switch (m) {
    case PLAY_SONG:   snprintf(out, n, "%s", songName(idx));  break;
    case PLAY_DANCE:  snprintf(out, n, "%s", danceName(idx)); break;
    case PLAY_PHRASE: snprintf(out, n, "Phrase #%d", idx);    break;  // enriched in M2
    case LED_COLOR:   snprintf(out, n, "Light: %s", ledName(idx)); break;
    default:          snprintf(out, n, "#%d", idx);           break;
  }
}

} // namespace catalog
