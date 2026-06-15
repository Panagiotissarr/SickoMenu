#include "Il2CppApi.h"
#include <dlfcn.h>
#include <cstdio>

namespace {

// Function pointer typedefs matching Il2Cpp exported symbols
typedef void* (*il2cpp_init_t)(const char* domain);
typedef void* (*il2cpp_get_il2cpp_class_t)(void* assembly, const char* ns, const char* name);
typedef void* (*il2cpp_class_from_name_t)(void* image, const char* ns, const char* name);
typedef void* (*il2cpp_class_get_method_from_name_t)(void* klass, const char* name, int params);
typedef void* (*il2cpp_class_get_property_from_name_t)(void* klass, const char* name);
typedef void* (*il2cpp_property_get_get_method_t)(void* prop);
typedef void* (*il2cpp_property_get_set_method_t)(void* prop);
typedef void* (*il2cpp_class_get_field_from_name_t)(void* klass, const char* name);
typedef void* (*il2cpp_runtime_invoke_t)(void* method, void* obj, void** args, void** exc);
typedef void* (*il2cpp_string_new_t)(const char* str);
typedef const char* (*il2cpp_string_chars_t)(void* str);
typedef void* (*il2cpp_object_new_t)(void* klass);
typedef void* (*il2cpp_value_box_t)(void* klass, void* value);
typedef void* (*il2cpp_field_get_value_t)(void* obj, void* field, void* value);
typedef void* (*il2cpp_field_set_value_t)(void* obj, void* field, void* value);
typedef void (*il2cpp_thread_attach_t)(void* domain);
typedef void (*il2cpp_thread_detach_t)(void* domain);
typedef void* (*il2cpp_array_new_t)(void* klass, uintptr_t len);
typedef void* (*il2cpp_array_get_t)(void* arr, uintptr_t idx);
typedef void (*il2cpp_array_set_t)(void* arr, uintptr_t idx, void* value);
typedef uintptr_t (*il2cpp_array_length_t)(void* arr);
typedef void (*il2cpp_free_t)(void* ptr);
typedef void* (*il2cpp_domain_get_t)();
typedef void* (*il2cpp_domain_assembly_open_t)(void* domain, const char* path);

// Resolved function pointers
static il2cpp_class_from_name_t fn_class_from_name = nullptr;
static il2cpp_class_get_method_from_name_t fn_method_from_name = nullptr;
static il2cpp_class_get_property_from_name_t fn_property_from_name = nullptr;
static il2cpp_property_get_get_method_t fn_property_get_get = nullptr;
static il2cpp_property_get_set_method_t fn_property_get_set = nullptr;
static il2cpp_class_get_field_from_name_t fn_field_from_name = nullptr;
static il2cpp_runtime_invoke_t fn_runtime_invoke = nullptr;
static il2cpp_string_new_t fn_string_new = nullptr;
static il2cpp_string_chars_t fn_string_chars = nullptr;
static il2cpp_object_new_t fn_object_new = nullptr;
static il2cpp_value_box_t fn_value_box = nullptr;
static il2cpp_field_get_value_t fn_field_get_value = nullptr;
static il2cpp_field_set_value_t fn_field_set_value = nullptr;
static il2cpp_thread_attach_t fn_thread_attach = nullptr;
static il2cpp_thread_detach_t fn_thread_detach = nullptr;
static il2cpp_array_new_t fn_array_new = nullptr;
static il2cpp_array_get_t fn_array_get = nullptr;
static il2cpp_array_set_t fn_array_set = nullptr;
static il2cpp_array_length_t fn_array_length = nullptr;
static il2cpp_free_t fn_free = nullptr;
static il2cpp_domain_get_t fn_domain_get = nullptr;
static il2cpp_domain_assembly_open_t fn_domain_assembly_open = nullptr;

template<typename T>
T Resolve(void* handle, const char* name) {
    auto ptr = reinterpret_cast<T>(dlsym(handle, name));
    if (!ptr)
        printf("[Sicko] Failed to resolve Il2Cpp API: %s\n", name);
    return ptr;
}

} // anonymous namespace

