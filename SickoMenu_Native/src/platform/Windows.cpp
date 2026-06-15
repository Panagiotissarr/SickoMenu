#include "../il2cpp/Il2CppApi.h"
#include <windows.h>

BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID lpReserved) {
    if (reason == DLL_PROCESS_ATTACH) {
        DisableThreadLibraryCalls(hModule);
        CreateThread(nullptr, 0, [](LPVOID) -> DWORD {
            if (!sicko::il2cpp::Initialize())
                return 1;
            // TODO: Start SickoMenu features
            return 0;
        }, nullptr, 0, nullptr);
    }
    return TRUE;
}
