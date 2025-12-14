# **Unity Multiplayer Networking: Complete Master Guide**

## **1. Networking Fundamentals**

### **1.1 IP Addressing & NAT**
- **Public IP:** Assigned by ISP, globally unique, required for external communication
- **Private IP:** Used within local networks (e.g., 192.168.x.x), hidden externally via NAT
- **NAT Problem:** Home routers block incoming connections, preventing direct hosting without port forwarding
- **Port Forwarding:** Required to expose a local game server to the internet
  - **Steps:** Identify port → Access router → Configure rule → Save and test
  - **Security Risk:** Opens network to potential attacks

### **1.2 Network Ports**
- **Ports:** Logical endpoints for applications (e.g., port 7777 for game hosting, 80 for HTTP)
- Act as "door numbers" for different services on the same IP address

### **1.3 Communication Models**
- **Client-Server:** Central server manages game state; clients send inputs
  - **Pros:** Security, consistency, scalability
  - **Cons:** Server cost, single point of failure
  - **Ideal for:** MMOs, competitive games (Fortnite, WoW)
  
- **Peer-to-Peer (P2P):** Players connect directly
  - **Pros:** Lower latency, no server cost
  - **Cons:** Cheating risk, synchronization challenges
  - **Ideal for:** Small, informal games

### **1.4 OSI Model Layers (Relevant to Gaming)**
1. **Network Layer (3):** IP addressing and routing
2. **Transport Layer (4):** TCP/UDP protocols
3. **Session Layer (5):** Connection management (lobbies, matchmaking)
4. **Presentation Layer (6):** Data formatting, encryption
5. **Application Layer (7):** Game code, custom protocols

### **1.5 Transport Protocols: TCP vs UDP**
- **TCP:** Reliable, ordered, connection-oriented. Higher latency.
  - **Best for:** Chat, login, critical data
  
- **UDP:** Unreliable, connectionless. Lower latency, ideal for real-time games.
  - **Best for:** Real-time gameplay
  
- **Unity Transport Protocol (UTP):** Built on UDP with reliability options
  - UDP-based with selective reliability
  - Supports both reliable and unreliable modes

### **1.6 Key Networking Concepts & Performance Metrics**
- **Latency:** Delay in data transmission. Affects responsiveness.
- **Bandwidth:** Data transfer rate (Mbps/Gbps). Maximum data rate.
- **Jitter:** Variation in packet arrival times. Fluctuation in latency.
- **Round-Trip Time (RTT):** Time for data to go client → server → client.
- **Tick Rate:** How often the server updates game state (e.g., 60 Hz).
- **Network Hops:** Routers data passes through; each adds latency.

---

## **2. Unity Multiplayer Services: Relay & Lobby**

### **2.1 Why Use Unity Relay?**
- Solves **NAT traversal** and firewall issues
- Players connect to a public Relay server; no port forwarding needed
- **All traffic** routes through Relay server
- Works alongside **Lobby** for player discovery and matchmaking

### **2.2 Relay + Lobby Architecture**
1. **Lobby:** Handles player discovery and matchmaking
2. **Relay:** Handles actual network connections
3. **Join Codes:** Unique strings stored in Lobby data for clients to connect

### **2.3 Relay + Lobby Integration Flow**
1. **Host creates a Relay allocation** (`RelayService.Instance.CreateAllocationAsync`)
2. **Host gets a join code** (`RelayService.Instance.GetJoinCodeAsync`)
3. **Host stores join code in Lobby data** (`CreateLobbyOptions.Data`)
4. **Client joins Lobby** and retrieves the join code
5. **Client joins Relay session** (`RelayService.Instance.JoinAllocationAsync`)
6. Both host and client configure `UnityTransport` with `SetRelayServerData`

### **2.4 Implementation Steps**

#### **Host-Side:**
```csharp
// Add required namespaces
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

// Step 1: Create Relay allocation (maxConnections = maxPlayers - 1)
Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

// Step 2: Get Relay join code
string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

// Step 3: Configure transport with "dtls" for encryption
NetworkManager.Singleton.GetComponent<UnityTransport>()
    .SetRelayServerData(new RelayServerData(allocation, "dtls"));

// Step 4: Create Lobby with join code in data
var options = new CreateLobbyOptions();
options.Data = new Dictionary<string, DataObject>
{
    { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
};
Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

// Step 5: Start host
NetworkManager.Singleton.StartHost();
```

