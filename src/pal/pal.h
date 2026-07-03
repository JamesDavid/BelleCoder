#pragma once
// pal/pal.h — Protocol Abstraction Layer (SPEC §10). ALL doll-specific knowledge lives here.
// The UI/runner call this stable surface and never change.
//
// RE complete (docs/BLE_PROTOCOL.md). This builds the REAL packets and, when a doll is linked
// (BLE_ENABLED && ble::isConnected()), writes them over NimBLE. Otherwise it SIMULATEs: logs the
// exact bytes so the whole app is exercisable with no doll. Flip BLE_ENABLED to 1 for live use
// (§11.4 covers the remaining timing/power tuning).
#include <stdint.h>
#include "../model/Move.h"

#ifndef BLE_ENABLED
#define BLE_ENABLED 0            // 0 = force SIMULATE; 1 = write when linked
#endif

namespace pal {
  // recovered from the app decompile (docs/BLE_PROTOCOL.md)
  constexpr const char* DOLL_NAME_PREFIX = "DanceCD";
  constexpr const char* CMD_CHAR_UUID    = "51901383-030F-4859-B643-256B0B2F5562"; // app->toy write
  constexpr const char* NTF_CHAR_UUID    = "51901382-030F-4859-B643-256B0B2F5562"; // toy->app notify
  constexpr bool        WRITE_NO_RESP    = true;

  void     begin();
  void     tick();                             // sends the deferred motor-brake; call from loop()
  bool     connect();
  bool     isConnected();
  bool     simulated();                        // true when not writing to a real doll
  bool     sendMove(MoveId m, int16_t p1);     // build + send (or simulate) one move
  uint16_t durationOf(MoveId m, int16_t p1);   // ms the move occupies (paces runner/UI)
  bool     supports(MoveId m);                 // drives palette greying
  void     settle();                           // neutral stop (brake wheels, arms down)

  void     setVolume(int v);                   // SetToyVolume 0..5
  void     requestBelleBattery();              // RequestLVDValue (doll replies on notify)
  int      belleBattery();                     // -1 unknown, else 0..100 (from notify)
  void     onNotify(const uint8_t* data, int len);   // parse toy->app notifications
  bool     takeNecklacePress();                // one-shot: Belle's necklace button was pressed

  const char* lastLog();
}
