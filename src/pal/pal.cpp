#include "pal.h"
#include "../services/Ble.h"
#include <Arduino.h>
#include <LittleFS.h>

// Belle is a WHEELED doll (docs/BLE_PROTOCOL.md): moves compose from MotorRun(20) start/brake,
// arm poses via Motor1Goto/CamGoto(25), audio via PlayAudioSequence(16), dances via PlaySequence(23).

namespace {
  // opcodes
  enum { OP_AUDIO=16, OP_MOTOR=20, OP_LED=21, OP_SEQ=23, OP_CAMGOTO=25, OP_VOL=35, OP_APPMODE=80 };
  // BLEToyMotor / State / Direction
  enum { M_ARMS=0, M_LWHEEL=1, M_RWHEEL=2, M_BOTH=3 };
  enum { S_RUN=0, S_BRAKE=1, S_COAST=2 };
  enum { D_FWD=0, D_REV=1 };
  // BLEToyCamPosition
  enum { CAM_DOWN=0, CAM_OUT=4, CAM_LOUT=8, CAM_LUP=12, CAM_LDOWN=16, CAM_UP=20, CAM_FWD=24 };

  char g_log[100] = "idle";

  // deferred brake so motion moves self-terminate without blocking (pal::tick)
  bool     g_brakePending = false;
  uint32_t g_brakeAt = 0;

  // phrase index -> playback ms, loaded from /phrase_durations.csv (docs/BLE_PROTOCOL.md)
  const int PHRASE_MAX = 803;
  uint16_t* g_phraseMs = nullptr;

  // ---- transport: log always; write when linked ----
  bool tx(const uint8_t* p, int n, const char* human) {
    // human-readable byte trace
    char hex[64]; int o=0;
    for (int i=0;i<n && o<(int)sizeof(hex)-3;i++) o += snprintf(hex+o, sizeof(hex)-o, "%02X ", p[i]);
    snprintf(g_log, sizeof g_log, "%s [%s]", human, hex);
    bool wrote = false;
#if BLE_ENABLED
    if (ble::isConnected()) wrote = ble::writeCmd(p, n);
#endif
    Serial.printf("[MOVE] %s%s\n", g_log, wrote ? "" : "  (SIM)");
    return true;
  }

  void motorRun(uint8_t motor, uint8_t state, uint8_t power, uint8_t dir, const char* h) {
    uint8_t p[5] = { OP_MOTOR, motor, state, power, dir }; tx(p, 5, h);
  }
  void camGoto(uint8_t pos, const char* h) {
    uint8_t p[3] = { OP_CAMGOTO, 0 /*dir=closest*/, pos }; tx(p, 3, h);
  }
  void playAudio(uint16_t idx, const char* h) {
    uint8_t p[3] = { OP_AUDIO, (uint8_t)(idx & 0xFF), (uint8_t)(idx >> 8) }; tx(p, 3, h);
  }
  void playSeq(uint16_t hl, const char* h) {
    uint8_t p[4] = { OP_SEQ, 0x02, (uint8_t)(hl & 0xFF), (uint8_t)(hl >> 8) }; tx(p, 4, h);
  }

  void loadDurations() {
    g_phraseMs = (uint16_t*)calloc(PHRASE_MAX, sizeof(uint16_t));
    if (!g_phraseMs) return;
    File f = LittleFS.open("/phrase_durations.csv", "r");
    if (!f) { Serial.println("[pal] no phrase_durations.csv"); return; }
    f.readStringUntil('\n');                   // header
    int n = 0;
    while (f.available()) {
      String line = f.readStringUntil('\n');   // ePxxx,idx,seconds
      int c1 = line.indexOf(','), c2 = line.indexOf(',', c1+1);
      if (c1 < 0 || c2 < 0) continue;
      int idx = line.substring(c1+1, c2).toInt();
      float sec = line.substring(c2+1).toFloat();
      if (idx >= 0 && idx < PHRASE_MAX) { g_phraseMs[idx] = (uint16_t)(sec*1000); n++; }
    }
    f.close();
    Serial.printf("[pal] loaded %d phrase durations\n", n);
  }
  uint16_t phraseMs(int idx, uint16_t fallback) {
    if (g_phraseMs && idx >= 0 && idx < PHRASE_MAX && g_phraseMs[idx]) return g_phraseMs[idx];
    return fallback;
  }
}

