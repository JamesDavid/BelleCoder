#!/usr/bin/env bash
# SPEC §11.2 — decompile the Unity-Mono Assembly-CSharp.dll to C#.
# This app is Unity+Mono (libmono.so + assets/bin/Data/Managed/*.dll), so the game
# logic is standard .NET CIL — dnSpy/ILSpy gives clean C#. (NOT IL2CPP; jadx is only
# useful for the thin classes.dex Java glue.)
set -euo pipefail
APK="${1:-re/dancecode.apk}"
OUT="${2:-re/decompiled}"
DNSPY="${DNSPY:-re/tools/dnSpy/dnSpy.Console.exe}"   # netframework build; runs on Windows .NET 4.x

# 1. fetch dnSpy.Console.exe once (self-contained, no SDK needed)
if [ ! -f "$DNSPY" ]; then
  echo ">> fetching dnSpyEx console decompiler"
  mkdir -p "$(dirname "$DNSPY")"
  url=$(curl -s "https://api.github.com/repos/dnSpyEx/dnSpy/releases/latest" \
        | python -c "import sys,json;print(next(a['browser_download_url'] for a in json.load(sys.stdin)['assets'] if a['name']=='dnSpy-netframework.zip'))")
  curl -sL -o /tmp/dnspy.zip "$url"
  unzip -o -q /tmp/dnspy.zip -d "$(dirname "$DNSPY")"
fi

# 2. extract the managed assemblies from the APK
echo ">> extracting Managed/*.dll"
unzip -o -q "$APK" "assets/bin/Data/Managed/*.dll" -d re/apk_x

# 3. decompile the two app assemblies
echo ">> decompiling -> $OUT (~30-60s)"
"$DNSPY" --no-tokens -o "$OUT" \
  re/apk_x/assets/bin/Data/Managed/Assembly-CSharp.dll \
  re/apk_x/assets/bin/Data/Managed/Assembly-CSharp-firstpass.dll
echo ">> done: $(find "$OUT" -name '*.cs' | wc -l) C# files. Start at BLEToyAPI.cs / AppToToyOpCodes.cs"
