# **Unity Multiplayer Networking Essentials**

## **1. Networking Fundamentals**

### **1.1 IP Addresses & Ports**
- **Public IP:** Assigned by ISP, globally unique.
- **Private IP:** Used within local networks (e.g., 192.168.x.x).
- **Ports:** Logical endpoints for applications (e.g., port 7777 for game hosting).
- **Port Forwarding:** Required to expose a local game server to the internet.

### **1.2 Communication Models**
- **Client-Server:** Central server manages game state; clients send inputs.
  - Pros: Security, consistency, scalability.
  - Cons: Server cost, single point of failure.
- **Peer-to-Peer (P2P):** Players connect directly.
  - Pros: Lower latency, no server cost.
  - Cons: Cheating risk, synchronization challenges.

### **1.3 Protocols: TCP vs UDP**
- **TCP:** Reliable, ordered, connection-oriented. Higher latency.
- **UDP:** Unreliable, connectionless. Lower latency, ideal for real-time games.
- **Unity Transport Protocol (UTP):** Built on UDP with reliability options.

### **1.4 Key Networking Concepts**
- **Latency:** Delay in data transmission. Affects responsiveness.
- **Bandwidth:** Data transfer rate (Mbps/Gbps).
- **Jitter:** Variation in packet arrival times.
- **Round-Trip Time (RTT):** Time for data to go client → server → client.
- **Tick Rate:** How often the server updates game state (e.g., 60 Hz).
- **Network Hops:** Routers data passes through; each adds latency.

---

## **2. Unity Multiplayer Services: Relay & Lobby**

### **2.1 Why Use Unity Relay?**
- Solves **NAT traversal** and firewall issues.
- Players connect to a public Relay server; no port forwarding needed.
- Works alongside **Lobby** for player discovery and matchmaking.

### **2.2 Relay + Lobby Integration Flow**
1. **Host creates a Relay allocation** (`RelayService.Instance.CreateAllocationAsync`).
2. **Host gets a join code** (`RelayService.Instance.GetJoinCodeAsync`).
3. **Host stores join code in Lobby data** (`CreateLobbyOptions.Data`).
4. **Client joins Lobby** and retrieves the join code.
5. **Client joins Relay session** (`RelayService.Instance.JoinAllocationAsync`).
6. Both host and client configure `UnityTransport` with `SetRelayServerData`.

### **2.3 Code Snippets**

#### **Host-Side:**
```csharp
// Create Relay allocation
Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

// Configure transport
NetworkManager.Singleton.GetComponent<UnityTransport>()
    .SetRelayServerData(new RelayServerData(allocation, "dtls"));

// Create Lobby with join code in data
var options = new CreateLobbyOptions();
options.Data = new Dictionary<string, DataObject>
{
    { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
};
Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

// Start host
NetworkManager.Singleton.StartHost();
```

#### **Client-Side:**
```csharp
// Join Lobby
Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
string joinCode = lobby.Data["RelayJoinCode"].Value;

// Join Relay
JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

// Configure transport and start client
NetworkManager.Singleton.GetComponent<UnityTransport>()
    .SetRelayServerData(new RelayServerData(joinAlloc, "dtls"));
NetworkManager.Singleton.StartClient();
```

---

## **3. Netcode for GameObjects: Core Building Blocks**

### **3.1 NetworkManager & Transport**
- Central singleton for managing connections.
- Uses **Unity Transport (UTP)**.

### **3.2 NetworkObject & NetworkBehaviour**
- `NetworkObject` required on networked prefabs.
- `NetworkBehaviour` is the base class for networked scripts.

### **3.3 NetworkVariable**
- Synchronizes state across clients.
- Server authoritative by default.

Example:
```csharp
private NetworkVariable<float> timer = new NetworkVariable<float>(
    8f,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
);
```

### **3.4 Remote Procedure Calls (RPCs)**
- **ServerRpc:** Client → Server.
- **ClientRpc:** Server → Client(s).

