#pragma once
#include <cstdint>
#include <string>
#include <vector>

// Lightweight C++ wrapper for the Il2Cpp C API
// These are the core Il2Cpp runtime functions exported by all Il2Cpp games.
// Function pointers are resolved at runtime from the game's exported symbols.

namespace sicko::il2cpp {

// Initialize: resolve all Il2Cpp API function pointers from the game binary
bool Initialize();

// --- Assembly / Class / Method resolution ---
void* GetClass(const char* assemblyName, const char* namespaze, const char* className);
void* GetClassFromName(const char* namespaze, const char* className);
void* GetMethod(void* klass, const char* methodName, int paramCount);
void* GetPropertyGetMethod(void* klass, const char* propName);
void* GetPropertySetMethod(void* klass, const char* propName);
void* GetField(void* klass, const char* fieldName);

// --- Invocation ---
void* RuntimeInvoke(void* method, void* obj, void** args);
void* RuntimeInvokeStatic(void* method, void** args);

// --- Object helpers ---
void* StringNew(const char* str);
const char* StringChars(void* str);
void* ObjectNew(void* klass);
void* BoxValueType(void* klass, void* value);

// --- Field access ---
void* ReadField(void* obj, void* field);
void WriteField(void* obj, void* field, void* value);

// --- Thread ---
void AttachCurrentThread();
void DetachCurrentThread();

// --- Array ---
void* ArrayNew(void* klass, uintptr_t length);
void* ArrayGet(void* arr, uintptr_t index);
void ArraySet(void* arr, uintptr_t index, void* value);
uintptr_t ArrayLength(void* arr);

// --- Memory ---
void Free(void* ptr);

// --- Domain ---
void* GetDomain();
void* DomainAssemblyOpen(void* domain, const char* path);

} // namespace sicko::il2cpp
