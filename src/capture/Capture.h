#pragma once
// capture/Capture.h — "Dance to Code" motion pipeline (SPEC §9). record -> segment (by
// stillness) -> classify (heuristic) -> emit the SAME Sequence model as the tap editor.
#include <stdint.h>
#include "../imu/Imu.h"
#include "../model/Sequence.h"

enum class CapPhase : uint8_t { Idle, Calibrating, Recording, Done };
enum class LiveHint : uint8_t { None, Spin, BounceUp, BounceDown, TiltL, TiltR, LeanF, LeanB };

class Capture {
public:
  void start(int sensitivity, bool bounceToArms);  // calibrate gyro, begin recording
  void stop();                                       // close final gesture -> out ready
  void feed(const ImuSample& s, uint32_t ms);        // one IMU sample

  CapPhase phase()    const { return _phase; }
  LiveHint hint()     const { return _hint; }
  int      committed()const { return _out.count; }
  bool     takeCommitFlash(MoveId& m, int16_t& p);   // one-shot: a gesture was just committed
  const Sequence& result() const { return _out; }

private:
  void closeGesture(uint32_t ms);
  void reset();

  CapPhase _phase = CapPhase::Idle;
  LiveHint _hint  = LiveHint::None;
  Sequence _out;

  // thresholds (scaled by sensitivity)
  float _gyroTh, _bounceTh, _tiltTh;
  bool  _bounceToArms = true;

  // per-gesture accumulators
  bool     _inGesture = false;
  uint32_t _lastMs = 0, _gestStart = 0, _stillSince = 0, _lastCommitMs = 0;
  float    _sumYaw = 0;         // integrated gz (deg)
  float    _sumRoll = 0, _sumPitch = 0; int _rpN = 0;
  int      _bounceCount = 0; bool _bounceUp = false; float _lastAz = 1.0f;

  // commit flash mailbox
  bool     _flash = false; MoveId _flashMove = STEP_FORWARD; int16_t _flashP = 0;
};
