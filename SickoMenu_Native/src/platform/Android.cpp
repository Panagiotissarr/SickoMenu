#include "../il2cpp/Il2CppApi.h"
#include <android/log.h>
#include <jni.h>

#define LOG_TAG "SickoMenu"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO, LOG_TAG, __VA_ARGS__)

extern "C" {

JNIEXPORT jint JNI_OnLoad(JavaVM* vm, void* reserved) {
    LOGI("SickoMenu loading...");
    
    if (!sicko::il2cpp::Initialize()) {
        LOGI("Failed to initialize Il2Cpp API");
        return JNI_VERSION_1_6;
    }
    
    LOGI("SickoMenu initialized successfully");
    return JNI_VERSION_1_6;
}

// Called from the game thread to process each frame
void SickoProcessFrame() {
    // TODO: Process SickoMenu features each frame
}

} // extern "C"
