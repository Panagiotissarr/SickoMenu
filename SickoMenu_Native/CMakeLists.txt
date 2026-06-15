cmake_minimum_required(VERSION 3.14)
project(SickoMenu_Native VERSION 5.0.0 LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Sources
file(GLOB_RECURSE SOURCES src/*.cpp src/*.c src/*.mm src/*.m)

# --- Windows (BepInEx/DLL) ---
if(WIN32)
    add_library(SickoMenu SHARED ${SOURCES})
    target_compile_definitions(SickoMenu PRIVATE PLATFORM_WINDOWS)
    target_link_libraries(SickoMenu PRIVATE)
endif()

# --- iOS (dylib) ---
if(APPLE AND NOT ANDROID)
    # iOS or macOS
    if(IOS OR CMAKE_SYSTEM_NAME STREQUAL "iOS")
        add_library(SickoMenu_iOS SHARED ${SOURCES} ios/KeyboardParser.mm)
        target_compile_definitions(SickoMenu_iOS PRIVATE PLATFORM_IOS)
        target_link_libraries(SickoMenu_iOS PRIVATE "-framework UIKit" "-framework Foundation")
        set_target_properties(SickoMenu_iOS PROPERTIES
            SUFFIX ".dylib"
            MACOSX_RPATH TRUE
            IOS_INSTALL_COMBINED YES
        )
    else()
        # macOS (standalone dylib, not BepInEx)
        add_library(SickoMenu_macOS SHARED ${SOURCES})
        target_compile_definitions(SickoMenu_macOS PRIVATE PLATFORM_MACOS)
        target_link_libraries(SickoMenu_macOS PRIVATE "-framework Foundation" "-framework ApplicationServices")
    endif()
endif()

# --- Android (.so) ---
if(ANDROID)
    add_library(SickoMenu_Android SHARED ${SOURCES})
    target_compile_definitions(SickoMenu_Android PRIVATE PLATFORM_ANDROID)
    target_link_libraries(SickoMenu_Android PRIVATE log)
endif()