#### **Client-Side:**
```csharp
// Step 1: Join Lobby
Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
string joinCode = lobby.Data["RelayJoinCode"].Value;

// Step 2: Join Relay allocation
JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

// Step 3: Configure transport and start client
NetworkManager.Singleton.GetComponent<UnityTransport>()
    .SetRelayServerData(new RelayServerData(joinAlloc, "dtls"));
NetworkManager.Singleton.StartClient();
```

---

## **3. Netcode for GameObjects: Core Building Blocks**

### **3.1 NetworkManager & Transport**
- Central singleton for managing connections
- Uses **Unity Transport (UTP)**

### **3.2 NetworkObject & NetworkBehaviour**
- `NetworkObject` required on networked prefabs
- `NetworkBehaviour` is the base class for networked scripts

### **3.3 Data Types for Networking**
- **Unmanaged Types (by Value):** Easy to transfer
  - `int`, `float`, `bool`, `enum`, `struct`
- **Managed Types (by Reference):** Require serialization
  - `class`, `object`, `array`, `string`

### **3.4 Synchronization Methods**
1. **NetworkVariables:** Automatic state synchronization
2. **Remote Procedure Calls (RPCs):** Event-based communication
3. **Custom Messages:** Low-level communication

### **3.5 NetworkVariable**
- Synchronizes state across clients
- Server authoritative by default
- Automatically synchronizes to late-joining clients

**Declaration:**
```csharp
private NetworkVariable<float> timer = new NetworkVariable<float>(
    8f,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
);
```

**Usage:**
```csharp
// Reading (anywhere)
float current = timer.Value;

// Writing (server only)
if (IsServer) {
    timer.Value = 5f;
}
```

### **3.6 Remote Procedure Calls (RPCs)**
- **ServerRpc:** Client → Server
- **ClientRpc:** Server → Client(s)

**Examples:**
```csharp
[ServerRpc]
private void ShootServerRpc(ServerRpcParams rpcParams) 
{
    Debug.Log($"Called by {rpcParams.Receive.SenderClientId}");
}

[ClientRpc]
private void UpdateHealthClientRpc(int health) 
{
    Debug.Log("Executed on all clients!");
    // Update health UI
}
```

### **3.7 Authority & Ownership**
- **Server Authority:** Server controls gameplay logic
- **Ownership:** Clients own their player objects; only owner can send input for that object
- Use `IsServer`, `IsClient`, `IsLocalPlayer` checks

---

## **4. Data Synchronization in Netcode**

### **4.1 NetworkVariable for State Sync**
- Best for persistent state (health, score, timer)
- Automatically synchronizes to late-joining clients

### **4.2 RPCs for Events**
- Use for one-off actions (shooting, chat messages, UI updates)

### **4.3 Managed vs Unmanaged Types**
- **Unmanaged:** `int`, `float`, `bool`, `struct` – copied by value
- **Managed:** `class`, `string`, `array` – require serialization

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
- `NetworkVariable<float>` for timer (starts at 8)
- Server updates timer; clients read
- Display using TextMeshPro

### **5.2 Complete Implementation**
```csharp
public class NetworkTimer : NetworkBehaviour
{
    private NetworkVariable<float> timeRemaining = new NetworkVariable<float>(8f);
    private bool timerStarted = false;
    public TMP_Text timerText;

    private void Update()
    {
        if (!IsServer) return;

        // Start timer when at least 2 players are connected
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2 && !timerStarted)
            timerStarted = true;

        // Server updates the timer
        if (timerStarted && timeRemaining.Value > 0)
        {
            timeRemaining.Value -= Time.deltaTime;
            if (timeRemaining.Value <= 0)
            {
                timeRemaining.Value = 0;
                // Timer finished logic
            }
        }
    }

    private void LateUpdate()
    {
        // All clients display the timer
        timerText.text = $"{timeRemaining.Value:F2}";
    }
}
```

---

## **6. 2D Space Shooter - Practical Example**

### **6.1 Project Structure**
- **NetworkManagerHud.cs:** Connection UI and flow
- **RandomPositionPlayerSpawner.cs:** Player spawning logic
- **NetworkObjectPool.cs:** Prefab pooling for performance
- **ShipControl.cs:** Core player controller with NetworkVariables
- **Bullet.cs, Asteroid.cs, Powerup.cs:** Networked gameplay objects

### **6.2 Key Design Patterns**
1. **Server Authority:** All gameplay logic runs on server
2. **Client Prediction:** Local input handling with server reconciliation
3. **State Synchronization:** NetworkVariables for shared game state
4. **Event Communication:** RPCs for one-time events
5. **Object Pooling:** Reuses networked prefabs for performance

