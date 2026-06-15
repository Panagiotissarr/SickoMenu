#import "KeyboardParser.h"
#include <mach/mach_time.h>

// Unity-iOS can expose its Input API via C-good-functions.
// We bridge by calling UnitySendMessage or directly manipulating Unity input.
// For the keyboard-to-touch translation, we synthesise touch events.

// Forward declarations for Unity's iOS input bridge (from Unity-iOS)
extern "C" {
    void UnitySendMessage(const char* obj, const char* method, const char* msg);
    void UnitySetKeyboardState(unsigned short keyCode, BOOL state);
}

@interface SickoKeyboardParser () {
    NSMutableSet<NSNumber*>* _pressedKeys;
    CGPoint _mousePos;
    BOOL _leftDown;
    BOOL _rightDown;
    BOOL _listening;
    UITextField* _hiddenField;
    CADisplayLink* _displayLink;
}

@property (nonatomic, strong) UIWindow* keyWindow;

@end

@implementation SickoKeyboardParser

+ (instancetype)sharedInstance {
    static SickoKeyboardParser* instance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        instance = [[self alloc] init];
    });
    return instance;
}

- (instancetype)init {
    self = [super init];
    if (self) {
        _pressedKeys = [NSMutableSet new];
        _mousePos = CGPointZero;
        _leftDown = NO;
        _rightDown = NO;
        _listening = NO;
        _keyWindow = nil;
        _hiddenField = nil;
    }
    return self;
}

- (void)startListening {
    if (_listening) return;
    _listening = YES;

    dispatch_async(dispatch_get_main_queue(), ^{
        // Get the key window
        if (@available(iOS 13.0, *)) {
            NSSet<UIScene*>* scenes = [UIApplication sharedApplication].connectedScenes;
            for (UIScene* scene in scenes) {
                if ([scene isKindOfClass:[UIWindowScene class]]) {
                    UIWindowScene* ws = (UIWindowScene*)scene;
                    // Prefer the first key window
                    for (UIWindow* w in ws.windows) {
                        if (w.isKeyWindow) {
                            self->_keyWindow = w;
                            break;
                        }
                    }
                    if (!self->_keyWindow)
                        self->_keyWindow = ws.windows.firstObject;
                    break;
                }
            }
        } else {
            self->_keyWindow = [UIApplication sharedApplication].keyWindow;
        }

        // iPadOS on Mac requires UIApplicationSupportsIndirectInputEvents in Info.plist.
        // We also create a hidden UITextField to capture physical keyboard input.
        if (!self->_hiddenField) {
            self->_hiddenField = [[UITextField alloc] initWithFrame:CGRectZero];
            self->_hiddenField.hidden = YES;
            [self->_keyWindow addSubview:self->_hiddenField];
        }

        // Become first responder to receive keyboard events
        [self->_hiddenField becomeFirstResponder];

        // Register for keyboard notifications
        [[NSNotificationCenter defaultCenter] addObserver:self
                                                 selector:@selector(keyboardWillShow:)
                                                     name:UIKeyboardWillShowNotification
                                                   object:nil];
        
        // Register for UIApplication keyboard notifications (iPadOS)
        [[NSNotificationCenter defaultCenter] addObserver:self
                                                 selector:@selector(keyboardDidChange:)
                                                     name:UITextFieldTextDidChangeNotification
                                                   object:self->_hiddenField];

        // Add mouse/trackpad event monitoring (iPadOS 13.4+)
        if (@available(iOS 13.4, *)) {
            // UIHoverGestureRecognizer for mouse position
            UIPanGestureRecognizer* pan = [[UIPanGestureRecognizer alloc] initWithTarget:self
                                                                                  action:@selector(handlePan:)];
            pan.allowedTouchTypes = @[ @(UITouchTypeIndirectPointer) ];
            [self->_keyWindow addGestureRecognizer:pan];

            // Click gesture for mouse buttons
            UITapGestureRecognizer* click = [[UITapGestureRecognizer alloc] initWithTarget:self
                                                                                    action:@selector(handleClick:)];
            click.allowedTouchTypes = @[ @(UITouchTypeIndirectPointer) ];
            [self->_keyWindow addGestureRecognizer:click];

            // Right click (secondary click)
            UITapGestureRecognizer* rightClick = [[UITapGestureRecognizer alloc] initWithTarget:self
                                                                                         action:@selector(handleRightClick:)];
            rightClick.allowedTouchTypes = @[ @(UITouchTypeIndirectPointer) ];
            rightClick.buttonMask = 2; // secondary button
            [self->_keyWindow addGestureRecognizer:rightClick];
        }
    });
}

- (void)stopListening {
    _listening = NO;
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    if (_hiddenField) {
        [_hiddenField removeFromSuperview];
        _hiddenField = nil;
    }
    if (_displayLink) {
        [_displayLink invalidate];
        _displayLink = nil;
    }
}

- (BOOL)isKeyPressed:(UIKeyboardHIDUsage)keyCode {
    return [_pressedKeys containsObject:@(keyCode)];
}

- (CGPoint)mouseLocation {
    return _mousePos;
}

- (BOOL)isLeftMouseDown {
    return _leftDown;
}

- (BOOL)isRightMouseDown {
    return _rightDown;
}

#pragma mark - Keyboard notifications

