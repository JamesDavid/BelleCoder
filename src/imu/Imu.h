#pragma once
// imu/Imu.h — optional IMU capability + abstraction layer (SPEC §9, mirrors the PAL pattern).
// Auto-detects a supported 6/9-axis part on I2C at boot; the whole capture feature gates on a
// single runtime present() flag. Absent -> capture UI hidden entirely, no library init.
#include <stdint.h>

enum ImuKind : uint8_t { IMU_NONE, IMU_MPU6050, IMU_ICM42670, IMU_LSM6DSO, IMU_BNO08X, IMU_BNO055 };

struct ImuSample { float ax, ay, az;   // accel, g
                   float gx, gy, gz; }; // gyro, deg/s

namespace imu {
  ImuKind     detect();        // probe known addrs, verify WHO_AM_I, retry; sets kind
  bool        begin();         // init the detected part; false -> treat as absent (fail-safe)
  bool        present();       // kind != IMU_NONE and begun
  ImuKind     kind();
  const char* label();         // e.g. "MPU-6050 @0x68"
  bool        read(ImuSample& s);
  void        calibrateGyro(int samples = 200);   // bias-cal held still (capture start)

  // debug/demo: force a synthetic IMU so the capture UI can be shown without hardware
  void        forceSynthetic(bool on);
  bool        isSynthetic();
}
