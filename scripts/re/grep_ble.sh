#!/usr/bin/env bash
# SPEC §11.2 — pull the PAL-relevant facts out of the decompiled C#.
set -euo pipefail
SRC="${1:-re/decompiled/Assembly-CSharp}"

echo "===== characteristic + service UUIDs ====="
grep -rniE '[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}' "$SRC" \
  | grep -iE 'characteristic|service|uuid' | sort -u

echo; echo "===== adv-name filter ====="
grep -rniE 'toyNameFilter\s*=' "$SRC"

echo; echo "===== write type (withResponse flag) ====="
grep -rniE 'WriteCharacteristicWithIdentifiers|withResponse' "$SRC" | grep -iE 'false|true|bool withResponse'

echo; echo "===== opcode table ====="
sed -n '/enum AppToToyOpCodes/,/}/p' "$SRC/AppToToyOpCodes.cs"

echo; echo "===== packet builders (opcode -> bytes) ====="
grep -rnE 'new byte\[\]?\s*\{|array\[0\] = [0-9]+' "$SRC/BLEToyAPI.cs"