- (void)keyboardWillShow:(NSNotification*)notification {
    // iPadOS on Mac shows a software keyboard indicator; we hide it
    if (_hiddenField) {
        dispatch_async(dispatch_get_main_queue(), ^{
            [self->_hiddenField resignFirstResponder];
            [self->_hiddenField becomeFirstResponder];
        });
    }
}

- (void)keyboardDidChange:(NSNotification*)notification {
    // Capture the typed text and map to key presses
    NSString* text = _hiddenField.text;
    if (text.length > 0) {
        unichar c = [text characterAtIndex:text.length - 1];
        [self mapCharToKey:c pressed:YES];
        // Schedule key release (simulates a tap rather than hold)
        dispatch_after(dispatch_time(DISPATCH_TIME_NOW, 0.05 * NSEC_PER_SEC),
                       dispatch_get_main_queue(), ^{
            [self mapCharToKey:c pressed:NO];
        });
    }
    _hiddenField.text = @"";
}

- (void)mapCharToKey:(unichar)c pressed:(BOOL)pressed {
    // Map characters to Unity key codes
    unsigned short unityKey = 0;
    switch (c) {
        case 'w': case 'W': unityKey = 0x1D; break; // W
        case 'a': case 'A': unityKey = 0x04; break; // A
        case 's': case 'S': unityKey = 0x16; break; // S
        case 'd': case 'D': unityKey = 0x07; break; // D
        case ' ': unityKey = 0x2C; break; // Space
        case 0x1B: unityKey = 0x29; break; // Escape
        case '\n': unityKey = 0x28; break; // Enter
        case '\t': unityKey = 0x2B; break; // Tab
        case 'q': case 'Q': unityKey = 0x14; break;
        case 'e': case 'E': unityKey = 0x08; break;
        case 'r': case 'R': unityKey = 0x15; break;
        case 'f': case 'F': unityKey = 0x09; break;
        default: break;
    }
    if (unityKey) {
        if (pressed)
            [_pressedKeys addObject:@(unityKey)];
        else
            [_pressedKeys removeObject:@(unityKey)];
        UnitySetKeyboardState(unityKey, pressed);
    }
}

#pragma mark - Mouse/trackpad input (iPadOS 13.4+)

- (void)handlePan:(UIPanGestureRecognizer*)gesture {
    CGPoint loc = [gesture locationInView:_keyWindow];
    CGRect bounds = _keyWindow.bounds;
    // Normalize to Unity screen coordinates
    _mousePos = CGPointMake(loc.x / bounds.size.width, 1.0 - loc.y / bounds.size.height);
}

- (void)handleClick:(UITapGestureRecognizer*)gesture {
    _leftDown = (gesture.state == UIGestureRecognizerStateRecognized ||
                 gesture.state == UIGestureRecognizerStateBegan);
    if (gesture.state == UIGestureRecognizerStateRecognized) {
        // Single click - inject touch
        [self injectTouchAtPoint:[gesture locationInView:_keyWindow]];
    }
}

- (void)handleRightClick:(UITapGestureRecognizer*)gesture {
    _rightDown = (gesture.state == UIGestureRecognizerStateRecognized ||
                  gesture.state == UIGestureRecognizerStateBegan);
}

#pragma mark - Touch injection

- (void)injectTouchAtPoint:(CGPoint)point {
    // Use Unity's native touch injection API if available
    // This sends a touch event to Unity's input system
    extern void UnitySendTouchEvent(int fingerId, float x, float y, int phase);
    
    CGRect bounds = _keyWindow.bounds;
    float nx = point.x / bounds.size.width;
    float ny = 1.0 - point.y / bounds.size.height;
    
    UnitySendTouchEvent(0, nx, ny, 0); // Began
    UnitySendTouchEvent(0, nx, ny, 2); // Ended
}

#pragma mark - Input frame processing

- (void)processInputFrame {
    // Called every game frame to process queued input
    // Map WASD to Unity movement
    
    if ([self isKeyPressed:0x1D]) { // W
        [self injectDirectionalInput:0 y:1];
    }
    if ([self isKeyPressed:0x04]) { // A
        [self injectDirectionalInput:-1 y:0];
    }
    if ([self isKeyPressed:0x16]) { // S
        [self injectDirectionalInput:0 y:-1];
    }
    if ([self isKeyPressed:0x07]) { // D
        [self injectDirectionalInput:1 y:0];
    }
}

- (void)injectDirectionalInput:(int)x y:(int)y {
    // Send joystick-style input to Unity
    // In Among Us, movement is controlled via PlayerControl.Cl_PlayerCmd
    // We set the Input.GetAxis("Horizontal") and Input.GetAxis("Vertical")
    extern void UnitySetInputAxis(const char* name, float value);
    if (x != 0) UnitySetInputAxis("Horizontal", (float)x);
    if (y != 0) UnitySetInputAxis("Vertical", (float)y);
}

@end

// C-compatible entry points for the dylib
extern "C" void SickoKeyboard_Start() {
    [[SickoKeyboardParser sharedInstance] startListening];
}

extern "C" void SickoKeyboard_Stop() {
    [[SickoKeyboardParser sharedInstance] stopListening];
}

extern "C" void SickoKeyboard_ProcessFrame() {
    [[SickoKeyboardParser sharedInstance] processInputFrame];
}