namespace pal {

void begin() {
  loadDurations();
  Serial.printf("[PAL] ready  name=%s  write=%s  mode=%s\n",
                DOLL_NAME_PREFIX, WRITE_NO_RESP ? "no-resp" : "resp",
                simulated() ? "SIMULATE" : "BLE");
}

bool simulated() {
#if BLE_ENABLED
  return !ble::isConnected();
#else
  return true;
#endif
}

bool connect()     { return ble::isConnected(); }
bool isConnected() { return ble::isConnected(); }
bool supports(MoveId m) { return m < MOVE_COUNT; }

uint16_t durationOf(MoveId m, int16_t p1) {
  switch (m) {
    case STEP_FORWARD: case STEP_BACK:   return 420 * (p1<1?1:p1);
    case TWIRL_LEFT:   case TWIRL_RIGHT: return 500 * (p1<1?1:p1);
    case ARM_L_UP: case ARM_L_DOWN: case ARM_R_UP: case ARM_R_DOWN:
    case ARMS_UP: case ARMS_DOWN:        return 450;
    case HEAD_LEFT: case HEAD_RIGHT: case HEAD_CENTER: return 350;
    case PLAY_SONG:                      return phraseMs(250 + (p1<1?1:p1), 4000);
    case PLAY_PHRASE:                    return phraseMs(p1, 2500);
    case PLAY_DANCE:                     return 6000;
    case WAIT:                           return p1<0?0:p1;
    case REPEAT:                         return 0;
    default:                             return 300;
  }
}

bool sendMove(MoveId m, int16_t p1) {
  const int n = (p1<1?1:p1);
  switch (m) {
    case STEP_FORWARD:
      motorRun(M_BOTH, S_RUN, 200, D_FWD, "Step fwd");
      g_brakePending = true; g_brakeAt = millis() + durationOf(m,p1); break;
    case STEP_BACK:
      motorRun(M_BOTH, S_RUN, 200, D_REV, "Step back");
      g_brakePending = true; g_brakeAt = millis() + durationOf(m,p1); break;
    case TWIRL_LEFT:
      motorRun(M_LWHEEL, S_RUN, 200, D_REV, "Twirl L");
      motorRun(M_RWHEEL, S_RUN, 200, D_FWD, "Twirl L");
      g_brakePending = true; g_brakeAt = millis() + durationOf(m,p1); break;
    case TWIRL_RIGHT:
      motorRun(M_LWHEEL, S_RUN, 200, D_FWD, "Twirl R");
      motorRun(M_RWHEEL, S_RUN, 200, D_REV, "Twirl R");
      g_brakePending = true; g_brakeAt = millis() + durationOf(m,p1); break;
    case ARMS_UP:    camGoto(CAM_UP,   "Arms up");   break;
    case ARMS_DOWN:  camGoto(CAM_DOWN, "Arms down"); break;
    case ARM_L_UP:   camGoto(CAM_LUP,  "L arm up");  break;
    case ARM_L_DOWN: camGoto(CAM_LDOWN,"L arm dn");  break;
    case ARM_R_UP:   camGoto(CAM_FWD,  "R arm up");  break;
    case ARM_R_DOWN: camGoto(CAM_DOWN, "R arm dn");  break;
    case HEAD_LEFT:  camGoto(CAM_LOUT, "Head L");    break;
    case HEAD_RIGHT: camGoto(CAM_OUT,  "Head R");    break;
    case HEAD_CENTER:camGoto(CAM_DOWN, "Head mid");  break;
    case PLAY_SONG:  playAudio(250 + n, "Song");     break;   // songs 251..260
    case PLAY_PHRASE:playAudio(p1,      "Phrase");   break;
    case PLAY_DANCE: playSeq(n,         "Dance");    break;   // HL index tuned live (§11.4)
    case WAIT:       snprintf(g_log,sizeof g_log,"wait %d ms",p1); Serial.printf("[MOVE] %s\n",g_log); break;
    case REPEAT:     break;
    default: break;
  }
  (void)n;
  return true;
}

void tick() {
  if (g_brakePending && (int32_t)(millis() - g_brakeAt) >= 0) {
    g_brakePending = false;
    motorRun(M_BOTH, S_BRAKE, 0, D_FWD, "brake");
  }
}

void settle() {
  g_brakePending = false;
  motorRun(M_BOTH, S_BRAKE, 0, D_FWD, "settle");
  camGoto(CAM_DOWN, "settle arms");
}

const char* lastLog() { return g_log; }

} // namespace pal
