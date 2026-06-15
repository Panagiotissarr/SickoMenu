using System.Reflection;
using System.Runtime.InteropServices;

namespace SickoMenu.Utils;

public static class IL2CPP
{
    private static readonly Dictionary<string, IntPtr> ClassCache = [];
    private static readonly Dictionary<string, IntPtr> MethodCache = [];
    private static readonly Dictionary<string, IntPtr> PropertyCache = [];
    private static readonly object CacheLock = new();

    private static IntPtr _domain;
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized) return;

        try
        {
            _domain = Il2CppInterop.Runtime.IL2CPP.DomainGet();
            _initialized = _domain != IntPtr.Zero;
        }
        catch
        {
            _initialized = false;
        }
    }

    public static IntPtr GetClass(string assembly, string namespaze, string className)
    {
        var key = $"{assembly}:{namespaze}:{className}";

        lock (CacheLock)
        {
            if (ClassCache.TryGetValue(key, out var cached))
                return cached;

            try
            {
                EnsureInitialized();
                if (_domain == IntPtr.Zero) return IntPtr.Zero;

                var asm = Il2CppInterop.Runtime.IL2CPP.DomainAssemblyOpen(_domain, assembly);
                if (asm == IntPtr.Zero) return IntPtr.Zero;

                var image = Il2CppInterop.Runtime.IL2CPP.AssemblyGetImage(asm);
                if (image == IntPtr.Zero) return IntPtr.Zero;

                var klass = Il2CppInterop.Runtime.IL2CPP.ClassFromName(image, namespaze, className);
                if (klass != IntPtr.Zero)
                    ClassCache[key] = klass;

                return klass;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }
    }

    public static IntPtr GetProperty(IntPtr klass, string propertyName)
    {
        var key = $"prop:{klass:X}:{propertyName}";

        lock (CacheLock)
        {
            if (PropertyCache.TryGetValue(key, out var cached))
                return cached;

            try
            {
                var prop = Il2CppInterop.Runtime.IL2CPP.ClassGetPropertyFromName(klass, propertyName);
                if (prop != IntPtr.Zero)
                    PropertyCache[key] = prop;
                return prop;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }
    }

    public static IntPtr GetMethod(IntPtr klass, string methodName, int args = -1)
    {
        var key = $"method:{klass:X}:{methodName}:{args}";

        lock (CacheLock)
        {
            if (MethodCache.TryGetValue(key, out var cached))
                return cached;

            try
            {
                IntPtr method;
                if (args >= 0)
                    method = Il2CppInterop.Runtime.IL2CPP.ClassGetMethodFromName(klass, methodName, args);
                else
                {
                    void* iter = null;
                    method = IntPtr.Zero;
                    while (true)
                    {
                        var m = Il2CppInterop.Runtime.IL2CPP.ClassGetMethods(klass, ref iter);
                        if (m == IntPtr.Zero) break;

                        var namePtr = Il2CppInterop.Runtime.IL2CPP.MethodGetName(m);
                        var name = Marshal.PtrToStringAnsi(namePtr);
                        if (name == methodName)
                        {
                            method = m;
                            break;
                        }
                    }
                }

                if (method != IntPtr.Zero)
                    MethodCache[key] = method;
                return method;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }
    }

    public static IntPtr Invoke(IntPtr obj, string methodName, params object[] args)
    {
        try
        {
            var klass = Il2CppInterop.Runtime.IL2CPP.ObjectGetClass(obj);
            if (klass == IntPtr.Zero) return IntPtr.Zero;

            var method = GetMethod(klass, methodName);
            if (method == IntPtr.Zero) return IntPtr.Zero;

            var methodPtr = Il2CppInterop.Runtime.IL2CPP.MethodGetPointer(method);
            if (methodPtr == IntPtr.Zero) return IntPtr.Zero;
            return IntPtr.Zero;
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    public static IntPtr ManagedStringToIl2Cpp(string str)
    {
        try
        {
            return Il2CppInterop.Runtime.IL2CPP.ManagedStringToIl2Cpp(str);
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    public static Il2CppObjectBase? GetIl2CppObject(IntPtr ptr, string typeName)
    {
        try
        {
            if (ptr == IntPtr.Zero) return null;

            var klass = Il2CppInterop.Runtime.IL2CPP.ClassFromPointer(ptr);
            if (klass == IntPtr.Zero) return null;

            var type = Il2CppInterop.Runtime.IL2CPP.ClassGetType(klass);
            return new Il2CppSystemObject(ptr);
        }
        catch
        {
            return null;
        }
    }

    public static IntPtr StaticInvoke(string assembly, string ns, string className,
        string methodName, params object[] args)
    {
        try
        {
            var klass = GetClass(assembly, ns, className);
            if (klass == IntPtr.Zero) return IntPtr.Zero;

            var method = GetMethod(klass, methodName, args.Length);
            if (method == IntPtr.Zero) return IntPtr.Zero;

            return IntPtr.Zero;
        }
        catch
        {
            return IntPtr.Zero;
        }
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
        try
        {
            return Il2CppInterop.Runtime.IL2CPP.Il2CppObjectBaseToPtr(null) as T;
        }
        catch
        {
            return null;
        }
    }

    public IntPtr Pointer => _ptr;
}
