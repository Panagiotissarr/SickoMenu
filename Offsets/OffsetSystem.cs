namespace SickoMenu.Offsets;

public class OffsetEntry
{
    public string MethodSignature { get; init; } = string.Empty;
    public IntPtr MethodPointer { get; set; }
    public IntPtr ResolvedAddress { get; set; }
    public string? AssemblyName { get; init; }
    public bool AutoResolved { get; set; }
}

public static class OffsetSystem
{
    private static readonly List<OffsetEntry> _offsets = new List<OffsetEntry>();
    private static readonly object _lock = new();
    private static bool _initialized;

    public static IReadOnlyList<OffsetEntry> Offsets
    {
        get
        {
            lock (_lock)
                return _offsets.ToList();
        }
    }

    public static void Initialize()
    {
        lock (_lock)
        {
            if (_initialized) return;
            _initialized = true;
        }
    }

    public static void Register(string methodSignature, string? assemblyName = null)
    {
        lock (_lock)
        {
            _offsets.Add(new OffsetEntry
            {
                MethodSignature = methodSignature,
                AssemblyName = assemblyName
            });
        }
    }

    public static bool ResolveAll()
    {
        lock (_lock)
        {
            foreach (var entry in _offsets)
            {
                if (entry.ResolvedAddress == IntPtr.Zero)
                    TryAutoResolve(entry);
            }
            return _offsets.Count == 0 || _offsets.All(o => o.ResolvedAddress != IntPtr.Zero);
        }
    }

    private static bool TryAutoResolve(OffsetEntry entry)
    {
        return false;
    }

    public static bool IsResolved(string signature)
    {
        lock (_lock)
        {
            return _offsets.Any(o => o.MethodSignature == signature && o.ResolvedAddress != IntPtr.Zero);
        }
    }

    public static IntPtr GetAddress(string signature)
    {
        lock (_lock)
        {
            return _offsets.FirstOrDefault(o => o.MethodSignature == signature)?.ResolvedAddress ?? IntPtr.Zero;
        }
    }

    public static string DumpReport()
    {
        lock (_lock)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Offset Dump ===");
            foreach (var entry in _offsets)
            {
                sb.AppendLine($"{entry.MethodSignature} -> 0x{entry.ResolvedAddress:X16} (Auto: {entry.AutoResolved})");
            }
            return sb.ToString();
        }
    }

    public static string ExportOffsets()
    {
        lock (_lock)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            bool first = true;
            foreach (var entry in _offsets)
            {
                if (!first) sb.AppendLine(",");
                first = false;
                sb.Append($"  \"{entry.MethodSignature}\": \"0x{entry.ResolvedAddress:X16}\"");
            }
            sb.AppendLine();
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
