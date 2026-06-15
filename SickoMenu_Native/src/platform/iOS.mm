#include "../il2cpp/Il2CppApi.h"
#include "../../ios/KeyboardParser.h"
#import <Foundation/Foundation.h>

extern "C" {
// Entry point called from the injected dylib
__attribute__((constructor))
void SickoInit() {
    NSLog(@"[Sicko] Initializing SickoMenu for iOS...");
    
    if (!sicko::il2cpp::Initialize()) {
        NSLog(@"[Sicko] Failed to initialize Il2Cpp API");
        return;
    }
    
    // Start keyboard/mouse parser for iPadOS on Mac
    dispatch_async(dispatch_get_main_queue(), ^{
        [SickoKeyboardParser.sharedInstance startListening];
        NSLog(@"[Sicko] Keyboard parser started");
        // TODO: Initialize game loop timer
    });
}

__attribute__((destructor))
void SickoDeinit() {
    [SickoKeyboardParser.sharedInstance stopListening];
    NSLog(@"[Sicko] SickoMenu unloaded");
}
}
