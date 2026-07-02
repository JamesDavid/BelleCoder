#include "Storage.h"
#include "../catalog/Catalog.h"
#include <Arduino.h>
#include <LittleFS.h>
#include <ArduinoJson.h>

namespace {
  const char* SEQ_DIR = "/seq";
  const char* PRE_DIR = "/presets";

  void safeName(const char* in, char* out, int n) {
    int j = 0;
    for (int i = 0; in[i] && j < n-1; i++) {
      char c = in[i];
      if (c==' ') c='_';
      if (isalnum(c) || c=='_' || c=='-') out[j++] = c;
    }
    out[j] = 0;
    if (!j) strncpy(out, "dance", n);
  }

  // ---- {arm,l,r} raw choreography -> editable moves ---------------------------
  MoveId classify(int arm, int l, int r) {
    if (l==0 && r==0) {
      if (arm >= 20)      return ARMS_UP;       // ArmsUp / ArmsForward
      if (arm == 0)       return ARMS_DOWN;
      if (arm==8 || arm==12) return ARM_L_UP;
      return ARMS_UP;
    }
    if (l == r)  return (l > 0) ? STEP_FORWARD : STEP_BACK;
    if (l == -r) return (l < 0) ? TWIRL_LEFT   : TWIRL_RIGHT;
    // pivot / uneven -> spin toward the faster wheel
    return (abs(l) > abs(r)) ? TWIRL_RIGHT : TWIRL_LEFT;
  }
}

namespace storage {

bool begin() {
  if (!LittleFS.begin(true)) { Serial.println("[fs] mount failed"); return false; }
  if (!LittleFS.exists(SEQ_DIR)) LittleFS.mkdir(SEQ_DIR);
  Serial.printf("[fs] mounted  used=%u/%u\n", (unsigned)LittleFS.usedBytes(), (unsigned)LittleFS.totalBytes());
  return true;
}

bool save(const Sequence& seq) {
  char nm[SEQ_NAME_LEN]; safeName(seq.name, nm, sizeof nm);
  char path[48]; snprintf(path, sizeof path, "%s/%s.json", SEQ_DIR, nm);
  File f = LittleFS.open(path, "w");
  if (!f) return false;
  JsonDocument doc;
  doc["name"] = seq.name;
  JsonArray arr = doc["steps"].to<JsonArray>();
  for (int i = 0; i < seq.count; i++) {
    JsonObject o = arr.add<JsonObject>();
    o["m"] = (int)seq.steps[i].move;
    o["p"] = seq.steps[i].p1;
    if (seq.steps[i].repeatGrp) o["g"] = seq.steps[i].repeatGrp;
  }
  serializeJson(doc, f);
  f.close();
  Serial.printf("[fs] saved %s (%d steps)\n", path, seq.count);
  return true;
}

bool load(const char* name, Sequence& out) {
  char path[48]; snprintf(path, sizeof path, "%s/%s.json", SEQ_DIR, name);
  File f = LittleFS.open(path, "r");
  if (!f) return false;
  JsonDocument doc;
  if (deserializeJson(doc, f)) { f.close(); return false; }
  f.close();
  out.clear();
  out.setName(doc["name"] | name);
  for (JsonObject o : doc["steps"].as<JsonArray>()) {
    if (out.full()) break;
    Step s{ (MoveId)(int)o["m"], (int16_t)(o["p"] | 0), (uint8_t)(o["g"] | 0) };
    out.append(s);
  }
  return true;
}

bool remove(const char* name) {
  char path[48]; snprintf(path, sizeof path, "%s/%s.json", SEQ_DIR, name);
  return LittleFS.remove(path);
}

static int listDir(const char* dir, const char* suffix, char names[][SEQ_NAME_LEN], int maxN) {
  File d = LittleFS.open(dir);
  if (!d || !d.isDirectory()) return 0;
  int n = 0; File e;
  int suflen = strlen(suffix);
  while ((e = d.openNextFile()) && n < maxN) {
    const char* fn = e.name();                 // basename
    int len = strlen(fn);
    if (len > suflen && !strcmp(fn + len - suflen, suffix)) {
      int base = len - suflen;
      if (base > SEQ_NAME_LEN-1) base = SEQ_NAME_LEN-1;
      strncpy(names[n], fn, base); names[n][base] = 0;
      n++;
    }
    e.close();
  }
  d.close();
  return n;
}

int listSaved(char names[][SEQ_NAME_LEN], int maxN)   { return listDir(SEQ_DIR, ".json", names, maxN); }
int listPresets(char names[][SEQ_NAME_LEN], int maxN) { return listDir(PRE_DIR, "_dance.json", names, maxN); }

bool loadPreset(const char* name, Sequence& out) {
  char path[56]; snprintf(path, sizeof path, "%s/%s_dance.json", PRE_DIR, name);
  File f = LittleFS.open(path, "r");
  if (!f) return false;
  JsonDocument doc;
  if (deserializeJson(doc, f)) { f.close(); return false; }
  f.close();

  out.clear();
  { char nm[SEQ_NAME_LEN]; snprintf(nm, sizeof nm, "%s", name); nm[0]=toupper(nm[0]); out.setName(nm); }

  // run-length compress consecutive identical raw tokens into one move (amount = run length)
  MoveId prev = MOVE_COUNT; int run = 0;
  auto flush = [&](){
    if (run <= 0) return;
    const MoveInfo& mi = moveInfo(prev);
    int16_t p = mi.hasParam ? (int16_t)min(run, (int)mi.pMax) : 0;
    if (!out.full()) { Step s{ prev, p, 0 }; out.append(s); }
    run = 0;
  };
  for (int i = 0; ; i++) {
    char key[10]; snprintf(key, sizeof key, "step%d", i);
    if (!doc[key].is<JsonObject>()) break;
    int arm = doc[key]["arm"] | 0;
    int l   = atoi((const char*)(doc[key]["l"] | "0"));
    int r   = atoi((const char*)(doc[key]["r"] | "0"));
    MoveId m = classify(arm, l, r);
    if (m == prev) run++;
    else { flush(); prev = m; run = 1; }
    if (out.full()) break;
  }
  flush();
  return !out.empty();
}

} // namespace storage