Example:
```csharp
[ServerRpc]
private void ShootServerRpc(ServerRpcParams rpcParams) { }

[ClientRpc]
private void UpdateHealthClientRpc(int health) { }
```

### **3.5 Authority & Ownership**
- **Server Authority:** Server controls gameplay logic.
- **Ownership:** Clients own their player objects; only owner can send input for that object.
- Use `IsServer`, `IsClient`, `IsLocalPlayer` checks.

---

## **4. Data Synchronization in Netcode**

### **4.1 NetworkVariable for State Sync**
- Best for persistent state (health, score, timer).
- Automatically synchronizes to late-joining clients.

### **4.2 RPCs for Events**
- Use for one-off actions (shooting, chat messages, UI updates).

### **4.3 Managed vs Unmanaged Types**
- **Unmanaged:** `int`, `float`, `bool`, `struct` – copied by value.
- **Managed:** `class`, `string`, `array` – require serialization.

### **4.4 Custom Serialization**
Implement `INetworkSerializable` for custom structs:
```csharp
public struct PlayerData : INetworkSerializable
{
    public int Score;
    public bool IsReady;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Score);
        serializer.SerializeValue(ref IsReady);
    }
}
```

---

## **5. Practical Implementation: Network Timer Example**

### **5.1 Requirements**
- `NetworkVariable<float>` for timer (starts at 8).
- Server updates timer; clients read.
- Display using TextMeshPro.

### **5.2 Code Outline**
```csharp
public class NetworkTimer : NetworkBehaviour
{
    private NetworkVariable<float> timeRemaining = new NetworkVariable<float>(8f);
    private bool timerStarted = false;
    public TMP_Text timerText;

    private void Update()
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton.ConnectedClients.Count >= 2 && !timerStarted)
            timerStarted = true;

        if (timerStarted && timeRemaining.Value > 0)
            timeRemaining.Value -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        timerText.text = $"{timeRemaining.Value:F2}";
    }
}
```

---

## **6. 2D Space Shooter Netcode Breakdown**

### **6.1 Key Components**
- **ShipControl:** Player movement, firing, health (uses `NetworkVariable` and RPCs).
- **Bullet:** Projectile logic; server-authoritative physics.
- **Asteroid:** Splits on destruction; uses `NetworkVariable` for size.
- **Powerup:** Applies buffs; server validates pickup.
- **NetworkObjectPool:** Reuses networked prefabs (bullets, asteroids).

### **6.2 Gameplay Flow**
1. **Movement:** Client sends `ServerRpc` with input; server applies physics.
2. **Firing:** Client sends `ServerRpc`; server spawns bullet via pool.
3. **Collisions:** Server detects and applies damage/effects.
4. **Visuals:** Clients handle particles, UI, and sounds via `ClientRpc`.

---

## **7. Troubleshooting Common Issues**

- **“Join code not found”:** Relay allocation expired or not created.
- **“Not authenticated”:** Call `AuthenticationService.Instance.SignInAsync()` first.
- **“KeyNotFoundException for RelayJoinCode”:** Host didn’t store code in Lobby Data.
- **NetworkManager is null:** Ensure `NetworkManager` is in the scene.
- **High latency:** Use region-based servers, optimize bandwidth, prefer wired connections.

---

## **8. Resources & Documentation**

- **Unity Relay Docs:** [docs.unity.com/ugs/manual/relay](https://docs.unity.com/ugs/manual/relay)
- **Netcode for GameObjects:** [docs.unity3d.com/Packages/com.unity.netcode.gameobjects](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects)
- **Lobby Service:** [docs.unity.com/ugs/manual/lobby](https://docs.unity.com/ugs/manual/lobby)
- **Sample Projects:** [Unity Multiplayer Samples GitHub](https://github.com/Unity-Technologies/com.unity.multiplayer.samples)

---