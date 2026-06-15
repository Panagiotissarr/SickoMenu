#!/bin/bash
# Build SickoMenu for iOS (dylib for IPA injection)
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/.."

# Configuration
ARCHS="arm64"
MIN_IOS_VERSION="14.0"
OUTPUT_DIR="$PROJECT_DIR/build/ios"

mkdir -p "$OUTPUT_DIR"

for ARCH in $ARCHS; do
    echo "Building for $ARCH..."
    
    clang++ -std=c++17 -O2 -arch $ARCH \
        -isysroot $(xcrun --sdk iphoneos --show-sdk-path) \
        -miphoneos-version-min=$MIN_IOS_VERSION \
        -fobjc-arc \
        -dynamiclib \
        -I"$PROJECT_DIR/src" \
        -I"$PROJECT_DIR/ios" \
        "$PROJECT_DIR/src/il2cpp/Il2CppApi.cpp" \
        "$PROJECT_DIR/src/platform/iOS.mm" \
        "$PROJECT_DIR/ios/KeyboardParser.mm" \
        -o "$OUTPUT_DIR/SickoMenu_${ARCH}.dylib" \
        -framework UIKit \
        -framework Foundation \
        -Wl,-dead_strip
    
    echo "  -> $OUTPUT_DIR/SickoMenu_${ARCH}.dylib"
done

# Create universal binary
if [ ${#ARCHS[@]} -gt 1 ]; then
    lipo -create "$OUTPUT_DIR"/SickoMenu_*.dylib -output "$OUTPUT_DIR/SickoMenu.dylib"
    echo "Created universal: $OUTPUT_DIR/SickoMenu.dylib"
else
    mv "$OUTPUT_DIR/SickoMenu_${ARCHS}.dylib" "$OUTPUT_DIR/SickoMenu.dylib"
fi

echo "iOS build complete: $OUTPUT_DIR/SickoMenu.dylib"
