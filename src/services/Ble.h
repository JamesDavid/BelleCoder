#pragma once
// services/Ble.h — NimBLE central (SPEC §M3). Scans for the Belle doll (adv name "DanceCD"),
// connects, discovers the recovered write/notify characteristics (service UUID is found
// dynamically — docs/BLE_PROTOCOL.md), subscribes, and exposes a write for the PAL (M4).
#include <stdint.h>

namespace ble {
  enum class State : uint8_t { Off, Idle, Scanning, Found, Connecting, Connected, Failed };

  void        begin();
  void        startScan();          // async; results pumped in tick()
  void        connectFound();       // connect to the best DanceCD match
  void        disconnect();
  void        tick();               // call from loop(): drives the async state machine
  bool        isConnected();
  State       state();
  const char* statusText();
  int         rssi();

  bool        writeCmd(const uint8_t* data, int len);   // used by the PAL at M4
}