bool sicko::il2cpp::Initialize() {
    // Try to find the Il2Cpp runtime library
    void* handle = nullptr;
    
#ifdef PLATFORM_WINDOWS
    handle = GetModuleHandleA("GameAssembly.dll");
#elif defined(PLATFORM_IOS)
    handle = dlopen("/usr/lib/libil2cpp.dylib", RTLD_NOLOAD);
    if (!handle) handle = dlopen(NULL, RTLD_NOLOAD);
#elif defined(PLATFORM_ANDROID)
    handle = dlopen("libil2cpp.so", RTLD_NOLOAD);
    if (!handle) handle = dlopen(NULL, RTLD_NOLOAD);
#else
    handle = dlopen(NULL, RTLD_NOLOAD);
#endif
    
    if (!handle) {
        printf("[Sicko] Could not open Il2Cpp runtime\n");
        return false;
    }
    
    // Resolve all function pointers
    fn_class_from_name = Resolve<il2cpp_class_from_name_t>(handle, "il2cpp_class_from_name");
    fn_method_from_name = Resolve<il2cpp_class_get_method_from_name_t>(handle, "il2cpp_class_get_method_from_name");
    fn_property_from_name = Resolve<il2cpp_class_get_property_from_name_t>(handle, "il2cpp_class_get_property_from_name");
    fn_property_get_get = Resolve<il2cpp_property_get_get_method_t>(handle, "il2cpp_property_get_get_method");
    fn_property_get_set = Resolve<il2cpp_property_get_set_method_t>(handle, "il2cpp_property_get_set_method");
    fn_field_from_name = Resolve<il2cpp_class_get_field_from_name_t>(handle, "il2cpp_class_get_field_from_name");
    fn_runtime_invoke = Resolve<il2cpp_runtime_invoke_t>(handle, "il2cpp_runtime_invoke");
    fn_string_new = Resolve<il2cpp_string_new_t>(handle, "il2cpp_string_new");
    fn_string_chars = Resolve<il2cpp_string_chars_t>(handle, "il2cpp_string_chars");
    fn_object_new = Resolve<il2cpp_object_new_t>(handle, "il2cpp_object_new");
    fn_value_box = Resolve<il2cpp_value_box_t>(handle, "il2cpp_value_box");
    fn_field_get_value = Resolve<il2cpp_field_get_value_t>(handle, "il2cpp_field_get_value");
    fn_field_set_value = Resolve<il2cpp_field_set_value_t>(handle, "il2cpp_field_set_value");
    fn_thread_attach = Resolve<il2cpp_thread_attach_t>(handle, "il2cpp_thread_attach");
    fn_thread_detach = Resolve<il2cpp_thread_detach_t>(handle, "il2cpp_thread_detach");
    fn_array_new = Resolve<il2cpp_array_new_t>(handle, "il2cpp_array_new");
    fn_array_get = Resolve<il2cpp_array_get_t>(handle, "il2cpp_array_get");
    fn_array_set = Resolve<il2cpp_array_set_t>(handle, "il2cpp_array_set");
    fn_array_length = Resolve<il2cpp_array_length_t>(handle, "il2cpp_array_length");
    fn_free = Resolve<il2cpp_free_t>(handle, "il2cpp_free");
    fn_domain_get = Resolve<il2cpp_domain_get_t>(handle, "il2cpp_domain_get");
    fn_domain_assembly_open = Resolve<il2cpp_domain_assembly_open_t>(handle, "il2cpp_domain_assembly_open");
    
    // Check critical functions
    if (!fn_class_from_name || !fn_method_from_name || !fn_runtime_invoke) {
        printf("[Sicko] Critical Il2Cpp functions not found!\n");
        return false;
    }
    
    printf("[Sicko] Il2Cpp API initialized successfully\n");
    return true;
}

void* sicko::il2cpp::GetClass(const char* assemblyName, const char* namespaze, const char* className) {
    return fn_class_from_name(nullptr, namespaze, className);
}

void* sicko::il2cpp::GetClassFromName(const char* namespaze, const char* className) {
    return GetClass(nullptr, namespaze, className);
}

void* sicko::il2cpp::GetMethod(void* klass, const char* methodName, int paramCount) {
    return fn_method_from_name(klass, methodName, paramCount);
}

void* sicko::il2cpp::GetPropertyGetMethod(void* klass, const char* propName) {
    auto prop = fn_property_from_name(klass, propName);
    return prop ? fn_property_get_get(prop) : nullptr;
}

void* sicko::il2cpp::GetPropertySetMethod(void* klass, const char* propName) {
    auto prop = fn_property_from_name(klass, propName);
    return prop ? fn_property_get_set(prop) : nullptr;
}

void* sicko::il2cpp::GetField(void* klass, const char* fieldName) {
    return fn_field_from_name(klass, fieldName);
}

void* sicko::il2cpp::RuntimeInvoke(void* method, void* obj, void** args) {
    void* exc = nullptr;
    auto result = fn_runtime_invoke(method, obj, args, &exc);
    if (exc) printf("[Sicko] RuntimeInvoke exception!\n");
    return result;
}

void* sicko::il2cpp::RuntimeInvokeStatic(void* method, void** args) {
    return RuntimeInvoke(method, nullptr, args);
}

void* sicko::il2cpp::StringNew(const char* str) {
    return fn_string_new(str);
}

const char* sicko::il2cpp::StringChars(void* str) {
    return fn_string_chars(str);
}

void* sicko::il2cpp::ObjectNew(void* klass) {
    return fn_object_new(klass);
}

void* sicko::il2cpp::BoxValueType(void* klass, void* value) {
    return fn_value_box(klass, value);
}

void* sicko::il2cpp::ReadField(void* obj, void* field) {
    void* value = nullptr;
    fn_field_get_value(obj, field, &value);
    return value;
}

void sicko::il2cpp::WriteField(void* obj, void* field, void* value) {
    fn_field_set_value(obj, field, value);
}

void sicko::il2cpp::AttachCurrentThread() {
    if (fn_thread_attach) fn_thread_attach(fn_domain_get());
}

void sicko::il2cpp::DetachCurrentThread() {
    if (fn_thread_detach) fn_thread_detach(fn_domain_get());
}

void* sicko::il2cpp::ArrayNew(void* klass, uintptr_t length) {
    return fn_array_new(klass, length);
}

void* sicko::il2cpp::ArrayGet(void* arr, uintptr_t index) {
    return fn_array_get(arr, index);
}

void sicko::il2cpp::ArraySet(void* arr, uintptr_t index, void* value) {
    fn_array_set(arr, index, value);
}

uintptr_t sicko::il2cpp::ArrayLength(void* arr) {
    return fn_array_length(arr);
}

void sicko::il2cpp::Free(void* ptr) {
    if (fn_free) fn_free(ptr);
}

void* sicko::il2cpp::GetDomain() {
    return fn_domain_get ? fn_domain_get() : nullptr;
}

void* sicko::il2cpp::DomainAssemblyOpen(void* domain, const char* path) {
    return fn_domain_assembly_open ? fn_domain_assembly_open(domain, path) : nullptr;
}
