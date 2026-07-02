#include "Imu.h"
#include "../hal/Board.h"
#include <Arduino.h>
#include <Wire.h>

// Register-level drivers (no external lib) for the supported parts. WHO_AM_I / CHIP_ID checks
// avoid a false positive from an unrelated device at the same address (SPEC §9). MPU-6050 is the
// reference read path; ICM-42670 / LSM6DSO are implemented per datasheet. BNO parts are
// detect-only (their fused SHTP/register protocol is out of scope) -> begin() fails -> fail-safe.

namespace {
  ImuKind  g_kind = IMU_NONE;
  uint8_t  g_addr = 0;
  bool     g_begun = false;
  bool     g_synth = false;
  char     g_label[24] = "none";
  float    g_gbx=0, g_gby=0, g_gbz=0;   // gyro bias

  bool r8(uint8_t addr, uint8_t reg, uint8_t& v) {
    Wire.beginTransmission(addr); Wire.write(reg);
    if (Wire.endTransmission(false) != 0) return false;
    if (Wire.requestFrom((int)addr, 1) != 1) return false;
    v = Wire.read(); return true;
  }
  void w8(uint8_t addr, uint8_t reg, uint8_t v) {
    Wire.beginTransmission(addr); Wire.write(reg); Wire.write(v); Wire.endTransmission();
  }
  bool rN(uint8_t addr, uint8_t reg, uint8_t* buf, int n) {
    Wire.beginTransmission(addr); Wire.write(reg);
    if (Wire.endTransmission(false) != 0) return false;
    if (Wire.requestFrom((int)addr, n) != n) return false;
    for (int i=0;i<n;i++) buf[i]=Wire.read();
    return true;
  }
  int16_t be16(const uint8_t* p) { return (int16_t)((p[0]<<8)|p[1]); }
  int16_t le16(const uint8_t* p) { return (int16_t)((p[1]<<8)|p[0]); }

  // ---- detection: (addr, id-reg, expected) tries ----
  struct Probe { ImuKind kind; uint8_t addr; uint8_t idReg; uint8_t idVal; const char* name; };
  const Probe PROBES[] = {
    { IMU_MPU6050,  0x68, 0x75, 0x68, "MPU-6050" },
    { IMU_MPU6050,  0x69, 0x75, 0x68, "MPU-6050" },
    { IMU_ICM42670, 0x68, 0x75, 0x67, "ICM-42670" },
    { IMU_ICM42670, 0x69, 0x75, 0x67, "ICM-42670" },
    { IMU_LSM6DSO,  0x6A, 0x0F, 0x6C, "LSM6DSO" },
    { IMU_LSM6DSO,  0x6B, 0x0F, 0x6C, "LSM6DSO" },
    { IMU_BNO055,   0x28, 0x00, 0xA0, "BNO055" },
    { IMU_BNO055,   0x29, 0x00, 0xA0, "BNO055" },
    { IMU_BNO08X,   0x4A, 0x00, 0x00, "BNO08x" },   // SHTP: address-ACK only (detect-only)
    { IMU_BNO08X,   0x4B, 0x00, 0x00, "BNO08x" },
  };
}

