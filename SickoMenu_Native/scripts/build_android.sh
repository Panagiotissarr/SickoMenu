#!/bin/bash
# Build SickoMenu for Android (.so for APK injection)
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/.."

# Configuration
NDK_DIR="${ANDROID_NDK_HOME:-$HOME/Android/Sdk/ndk/25.2.9519653}"
API_LEVEL="21"
OUTPUT_DIR="$PROJECT_DIR/build/android"
ABIS=("arm64-v8a" "armeabi-v7a")

if [ ! -d "$NDK_DIR" ]; then
    echo "Error: Android NDK not found. Set ANDROID_NDK_HOME"
    exit 1
fi

mkdir -p "$OUTPUT_DIR"

for ABI in "${ABIS[@]}"; do
    case $ABI in
        arm64-v8a)
            TOOLCHAIN="$NDK_DIR/toolchains/llvm/prebuilt/darwin-x86_64"
            TARGET="aarch64-linux-android"
            ARCH_FLAGS="-march=armv8-a"
            ;;
        armeabi-v7a)
            TOOLCHAIN="$NDK_DIR/toolchains/llvm/prebuilt/darwin-x86_64"
            TARGET="armv7a-linux-androideabi"
            ARCH_FLAGS="-march=armv7-a -mfloat-abi=softfp"
            ;;
    esac
    
    CLANG="$TOOLCHAIN/bin/${TARGET}${API_LEVEL}-clang++"
    
    echo "Building for $ABI ($TARGET)..."
    
    $CLANG -std=c++17 -O2 -fPIC \
        $ARCH_FLAGS \
        -I"$PROJECT_DIR/src" \
        "$PROJECT_DIR/src/il2cpp/Il2CppApi.cpp" \
        "$PROJECT_DIR/src/platform/Android.cpp" \
        -shared \
        -o "$OUTPUT_DIR/libSickoMenu_${ABI}.so" \
        -llog \
        -Wl,-soname=libSickoMenu.so
    
    echo "  -> $OUTPUT_DIR/libSickoMenu_${ABI}.so"
done

echo "Android build complete"
ls -la "$OUTPUT_DIR/"*.so 2>/dev/null || true
