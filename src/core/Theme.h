#pragma once
// core/Theme.h — Belle palette + layout constants. Warm gold/rose "Beauty and the Beast"
// look; big touch targets for small hands (SPEC §7).
#include <stdint.h>

// RGB565 colours
namespace Theme {
  constexpr uint16_t BG        = 0x18E3;  // deep plum-charcoal
  constexpr uint16_t BG_ALT    = 0x2145;  // panel fill
  constexpr uint16_t CARD      = 0x2967;  // list row / card
  constexpr uint16_t CARD_SEL  = 0x8305;  // selected row (rose)
  constexpr uint16_t GOLD      = 0xFEA0;  // Belle gold  (accent / titles)
  constexpr uint16_t GOLD_DIM  = 0xB543;
  constexpr uint16_t ROSE      = 0xF9EB;  // enchanted rose
  constexpr uint16_t TEAL      = 0x5D9F;  // spin / info
  constexpr uint16_t GREEN     = 0x3667;  // run / connected
  constexpr uint16_t RED       = 0xE985;  // stop / delete
  constexpr uint16_t TEXT      = 0xFFFF;  // primary text
  constexpr uint16_t TEXT_DIM  = 0xA534;  // secondary text
  constexpr uint16_t OUTLINE   = 0x4A69;  // borders

  // move-category accent colours (palette tabs / step icons)
  constexpr uint16_t C_MOVE    = 0x5D9F;  // locomotion (teal)
  constexpr uint16_t C_SPIN    = 0xC5FF;  // spin (violet)
  constexpr uint16_t C_ARMS    = 0xFD40;  // arms (amber)
  constexpr uint16_t C_HEAD    = 0x4E9F;  // head (blue)
  constexpr uint16_t C_SOUND   = 0xF9EB;  // sound (rose)
  constexpr uint16_t C_DANCE   = 0x9E7F;  // dance (lilac)
  constexpr uint16_t C_WAIT    = 0x7BEF;  // wait (grey)
  constexpr uint16_t C_REPEAT  = 0xFEA0;  // repeat (gold)
}
