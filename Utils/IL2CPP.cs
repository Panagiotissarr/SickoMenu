using System.Reflection;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime.InteropTypes;

namespace SickoMenu.Utils;

public static class IL2CPP
{
    private static readonly Dictionary<string, IntPtr> ClassCache = new Dictionary<string, IntPtr>();
    private static readonly Dictionary<string, IntPtr> MethodCache = new Dictionary<string, IntPtr>();
    private static readonly Dictionary<string, IntPtr> PropertyCache = new Dictionary<string, IntPtr>();
    private static readonly object CacheLock = new();

    private static IntPtr _domain;
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;
    }

    public static IntPtr GetClass(string assembly, string namespaze, string className)
    {
        return IntPtr.Zero;
    }

    public static IntPtr GetProperty(IntPtr klass, string propertyName)
    {
        return IntPtr.Zero;
    }

    public static IntPtr GetMethod(IntPtr klass, string methodName, int args = -1)
    {
        return IntPtr.Zero;
    }

    public static IntPtr Invoke(IntPtr obj, string methodName, params object[] args)
    {
        return IntPtr.Zero;
    }

    public static IntPtr ManagedStringToIl2Cpp(string str)
    {
        return IntPtr.Zero;
    }

    public static Il2CppObjectBase? GetIl2CppObject(IntPtr ptr, string typeName)
    {
        return null;
    }

    public static IntPtr StaticInvoke(string assembly, string ns, string className,
        string methodName, params object[] args)
    {
        return IntPtr.Zero;
    }
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