### **6.3 Gameplay Flow Examples**

#### **Movement:**
1. Client reads input
2. Client sends ServerRPC with input data
3. Server applies physics
4. Server updates NetworkVariables
5. Clients render updated positions

#### **Firing:**
1. Client sends fire command via ServerRPC
2. Server checks energy, spawns bullet via pool
3. Server sends ClientRPC for visual/audio feedback
4. Clients play effects locally

#### **Collisions:**
1. Server detects collisions
2. Server applies damage/effects
3. Server updates health NetworkVariables
4. Server sends ClientRPCs for visual effects

---

## **7. Troubleshooting Common Issues**

| Problem | Cause | Solution |
|---------|-------|----------|
| **"Join code not found"** | Relay allocation expired or not created | Ensure host calls `CreateAllocationAsync` |
| **"Not authenticated"** | Player not signed in | Call `AuthenticationService.Instance.SignInAsync()` first |
| **"KeyNotFoundException for RelayJoinCode"** | Host didn't store code in Lobby Data | Check `CreateLobbyOptions.Data` setup |
| **NetworkManager is null** | Not in scene or misconfigured | Add NetworkManager + UnityTransport to scene |
| **High latency** | Distance, network hops | Use region-based servers, optimize bandwidth, prefer wired connections |
| **Jitter** | Network congestion | Implement QoS, prioritize game traffic |
| **Connection failures** | Transport misconfiguration | Use same "dtls" on host and client |

---

## **8. Best Practices Checklist**

### **8.1 Security & Performance**
- [ ] Always use **"dtls"** for encrypted Relay connections
- [ ] Implement **server authority** for critical logic
- [ ] Use `INetworkSerializable` for complex data types
- [ ] Pool `NetworkObject`s for better performance
- [ ] Compress game data for faster transmission
- [ ] Use appropriate `NetworkVariable` permissions (Server write, Everyone read)

### **8.2 Code Organization**
- [ ] Separate server and client responsibilities
- [ ] Create helper methods for Relay operations
- [ ] Use proper null checking for Lobby data
- [ ] Implement proper error handling for network operations
- [ ] Add debug logging for network events

### **8.3 Testing**
- [ ] Test with two instances (ParrelSync or builds)
- [ ] Test with different network conditions
- [ ] Verify late-join client behavior
- [ ] Test disconnection and reconnection scenarios
- [ ] Monitor bandwidth usage

---

## **9. Resources & Quick Reference**

### **9.1 Documentation**
- **Unity Relay Docs:** [docs.unity.com/ugs/manual/relay](https://docs.unity.com/ugs/manual/relay)
- **Netcode for GameObjects:** [docs.unity3d.com/Packages/com.unity.netcode.gameobjects](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects)
- **Lobby Service:** [docs.unity.com/ugs/manual/lobby](https://docs.unity.com/ugs/manual/lobby)

### **9.2 Sample Projects**
- **Unity Multiplayer Samples GitHub:** [github.com/Unity-Technologies/com.unity.multiplayer.samples](https://github.com/Unity-Technologies/com.unity.multiplayer.samples)
- **Game Lobby Sample:** [github.com/Unity-Technologies/com.unity-services.samples-game-lobby](https://github.com/Unity-Technologies/com.unity-services.samples-game-lobby)
- **2D Space Shooter:** [github.com/Unity-Technologies/com.unity.multiplayer.samples.bitesize](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.bitesize)

### **9.3 Key API Classes**
- **`RelayService.Instance`** - Main Relay interface
- **`LobbyService.Instance`** - Main Lobby interface
- **`NetworkManager.Singleton`** - Central Netcode manager
- **`Allocation`/`JoinAllocation`** - Relay connection data
- **`RelayServerData`** - Transport configuration

### **9.4 Quick Reference Workflows**

#### **Host Workflow:**
```
CreateAllocationAsync → GetJoinCodeAsync → SetRelayServerData → 
StoreInLobbyData → StartHost()
```

#### **Client Workflow:**
```
JoinLobby → ExtractJoinCode → JoinAllocationAsync → 
SetRelayServerData → StartClient()
```

#### **NetworkVariable Permissions:**
- **Read:** Everyone, Owner
- **Write:** Server, Owner

#### **RPC Targets:**
- **ServerRpc:** SendTo.Server
- **ClientRpc:** SendTo.ClientsAndHost, SendTo.Everyone

#### **Connection Types for Relay:**
- **"dtls":** Encrypted (recommended)
- **"udp":** Unencrypted
- **"wss":** WebSocket
