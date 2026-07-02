#include "Ble.h"
#include "../pal/pal.h"
#include <Arduino.h>
#include <NimBLEDevice.h>
#include <string>

namespace {
  ble::State        g_state = ble::State::Off;
  char              g_status[40] = "off";
  volatile bool     g_haveMatch = false;
  volatile bool     g_wantConnect = false;
  int               g_rssi = 0;
  NimBLEAddress     g_matchAddr;
  std::string       g_matchName;

  NimBLEClient*             g_client = nullptr;
  NimBLERemoteCharacteristic* g_cmdChar = nullptr;

  void setStatus(ble::State s, const char* t) {
    g_state = s; strncpy(g_status, t, sizeof g_status - 1); g_status[sizeof g_status-1] = 0;
  }

  // ---- scan callbacks ----
  class ScanCB : public NimBLEAdvertisedDeviceCallbacks {
    void onResult(NimBLEAdvertisedDevice* d) override {
      std::string name = d->getName();
      if (name.rfind(pal::DOLL_NAME_PREFIX, 0) == 0) {   // starts with "DanceCD"
        g_matchAddr = d->getAddress();
        g_matchName = name;
        g_rssi = d->getRSSI();
        g_haveMatch = true;
        NimBLEDevice::getScan()->stop();
      }
    }
  };

  void onScanEnded(NimBLEScanResults) {
    if (g_haveMatch) {
      char t[40]; snprintf(t, sizeof t, "Belle found  %ddBm", g_rssi);
      setStatus(ble::State::Found, t);
    } else if (g_state == ble::State::Scanning) {
      setStatus(ble::State::Idle, "no Belle found");
    }
  }

  void notifyCB(NimBLERemoteCharacteristic*, uint8_t* data, size_t len, bool) {
    Serial.printf("[ble] notify %d bytes\n", (int)len);
  }

  // discover the recovered write/notify chars across all services (service UUID is dynamic)
  bool discoverChars() {
    if (!g_client) return false;
    NimBLEUUID cmd(pal::CMD_CHAR_UUID), ntf(pal::NTF_CHAR_UUID);
    auto services = g_client->getServices(true);
    for (auto* svc : *services) {
      auto chars = svc->getCharacteristics(true);
      for (auto* c : *chars) {
        if (c->getUUID() == cmd) g_cmdChar = c;
        if (c->getUUID() == ntf && c->canNotify()) c->subscribe(true, notifyCB);
      }
    }
    return g_cmdChar != nullptr;
  }

  void doConnect() {
    setStatus(ble::State::Connecting, "connecting...");
    if (!g_client) g_client = NimBLEDevice::createClient();
    if (!g_client->connect(g_matchAddr)) {
      setStatus(ble::State::Failed, "connect failed");
      return;
    }
    if (discoverChars()) {
      char t[40]; snprintf(t, sizeof t, "Belle linked  %ddBm", g_rssi);
      setStatus(ble::State::Connected, t);
    } else {
      setStatus(ble::State::Failed, "no command char");
      g_client->disconnect();
    }
  }
}

namespace ble {

void begin() {
  NimBLEDevice::init("BelleCoder");
  NimBLEDevice::setPower(ESP_PWR_LVL_P9);
  setStatus(State::Idle, "not linked");
}

void startScan() {
  g_haveMatch = false;
  g_cmdChar = nullptr;
  setStatus(State::Scanning, "scanning...");
  NimBLEScan* scan = NimBLEDevice::getScan();
  scan->setAdvertisedDeviceCallbacks(new ScanCB(), false);
  scan->setActiveScan(true);
  scan->setInterval(80);
  scan->setWindow(60);
  scan->start(5, onScanEnded, false);   // async, 5s
}

void connectFound() {
  if (g_haveMatch) g_wantConnect = true;
}

void disconnect() {
  if (g_client && g_client->isConnected()) g_client->disconnect();
  setStatus(State::Idle, "not linked");
}

void tick() {
  if (g_wantConnect) { g_wantConnect = false; doConnect(); }
  if (g_state == State::Connected && g_client && !g_client->isConnected())
    setStatus(State::Idle, "link lost");
}

bool isConnected()      { return g_state == State::Connected; }
State state()           { return g_state; }
const char* statusText(){ return g_status; }
int rssi()              { return g_rssi; }

bool writeCmd(const uint8_t* data, int len) {
  if (!g_cmdChar) return false;
  return g_cmdChar->writeValue(data, len, !pal::WRITE_NO_RESP);  // response = !no_resp
}

} // namespace ble
