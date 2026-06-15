namespace SickoMenu.Offsets;

public static class OffsetEntry
{
    public required string MethodSignature { get; init; }
    public required IntPtr MethodPointer { get; set; }
    public IntPtr ResolvedAddress { get; set; }
    public string? AssemblyName { get; init; }
    public bool AutoResolved { get; set; }
}

public static class OffsetSystem
{
    private static readonly List<OffsetEntry> _offsets = [];
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
            RegisterDefaults();
            _initialized = true;
        }
    }

    private static void RegisterDefaults()
    {
        Register("Assembly-CSharp, System.Void PlayerControl::FixedUpdate()");
        Register("Assembly-CSharp, System.Boolean PlayerControl::get_CanMove()");
        Register("Assembly-CSharp, System.Void PlayerControl::MurderPlayer(PlayerControl,MurderResultFlags)");
        Register("Assembly-CSharp, System.Void PlayerControl::CmdCheckMurder(PlayerControl)");
        Register("Assembly-CSharp, System.Void PlayerControl::CheckMurder(PlayerControl)");
        Register("Assembly-CSharp, System.Void PlayerControl::HandleRpc(System.Byte,MessageReader)");
        Register("Assembly-CSharp, System.Void PlayerControl::RpcSyncSettings(System.Byte[])");
        Register("Assembly-CSharp, System.Void PlayerControl::RpcSendChat(System.String)");
        Register("Assembly-CSharp, System.Void PlayerControl::CmdReportDeadBody(NetworkedPlayerInfo)");
        Register("Assembly-CSharp, System.Void PlayerControl::RpcStartMeeting(NetworkedPlayerInfo)");
        Register("Assembly-CSharp, System.Void PlayerControl::StartMeeting(NetworkedPlayerInfo)");
        Register("Assembly-CSharp, System.Void PlayerControl::CompleteTask(System.UInt32)");
        Register("Assembly-CSharp, System.Void PlayerControl::Shapeshift(PlayerControl,System.Boolean)");
        Register("Assembly-CSharp, System.Void PlayerControl::CmdCheckShapeshift(PlayerControl,System.Boolean)");
        Register("Assembly-CSharp, System.Void PlayerControl::ProtectPlayer(PlayerControl,System.Int32)");
        Register("Assembly-CSharp, System.Void MeetingHud::Update()");
        Register("Assembly-CSharp, System.Void MeetingHud::PopulateResults(MeetingHud/VoterState[])");
        Register("Assembly-CSharp, System.Void MeetingHud::Awake()");
        Register("Assembly-CSharp, System.Void MeetingHud::Close()");
        Register("Assembly-CSharp, System.Void HudManager::Update()");
        Register("Assembly-CSharp, System.Void InnerNetClient::Update()");
        Register("Assembly-CSharp, System.Void InnerNetClient::EnqueueDisconnect(DisconnectReasons,System.String)");
        Register("Assembly-CSharp, System.Void AmongUsClient::OnGameJoined(System.String)");
        Register("Assembly-CSharp, System.Void AmongUsClient::OnPlayerLeft(ClientData,DisconnectReasons)");
        Register("Assembly-CSharp, System.Void AmongUsClient::OnPlayerJoined(ClientData)");
        Register("Assembly-CSharp, System.Void AmongUsClient::OnGameEnd(EndGameResult)");
        Register("Assembly-CSharp, System.Void PlayerControl::OnGameStart()");
        Register("Assembly-CSharp, System.Single ShipStatus::CalculateLightRadius(NetworkedPlayerInfo)");
        Register("Assembly-CSharp, System.Void ShipStatus::OnEnable()");
        Register("Assembly-CSharp, System.Void ShipStatus::RpcUpdateSystem(SystemTypes,System.Int32)");
        Register("Assembly-CSharp, System.Void ShipStatus::RpcCloseDoorsOfType(SystemTypes)");
        Register("Assembly-CSharp, System.Void AirshipStatus::OnEnable()");
        Register("Assembly-CSharp, System.Single AirshipStatus::CalculateLightRadius(NetworkedPlayerInfo)");
        Register("Assembly-CSharp, System.Void FungleShipStatus::OnEnable()");
        Register("Assembly-CSharp, System.Void LobbyBehaviour::Start()");
        Register("Assembly-CSharp, System.Void LobbyBehaviour::Update()");
        Register("Assembly-CSharp, System.Void Vent::CanUse(NetworkedPlayerInfo,System.Boolean&,System.Boolean&)");
        Register("Assembly-CSharp, System.Void Vent::EnterVent(PlayerControl)");
        Register("Assembly-CSharp, System.Void Vent::ExitVent(PlayerControl)");
        Register("Assembly-CSharp, System.Boolean ChatBubble::SetName(System.String,System.Boolean,System.Boolean,UnityEngine.Color)");
        Register("Assembly-CSharp, System.Void ChatController::AddChat(PlayerControl,System.String,System.Boolean)");
        Register("Assembly-CSharp, System.Void ChatController::SetVisible(System.Boolean)");
        Register("Assembly-CSharp, System.Void GameStartManager::Update()");
        Register("Assembly-CSharp, System.Void Camera::ScreenToWorldPoint(UnityEngine.Vector3)");
        Register("Assembly-CSharp, System.Void KeyboardJoystick::Update()");
        Register("Assembly-CSharp, System.Void PlayerPhysics::FixedUpdate()");
        Register("Assembly-CSharp, System.Void RoleManager::SelectRoles()");
        Register("Assembly-CSharp, System.Void ExileController::ReEnableGameplay()");
        Register("Assembly-CSharp, System.Void PingTracker::Update()");
        Register("Assembly-CSharp, System.Void FollowerCamera::Update()");
        Register("Assembly-CSharp, System.Void PlayerControl::CmdCheckVanish(System.Single)");
        Register("Assembly-CSharp, System.Void PlayerControl::CmdCheckAppear(System.Boolean)");
        Register("Assembly-CSharp, System.Void SabotageSystemType::SetInitialSabotageCooldown()");
    }

    public static void Register(string signature)
    {
        lock (_lock)
        {
            _offsets.Add(new OffsetEntry
            {
                MethodSignature = signature,
                MethodPointer = IntPtr.Zero,
                ResolvedAddress = IntPtr.Zero,
                AutoResolved = false
            });
        }
    }

    public static bool ResolveAll()
    {
        var success = true;
        lock (_lock)
        {
            foreach (var entry in _offsets)
            {
                if (!ResolveSingle(entry))
                    success = false;
            }
        }
        return success;
    }

    public static bool UpdateOffsets(string jsonOffsets)
    {
        try
        {
            var parsed = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonOffsets);
            if (parsed == null) return false;

            lock (_lock)
            {
                foreach (var (signature, hexPtr) in parsed)
                {
                    var existing = _offsets.FirstOrDefault(o => o.MethodSignature == signature);
                    if (existing != null)
                    {
                        if (long.TryParse(hexPtr.Replace("0x", ""),
                            System.Globalization.NumberStyles.HexNumber, null, out var addr))
                        {
                            existing.MethodPointer = new IntPtr(addr);
                            existing.ResolvedAddress = new IntPtr(addr);
                            existing.AutoResolved = true;
                        }
                    }
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string ExportOffsets()
    {
        var dict = new Dictionary<string, string>();
        lock (_lock)
        {
            foreach (var entry in _offsets)
            {
                if (entry.ResolvedAddress != IntPtr.Zero)
                    dict[entry.MethodSignature] = $"0x{entry.ResolvedAddress.ToString("X16")}";
            }
        }
        return System.Text.Json.JsonSerializer.Serialize(dict, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private static bool ResolveSingle(OffsetEntry entry)
    {
        try
        {
            var parts = entry.MethodSignature.Split(", ", 2);
            if (parts.Length != 2) return false;

            var assemblyName = parts[0];
            var methodPart = parts[1];

            var parenIdx = methodPart.IndexOf('(');
            if (parenIdx == -1) return false;

            var returnAndName = methodPart[..parenIdx].Trim().Split(' ', 2);
            if (returnAndName.Length != 2) return false;

            var qualifiedName = returnAndName[1];
            var doubleColon = qualifiedName.LastIndexOf("::");
            if (doubleColon == -1) return false;

            var className = qualifiedName[..doubleColon];
            var methodName = qualifiedName[(doubleColon + 2)..];

            var paramPart = methodPart[(parenIdx + 1)..methodPart.LastIndexOf(')')];

            var domain = Il2CppInterop.Runtime.IL2CPP.DomainGet();
            if (domain == IntPtr.Zero) return false;

            var assembly = Il2CppInterop.Runtime.IL2CPP.DomainAssemblyOpen(domain, assemblyName);
            if (assembly == IntPtr.Zero) return false;

            var image = Il2CppInterop.Runtime.IL2CPP.AssemblyGetImage(assembly);
            if (image == IntPtr.Zero) return false;

            var nsEnd = className.LastIndexOf('.');
            var ns = nsEnd > -1 ? className[..nsEnd] : "";
            var name = nsEnd > -1 ? className[(nsEnd + 1)..] : className;

            var klass = Il2CppInterop.Runtime.IL2CPP.ClassFromName(image, ns, name);
            if (klass == IntPtr.Zero) return false;

            void* iter = null;
            while (true)
            {
                var methodInfo = Il2CppInterop.Runtime.IL2CPP.ClassGetMethods(klass, ref iter);
                if (methodInfo == IntPtr.Zero) break;

                var namePtr = Il2CppInterop.Runtime.IL2CPP.MethodGetName(methodInfo);
                var currentName = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(namePtr);
                if (currentName != methodName) continue;

                entry.MethodPointer = Il2CppInterop.Runtime.IL2CPP.MethodGetPointer(methodInfo);
                entry.ResolvedAddress = entry.MethodPointer;
                entry.AutoResolved = true;
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsResolved(string signature)
    {
        lock (_lock)
        {
            return _offsets.Any(o => o.MethodSignature == signature && o.ResolvedAddress != IntPtr.Zero);
        }
    }

    public static IntPtr GetOffset(string signature)
    {
        lock (_lock)
        {
            return _offsets.FirstOrDefault(o => o.MethodSignature == signature)?.ResolvedAddress ?? IntPtr.Zero;
        }
    }

    public static string DumpReport()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== SickoMenu Offset Report ===");
        lock (_lock)
        {
            var resolved = _offsets.Count(o => o.AutoResolved);
            var total = _offsets.Count;
            sb.AppendLine($"Resolved: {resolved}/{total}");
            sb.AppendLine();
            foreach (var entry in _offsets)
            {
                var status = entry.AutoResolved ? "[OK]" : "[!!]";
                sb.AppendLine($"  {status} {entry.MethodSignature} => 0x{entry.ResolvedAddress.ToString("X16")}");
            }
        }
        return sb.ToString();
    }
}
