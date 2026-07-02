#!/usr/bin/env bash
# SPEC §11.1 — obtain the Dance Code APK and verify it before touching it.
# Static analysis only; we never INSTALL this APK.
set -euo pipefail
OUT="${1:-re/dancecode.apk}"
PKG="com.hasbro.dancecode"
EXPECT_SIZE=92788069
EXPECT_SHA256="bc79a90465259c2af2f03f331298dce2f080971e522066b596689e118b5a4794"
UA="Mozilla/5.0 (Windows NT 10.0; Win64; x64)"

mkdir -p "$(dirname "$OUT")"
echo ">> downloading $PKG (APKPure) -> $OUT"
curl -sL -A "$UA" -o "$OUT" "https://d.apkpure.com/b/APK/$PKG?version=latest"

size=$(wc -c < "$OUT")
sha=$(python -c "import hashlib,sys;print(hashlib.sha256(open(sys.argv[1],'rb').read()).hexdigest())" "$OUT")
echo ">> size   $size (expect $EXPECT_SIZE)"
echo ">> sha256 $sha"
[ "$size" = "$EXPECT_SIZE" ] || echo "!! size mismatch — mirror may have changed the build; re-verify before trusting"
[ "$sha"  = "$EXPECT_SHA256" ] || echo "!! sha256 mismatch — NOT the analyzed build; re-verify signing cert"

echo ">> signing cert (expect a HASBRO-named key, not CERT.RSA):"
unzip -l "$OUT" | grep -iE 'META-INF/.*\.(RSA|DSA|EC)$' || true
echo ">> done. Prefer a device-pulled copy over any mirror when possible (SPEC §11.1)."
