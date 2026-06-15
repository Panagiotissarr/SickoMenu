#pragma once

#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>

@interface SickoKeyboardParser : NSObject

+ (instancetype)sharedInstance;

// Start listening for keyboard events from a UIKeyCommand or UITextField
- (void)startListening;

// Stop listening
- (void)stopListening;

// Check if a key is currently held down
- (BOOL)isKeyPressed:(UIKeyboardHIDUsage)keyCode;

// Get mouse position (scaled to screen coordinates)
- (CGPoint)mouseLocation;

// Is mouse button pressed
- (BOOL)isLeftMouseDown;
- (BOOL)isRightMouseDown;

// Call this in your game loop to inject touch events into Unity
- (void)processInputFrame;

@end

// Virtual key codes commonly used
typedef NS_ENUM(NSUInteger, SickoKey) {
    SickoKey_W = 0x1D,  // UIKeyboardHIDUsageKeyboardW
    SickoKey_A = 0x04,  // UIKeyboardHIDUsageKeyboardA
    SickoKey_S = 0x16,  // UIKeyboardHIDUsageKeyboardS
    SickoKey_D = 0x07,  // UIKeyboardHIDUsageKeyboardD
    SickoKey_Space = 0x2C,
    SickoKey_Escape = 0x29,
    SickoKey_Enter = 0x28,
    SickoKey_Tab = 0x2B,
    SickoKey_Q = 0x14,
    SickoKey_E = 0x08,
    SickoKey_R = 0x15,
    SickoKey_F = 0x09,
    SickoKey_Shift = 0xE1,
    SickoKey_Ctrl = 0xE0,
    SickoKey_1 = 0x1E,
    SickoKey_2 = 0x1F,
    SickoKey_3 = 0x20,
    SickoKey_4 = 0x21,
};