namespace imu {

ImuKind detect() {
  if (g_synth) return g_kind;
  Wire.begin(PIN_I2C_SDA, PIN_I2C_SCL, I2C_FREQ_HZ);
  for (int attempt = 0; attempt < 3; attempt++) {
    for (const auto& p : PROBES) {
      Wire.beginTransmission(p.addr);
      if (Wire.endTransmission() != 0) continue;        // no ACK at this addr
      if (p.kind == IMU_BNO08X) {                        // detect-only
        g_kind=p.kind; g_addr=p.addr; snprintf(g_label,sizeof g_label,"%s @0x%02X",p.name,p.addr);
        return g_kind;
      }
      uint8_t id;
      if (r8(p.addr, p.idReg, id) && id == p.idVal) {
        g_kind=p.kind; g_addr=p.addr; snprintf(g_label,sizeof g_label,"%s @0x%02X",p.name,p.addr);
        Serial.printf("[imu] detected %s\n", g_label);
        return g_kind;
      }
    }
    delay(30);
  }
  g_kind = IMU_NONE; strncpy(g_label,"none",sizeof g_label);
  Serial.println("[imu] none detected");
  return g_kind;
}

bool begin() {
  if (g_synth) { g_begun = true; return true; }
  switch (g_kind) {
    case IMU_MPU6050:
      w8(g_addr, 0x6B, 0x00);            // wake
      w8(g_addr, 0x1B, 0x00);            // gyro ±250 dps
      w8(g_addr, 0x1C, 0x00);            // accel ±2 g
      g_begun = true; break;
    case IMU_ICM42670:
      w8(g_addr, 0x1F, 0x0F);            // PWR_MGMT0: accel+gyro LN mode
      delay(20); g_begun = true; break;
    case IMU_LSM6DSO:
      w8(g_addr, 0x10, 0x40);            // CTRL1_XL: 104 Hz, ±2 g
      w8(g_addr, 0x11, 0x40);            // CTRL2_G:  104 Hz, ±250 dps
      g_begun = true; break;
    case IMU_BNO055:
      w8(g_addr, 0x3D, 0x0C);            // OPR_MODE = NDOF
      delay(20); g_begun = true; break;
    default:
      g_begun = false;                    // BNO08x / none -> fail-safe absent
  }
  if (!g_begun) { g_kind = IMU_NONE; strncpy(g_label,"none",sizeof g_label); }
  return g_begun;
}

bool present() { return g_begun && g_kind != IMU_NONE; }
ImuKind kind() { return g_kind; }
const char* label() { return g_label; }

static bool readRaw(ImuSample& s) {
  uint8_t b[12];
  switch (g_kind) {
    case IMU_MPU6050: {
      if (!rN(g_addr, 0x3B, b, 6)) return false;         // accel
      s.ax=be16(b)/16384.f; s.ay=be16(b+2)/16384.f; s.az=be16(b+4)/16384.f;
      if (!rN(g_addr, 0x43, b, 6)) return false;         // gyro
      s.gx=be16(b)/131.f; s.gy=be16(b+2)/131.f; s.gz=be16(b+4)/131.f;
      return true;
    }
    case IMU_LSM6DSO: {
      if (!rN(g_addr, 0x22, b, 12)) return false;        // gyro(6) then accel(6), LE
      s.gx=le16(b)*8.75f/1000.f; s.gy=le16(b+2)*8.75f/1000.f; s.gz=le16(b+4)*8.75f/1000.f;
      s.ax=le16(b+6)*0.061f/1000.f; s.ay=le16(b+8)*0.061f/1000.f; s.az=le16(b+10)*0.061f/1000.f;
      return true;
    }
    case IMU_ICM42670: {
      if (!rN(g_addr, 0x0B, b, 12)) return false;        // accel(6) then gyro(6), BE
      s.ax=be16(b)/16384.f; s.ay=be16(b+2)/16384.f; s.az=be16(b+4)/16384.f;
      s.gx=be16(b+6)/131.f; s.gy=be16(b+8)/131.f; s.gz=be16(b+10)/131.f;
      return true;
    }
    case IMU_BNO055: {
      if (!rN(g_addr, 0x08, b, 6)) return false;         // ACC (mg? 1 m/s^2 = 100 LSB)
      s.ax=be16(b)/100.f/9.81f; s.ay=be16(b+2)/100.f/9.81f; s.az=be16(b+4)/100.f/9.81f;
      if (!rN(g_addr, 0x14, b, 6)) return false;         // GYR (16 LSB/dps)
      s.gx=be16(b)/16.f; s.gy=be16(b+2)/16.f; s.gz=be16(b+4)/16.f;
      return true;
    }
    default: return false;
  }
}

// synthetic motion for UI/classifier demo without hardware: a scripted routine of distinct
// gestures separated by stillness so the segmenter/classifier produce a real captured sequence:
// spin R -> bounce -> tilt R -> lean fwd -> spin L, looping every 8 s.
bool read(ImuSample& s) {
  if (g_synth) {
    uint32_t t = millis() % 8000;
    s.ax = s.ay = s.gx = s.gy = s.gz = 0; s.az = 1.0f;
    if      (t < 1000)              s.gz = 130;                                  // spin R
    else if (t < 1500)             ;                                            // still
    else if (t < 2500)              s.az = 1.0f + 0.5f*sinf((t-1500)*0.0314f);  // bounce (~5Hz)
    else if (t < 3000)             ;                                            // still
    else if (t < 4000)              s.ay = 0.5f;                                // tilt R -> head
    else if (t < 4500)             ;                                            // still
    else if (t < 5500)              s.ax = 0.5f;                                // lean fwd -> step
    else if (t < 6000)             ;                                            // still
    else if (t < 7000)              s.gz = -130;                               // spin L
    // 7000..8000 still
    return true;
  }
  if (!g_begun) return false;
  if (!readRaw(s)) return false;
  s.gx -= g_gbx; s.gy -= g_gby; s.gz -= g_gbz;
  return true;
}

void calibrateGyro(int samples) {
  if (g_synth || !g_begun) { g_gbx=g_gby=g_gbz=0; return; }
  float sx=0,sy=0,sz=0; int got=0; ImuSample s;
  float bx=g_gbx,by=g_gby,bz=g_gbz; g_gbx=g_gby=g_gbz=0;  // read raw during cal
  for (int i=0;i<samples;i++){ if(readRaw(s)){ sx+=s.gx; sy+=s.gy; sz+=s.gz; got++; } delay(3); }
  if (got){ g_gbx=sx/got; g_gby=sy/got; g_gbz=sz/got; }
  else { g_gbx=bx; g_gby=by; g_gbz=bz; }
}

void forceSynthetic(bool on) {
  g_synth = on;
  if (on) { g_kind=IMU_MPU6050; g_begun=true; strncpy(g_label,"synthetic (demo)",sizeof g_label); }
  else    { g_kind=IMU_NONE; g_begun=false; strncpy(g_label,"none",sizeof g_label); }
}
bool isSynthetic() { return g_synth; }

} // namespace imu
