#pragma once
// pal/pal.h — Protocol Abstraction Layer (SPEC §10). ALL doll-specific knowledge lives here.
// The UI/runner call this stable surface and never change. BLE_ENABLED gates SIMULATE vs real.
//
// The RE phase (SPEC §11) is complete — real UUIDs/opcodes are in docs/BLE_PROTOCOL.md and
// wired in at M4. Until BLE_ENABLED=1, sendMove() logs to Serial + the Run screen so the whole
// app is exercisable with no doll present.
#include <stdint.h>
#include "../model/Move.h"

#ifndef BLE_ENABLED
#define BLE_ENABLED 0            // 0 = SIMULATE, 1 = real NimBLE
#endif

namespace pal {
  // recovered from the app decompile (docs/BLE_PROTOCOL.md)
  constexpr const char* DOLL_NAME_PREFIX = "DanceCD";
  constexpr const char* CMD_CHAR_UUID    = "51901383-030F-4859-B643-256B0B2F5562"; // app->toy write
  constexpr const char* NTF_CHAR_UUID    = "51901382-030F-4859-B643-256B0B2F5562"; // toy->app notify
  constexpr bool        WRITE_NO_RESP    = true;

  void     begin();
  bool     connect();
  bool     isConnected();
  bool     simulated();                        // true when BLE_ENABLED==0 or not linked
  bool     sendMove(MoveId m, int16_t p1);     // fire one move at the doll (or simulate)
  uint16_t durationOf(MoveId m, int16_t p1);   // ms the move occupies (paces the runner/UI)
  bool     supports(MoveId m);                 // drives palette greying
  void     settle();                           // neutral "stop" (brake wheels, arms down)

  // last simulated/real command line, for the Run screen readout
  const char* lastLog();
}
