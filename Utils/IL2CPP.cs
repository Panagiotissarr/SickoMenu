using Il2CppInterop.Runtime.InteropTypes;

namespace SickoMenu.Utils;

public static class IL2CPP
{
    public static void EnsureInitialized() { }
}

public class Il2CppSystemObject
{
    private readonly IntPtr _ptr;

    public Il2CppSystemObject(IntPtr ptr)
    {
        _ptr = ptr;
    }

    public T? TryCast<T>() where T : Il2CppObjectBase
    {
        return null;
    }

    public IntPtr Pointer => _ptr;
}
