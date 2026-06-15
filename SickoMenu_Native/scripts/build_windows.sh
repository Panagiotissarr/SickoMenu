#!/bin/bash
# Build SickoMenu for Windows (DLL - can be used standalone or with BepInEx)
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/.."

OUTPUT_DIR="$PROJECT_DIR/build/windows"
mkdir -p "$OUTPUT_DIR"

# Cross-compile from macOS using MinGW
if command -v x86_64-w64-mingw32-g++ &> /dev/null; then
    echo "Cross-compiling for Windows using MinGW..."
    x86_64-w64-mingw32-g++ -std=c++17 -O2 -shared \
        -I"$PROJECT_DIR/src" \
        "$PROJECT_DIR/src/il2cpp/Il2CppApi.cpp" \
        "$PROJECT_DIR/src/platform/Windows.cpp" \
        -o "$OUTPUT_DIR/SickoMenu.dll" \
        -static-libgcc -static-libstdc++ \
        -Wl,-eDllMain
    echo "  -> $OUTPUT_DIR/SickoMenu.dll"
else
    echo "MinGW not found. Building for Windows requires either:"
    echo "  1. Visual Studio on Windows"
    echo "  2. MinGW cross-compiler (x86_64-w64-mingw32-g++)"
    echo ""
    echo "For now, use the C# BepInEx project from SickoMenu.csproj"
    echo "This native DLL is optional for Windows (BepInEx handles it)."
fi
