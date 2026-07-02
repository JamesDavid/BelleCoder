#include "Capture.h"
#include <Arduino.h>
#include <math.h>

void Capture::reset() {
  _inGesture = false; _sumYaw = 0; _sumRoll = 0; _sumPitch = 0; _rpN = 0;
  _bounceCount = 0; _bounceUp = false; _lastAz = 1.0f;
}

void Capture::start(int sensitivity, bool bounceToArms) {
  _bounceToArms = bounceToArms;
  // higher sensitivity -> lower thresholds (kids vary a lot; SPEC §9)
  float s = constrain(sensitivity, 1, 10);
  _gyroTh   = 55.0f - s * 4.0f;    // deg/s to count as "spinning"  (~15..51)
  _bounceTh = 0.55f - s * 0.035f;  // g of vertical wobble to count a bounce
  _tiltTh   = 0.45f - s * 0.02f;   // g of sustained tilt for head/lean
  _out.clear(); _out.setName("Danced");
  reset();
  imu::calibrateGyro(180);         // ~0.6s, held still
  _phase = CapPhase::Recording;
  _lastMs = millis(); _gestStart = _lastMs; _stillSince = _lastMs; _lastCommitMs = _lastMs;
  _hint = LiveHint::None;
}

void Capture::feed(const ImuSample& s, uint32_t ms) {
  if (_phase != CapPhase::Recording) return;
  float dt = (ms - _lastMs) / 1000.0f; _lastMs = ms;
  if (dt <= 0 || dt > 0.2f) dt = 0.02f;

  float az = s.az - 1.0f;                       // dynamic vertical (gravity removed)
  float gyroMag = fabsf(s.gz);
  float motion = gyroMag + fabsf(s.gx) + fabsf(s.gy) + fabsf(az) * 200.0f;

  bool active = motion > (_gyroTh);             // same scale as gyro threshold
  // live hint (what the child sees happening)
  if (gyroMag > _gyroTh)                 _hint = LiveHint::Spin;
  else if (az >  _bounceTh)              _hint = LiveHint::BounceUp;
  else if (az < -_bounceTh)              _hint = LiveHint::BounceDown;
  else if (s.ay >  _tiltTh)              _hint = LiveHint::TiltR;
  else if (s.ay < -_tiltTh)              _hint = LiveHint::TiltL;
  else if (s.ax >  _tiltTh)              _hint = LiveHint::LeanF;
  else if (s.ax < -_tiltTh)              _hint = LiveHint::LeanB;
  else                                   _hint = LiveHint::None;

  if (active) {
    if (!_inGesture) { _inGesture = true; _gestStart = ms; reset(); _inGesture = true; }
    _sumYaw   += s.gz * dt;
    _sumRoll  += s.ay; _sumPitch += s.ax; _rpN++;
    // bounce peak detection (az crossing the threshold up then down)
    if (az >  _bounceTh && !_bounceUp) { _bounceUp = true; }
    if (az < -_bounceTh && _bounceUp)  { _bounceUp = false; _bounceCount++; }
    _stillSince = ms;
  } else {
    // stillness delimits gestures (~350 ms)
    if (_inGesture && (ms - _stillSince) > 350) closeGesture(ms);
  }
}

void Capture::closeGesture(uint32_t ms) {
  uint32_t dur = ms - _gestStart;
  float roll  = _rpN ? _sumRoll / _rpN : 0;
  float pitch = _rpN ? _sumPitch / _rpN : 0;
  MoveId m; int16_t p = 1;
  bool emit = true;

  if (fabsf(_sumYaw) > 45.0f) {
    m = (_sumYaw < 0) ? TWIRL_LEFT : TWIRL_RIGHT;
    p = constrain((int)lroundf(fabsf(_sumYaw) / 180.0f), 1, 4);
  } else if (_bounceCount >= 1) {
    m = _bounceToArms ? ARMS_UP : STEP_FORWARD;
    p = _bounceToArms ? 0 : constrain(_bounceCount, 1, 4);
  } else if (fabsf(roll) > _tiltTh) {
    m = (roll < 0) ? HEAD_LEFT : HEAD_RIGHT;
  } else if (fabsf(pitch) > _tiltTh) {
    m = (pitch < 0) ? STEP_BACK : STEP_FORWARD;
    p = constrain((int)(dur / 500), 1, 4);
  } else {
    emit = false;   // too ambiguous -> drop
  }

  if (emit && !_out.full()) {
    // a longer still gap before this gesture becomes a WAIT (kid-natural pauses)
    uint32_t gap = _gestStart - _lastCommitMs;
    if (_out.count > 0 && gap > 700 && !_out.full()) {
      Step w{ WAIT, (int16_t)constrain((int)gap, 100, 3000), 0 }; _out.append(w);
    }
    Step st{ m, p, 0 };
    _out.append(st);
    _flash = true; _flashMove = m; _flashP = p;
    _lastCommitMs = ms;
  }
  _inGesture = false; reset();
}

void Capture::stop() {
  if (_phase == CapPhase::Recording && _inGesture) closeGesture(millis());
  _phase = CapPhase::Done;
  _hint = LiveHint::None;
}

bool Capture::takeCommitFlash(MoveId& m, int16_t& p) {
  if (!_flash) return false;
  _flash = false; m = _flashMove; p = _flashP; return true;
}
