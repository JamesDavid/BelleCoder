#include "pal.h"
#include <Arduino.h>

// Belle is a WHEELED doll (docs/BLE_PROTOCOL.md): moves are composed from MotorRun (opcode 20)
// start/wait/brake, arm poses via Motor1Goto (25), audio via PlayAudioSequence (16). At M4 these
// build real packets and go out over NimBLE; in SIMULATE we just format + log the bytes.

namespace {
  char g_log[96] = "idle";
  bool g_connected = false;

  // opcode bytes we would emit (documented; used verbatim at M4)
  enum { OP_AUDIO=16, OP_MOTOR=20, OP_LED=21, OP_SEQ=23, OP_CAMGOTO=25, OP_VOL=35, OP_APPMODE=80 };
}

namespace pal {

void begin() {
  snprintf(g_log, sizeof g_log, "PAL %s", simulated() ? "SIMULATE" : "BLE");
  Serial.printf("[PAL] %s  name=%s  write=%s\n",
                simulated() ? "SIMULATE" : "BLE(real)", DOLL_NAME_PREFIX,
                WRITE_NO_RESP ? "no-resp" : "resp");
}

bool simulated() {
#if BLE_ENABLED
  return !g_connected;
#else
  return true;
#endif
}

bool connect()      { g_connected = false; return false; }   // real scan/connect arrives at M3/M4
bool isConnected()  { return g_connected; }

bool supports(MoveId m) {
  // All catalog moves are exposed by the doll; refined once §11.4 live-confirms the set.
  return m < MOVE_COUNT;
}

// Per-move time budget that paces the runner + Run-screen highlight (SPEC §8). Locomotion/spin
// are motor-on windows; audio uses catalog-ish durations (tightened at M4 from phrase_durations.csv).
uint16_t durationOf(MoveId m, int16_t p1) {
  switch (m) {
    case STEP_FORWARD: case STEP_BACK:   return 420 * (p1 < 1 ? 1 : p1);
    case TWIRL_LEFT:   case TWIRL_RIGHT: return 500 * (p1 < 1 ? 1 : p1);
    case ARM_L_UP: case ARM_L_DOWN: case ARM_R_UP: case ARM_R_DOWN:
    case ARMS_UP: case ARMS_DOWN:        return 450;
    case HEAD_LEFT: case HEAD_RIGHT: case HEAD_CENTER: return 350;
    case PLAY_SONG:                      return 4000;
    case PLAY_PHRASE:                    return 2500;
    case PLAY_DANCE:                     return 6000;
    case WAIT:                           return p1 < 0 ? 0 : p1;
    case REPEAT:                         return 0;
    default:                             return 300;
  }
}

bool sendMove(MoveId m, int16_t p1) {
  const MoveInfo& mi = moveInfo(m);
  // Format the packet we WOULD send (real emission wired at M4).
  switch (catOf(m)) {
    case MoveCat::Move:
      snprintf(g_log, sizeof g_log, "MotorRun BothWheels %s p=%d [%d,3,0,200,%d]",
               m==STEP_FORWARD?"fwd":"back", p1, OP_MOTOR, m==STEP_FORWARD?0:1); break;
    case MoveCat::Spin:
      snprintf(g_log, sizeof g_log, "MotorRun L/R opposed %s x%d [%d,1..2,0,200,d]",
               m==TWIRL_LEFT?"L":"R", p1, OP_MOTOR); break;
    case MoveCat::Arms: case MoveCat::Head:
      snprintf(g_log, sizeof g_log, "CamGoto %s [%d,dir,pos]", mi.label, OP_CAMGOTO); break;
    case MoveCat::Sound:
      snprintf(g_log, sizeof g_log, "PlayAudio %s #%d [%d,%02X,%02X]",
               mi.label, p1, OP_AUDIO, p1 & 0xFF, (p1>>8) & 0xFF); break;
    case MoveCat::Dance:
      snprintf(g_log, sizeof g_log, "PlaySequence dance #%d [%d,02,..]", p1, OP_SEQ); break;
    case MoveCat::Wait:
      snprintf(g_log, sizeof g_log, "wait %d ms", p1); break;
    default:
      snprintf(g_log, sizeof g_log, "%s", mi.label); break;
  }
  Serial.printf("[MOVE] %s%s\n", g_log, simulated() ? "  (SIM)" : "");
  return true;
}

void settle() {
  snprintf(g_log, sizeof g_log, "settle: brake wheels + arms down");
  Serial.printf("[MOVE] %s\n", g_log);
}

const char* lastLog() { return g_log; }

} // namespace pal
