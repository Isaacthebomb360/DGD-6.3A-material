# **Unity Multiplayer Networking: CBA Master Guide**
*Complete Theory + Practical Implementation for Exam Preparation*

---

## **1. Networking Fundamentals - Theory & Practice**

### **1.1 IP Addressing & NAT**

#### **THEORY (CBA Focus):**
- **Public IP Address:** Assigned by ISP, globally unique, required for external communication
- **Private IP Address:** Local network address (192.168.x.x), hidden externally via NAT
- **NAT Problem:** Home routers block incoming connections, preventing direct hosting

#### **PRACTICAL IMPLEMENTATION:**
```csharp
// Port Forwarding Required for Direct Hosting:
// Without Relay: Host must configure router to forward port 7777 to local machine
// Security Risk: Opens network to potential attacks
```

### **1.2 Network Ports**

#### **THEORY (CBA Focus):**
- **Ports:** Logical endpoints (like door numbers) for applications
- **Common Ports:** 80 (HTTP), 443 (HTTPS), 7777 (common game port)
- **Port Forwarding:** Directs external traffic to specific device on local network

#### **PRACTICAL IMPLEMENTATION:**
```csharp
// Unity Default Ports:
// - Unity Transport: 7777 by default
// - Relay Server: Uses temporary ports automatically
```

---

## **2. Communication Models - Exam Focus**

### **2.1 Client-Server Model**

#### **THEORY (CBA Focus):**
**Architecture:**
- Central server as authority managing game state
- Clients connect to server, send inputs, receive updates

**Advantages:**
- **Security:** Centralized control enforces game rules
- **Consistency:** Synchronized game state across players
- **Scalability:** Suitable for large-scale games (MMORPGs, competitive games)

**Exam Note:** Client-Server is **exam preferred** for most scenarios unless specified otherwise

#### **PRACTICAL IMPLEMENTATION:**
```csharp
// Server Authority Pattern:
if (IsServer) 
{
    // Only server executes critical game logic
    // Clients only send inputs via ServerRpc
}
```

### **2.2 Peer-to-Peer (P2P) Model**

#### **THEORY (CBA Focus):**
**Architecture:**
- Decentralized: Each device communicates directly
- Used in older games (early Call of Duty)

**Benefits:**
- Lower costs (no dedicated servers)
- Potential lower latency (direct connections)

**Challenges (EXAM CRITICAL):**
- **Security:** No central authority → cheating risk
- **Synchronization:** Hard to keep game states consistent
- **Latency Variability:** Geographic distance causes lag

#### **PRACTICAL IMPLEMENTATION:**
```csharp
// P2P with Unity Relay (exam likely scenario):
// Use Relay to solve NAT issues in P2P-like setups
// Still client-server technically, but feels like P2P
```

### **2.3 Model Comparison Table (EXAM REFERENCE)**

| **Aspect** | **Client-Server** | **P2P** | **When to Use (EXAM)** |
|------------|-------------------|---------|------------------------|
| **Authority** | Centralized | Decentralized | Client-Server for competitive |
| **Security** | High | Low (cheating risk) | Client-Server when security needed |
| **Resources** | Server required | Lower cost | P2P for casual/small games |
| **Best For** | Large games | Small, informal | Turn-based: P2P; Real-time: Client-Server |

---

## **3. OSI Model & Transport Protocols**

### **3.1 OSI Model (7 Layers) - EXAM DETAIL**

#### **THEORY (CBA Focus):**
1. **Layer 7 - Application:** Your game code, custom protocols
2. **Layer 6 - Presentation:** Data formatting, encryption (JSON, binary)
3. **Layer 5 - Session:** Connection management (matchmaking, lobbies)
4. **Layer 4 - Transport:** TCP/UDP protocols (**most important for games**)
5. **Layer 3 - Network:** IP addressing and routing
6. **Layer 2 - Data Link:** Local network communication (Ethernet)
7. **Layer 1 - Physical:** Hardware (cables, Wi-Fi)

#### **PRACTICAL IMPLEMENTATION:**
```csharp
// Unity works at Layer 4+:
// - Transport Layer: UTP (built on UDP)
// - Session Layer: Lobby service
// - Application Layer: Your game code
```

### **3.2 TCP vs UDP - CRITICAL EXAM DIFFERENCE**

#### **THEORY (CBA Focus):**

**TCP (Transmission Control Protocol):**
- Connection-oriented (three-way handshake: SYN, SYN-ACK, ACK)
- Reliable delivery (guaranteed, in-order)
- Error checking and retransmission
- **Slower:** Higher latency due to overhead
- **Exam Use:** Chat, login, critical data

**UDP (User Datagram Protocol):**
- Connectionless (fire and forget)
- Unreliable (no delivery guarantees)
- No error checking (lost packets = gone)
- **Faster:** Lower latency, minimal overhead
- **Exam Use:** Real-time gameplay where speed > accuracy

#### **PRACTICAL IMPLEMENTATION:**
```csharp
// Unity Transport Protocol (UTP):
// - UDP-based with selective reliability
// - Can configure reliable/unreliable per message
NetworkManager.Singleton.GetComponent<UnityTransport>()
    .SetRelayServerData(allocation, "dtls");  // Uses UDP with encryption
```

---

## **4. Unity Netcode Architectures**

### **4.1 Netcode for GameObjects**

#### **THEORY (CBA Focus):**
- Built-in multiplayer framework for GameObject-based games
- **Best for:** Traditional games, moderate entity counts, complex behaviors
- **Key Features:** State synchronization, RPCs, scene management

#### **PRACTICAL IMPLEMENTATION:**
```csharp
public class PlayerController : NetworkBehaviour  // ← Must inherit from NetworkBehaviour
{
    private NetworkVariable<int> health = new NetworkVariable<int>(100);
    
    [ServerRpc]
    public void ShootServerRpc() { /* Server handles shooting */ }
    
    [ClientRpc]
    public void UpdateHealthClientRpc(int newHealth) { /* Update all clients */ }
}
```

### **4.2 Netcode for Entities (EXAM AWARENESS)**

#### **THEORY (CBA Focus):**
- Built on Unity Transport Package foundation
- **Best for:** MMOs, battle royales, thousands of entities
- **Architecture:** ECS (Entity Component System), data-oriented design
- **Exam Note:** Know this exists but focus on Netcode for GameObject for exam

---

## **5. Data Synchronization - THEORY + CODE**

### **5.1 Data Types (EXAM CRITICAL)**

#### **THEORY (CBA Focus):**
**Unmanaged (By Value) Types:**
- Variables store data directly
- **Examples:** `int`, `float`, `bool`, `enum`, `struct`
- **Network Transfer:** Easy, copied by value

**Managed (By Reference) Types:**
- Variables store references (pointers)
- **Examples:** `class`, `object`, `array`, `string`
- **Network Transfer:** Requires serialization

#### **PRACTICAL IMPLEMENTATION:**
```csharp
// GOOD (unmanaged - works directly):
private NetworkVariable<int> score = new NetworkVariable<int>(0);
private NetworkVariable<float> health = new NetworkVariable<float>(100f);

// BAD (managed - won't work directly):
// private NetworkVariable<string> playerName;  // ERROR!
// private NetworkVariable<GameObject> weapon;  // ERROR!

// SOLUTION: Use custom serialization
public struct PlayerData : INetworkSerializable
{
    public int Score;
    public bool IsReady;
    public FixedString32Bytes Name;  // FixedString for strings
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Score);
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref Name);
    }
}
```

### **5.2 Synchronization Methods (EXAM DECISION TREE)**

#### **THEORY + PRACTICE FLOWCHART:**

```
What needs syncing?
    ↓
Persistent state that late-joining clients need?
    ├── YES → Use NetworkVariable
    │        (health, score, timer, game state)
    │
    └── NO → One-time event/action?
            ├── YES → Client needs to notify Server?
            │        ├── YES → Use ServerRpc
            │        │        (player input, shooting, interactions)
            │        │
            │        └── NO → Server needs to notify Client(s)?
            │                ├── YES → Use ClientRpc
            │                │        (UI updates, effects, game events)
            │                │
            │                └── NO → Consider Custom Messages
            │                        (low-level, custom protocols)
            │
            └── NO → Re-evaluate: Probably NetworkVariable
```

### **5.3 NetworkVariable - COMPLETE GUIDE**

#### **THEORY (CBA Focus):**
- **Purpose:** Synchronize persistent state across clients (including late-joiners)
- **Permissions:** Control who can read/write (Everyone/Owner/Server)
- **Automatic:** Server authoritative by default

#### **PRACTICAL IMPLEMENTATION:**
```csharp
// Basic Declaration:
private NetworkVariable<int> score = new NetworkVariable<int>(
    0,  // Initial value
    NetworkVariableReadPermission.Everyone,  // Who can read
    NetworkVariableWritePermission.Server    // Who can write (CRITICAL: Usually Server!)
);

// Using in code:
void Update()
{
    // Reading (anywhere, anytime):
    int currentScore = score.Value;
    
    // Writing (SERVER ONLY unless Owner permission):
    if (IsServer && Input.GetKeyDown(KeyCode.Space))
    {
        score.Value += 10;  // Only server can write if permission is Server
    }
}

// Reacting to changes:
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();
    
    // Subscribe to value changes
    score.OnValueChanged += (oldValue, newValue) =>
    {
        Debug.Log($"Score changed from {oldValue} to {newValue}");
        UpdateScoreUI(newValue);
    };
}
```

### **5.4 RPCs - COMPLETE GUIDE**

#### **THEORY (CBA Focus):**
**ServerRpc:**
- Client → Server communication
- **Exam Use:** Player actions (shooting, moving, interacting)
- **Key Point:** Executes on server version of the object

**ClientRpc:**
- Server → Client(s) communication
- **Exam Use:** Game events, UI updates, effects
- **Key Point:** Doesn't persist for late-joining clients

#### **PRACTICAL IMPLEMENTATION:**
```csharp
// SERVER RPC EXAMPLE:
[ServerRpc]
private void ShootServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
{
    // Executes on SERVER
    Debug.Log($"Player {rpcParams.Receive.SenderClientId} shot");
    
    // Server validates and applies damage
    ApplyDamage(direction);
    
    // Notify all clients of the shot
    PlayShotEffectClientRpc(direction);
}

// CLIENT RPC EXAMPLE:
[ClientRpc]
private void PlayShotEffectClientRpc(Vector3 position)
{
    // Executes on ALL CLIENTS
    Instantiate(shotEffectPrefab, position, Quaternion.identity);
    audioSource.PlayOneShot(shotSound);
    
    // Note: Late-joining clients won't see this effect!
    // For persistent effects, use NetworkVariable
}

// RPC PARAMETERS EXAMPLES:
[ServerRpc]
void ExampleServerRpc(
    int number,                    // Basic type
    Vector3 position,              // Unity struct
    PlayerData data,               // Custom INetworkSerializable struct
    ServerRpcParams rpcParams      // Automatically filled
) { }

// TARGETING SPECIFIC CLIENTS:
[ClientRpc]
private void UpdatePlayerClientRpc(ClientRpcParams clientRpcParams = default)
{
    // Can target specific clients
    var clientRpcParams = new ClientRpcParams
    {
        Send = new ClientRpcSendParams
        {
            TargetClientIds = new ulong[] { specificClientId }
        }
    };
    UpdatePlayerClientRpc(clientRpcParams);
}
```

---

## **6. Unity Relay & Lobby - EXAM FOCUS**

### **6.1 Why Unity Relay? (EXAM QUESTION)**

#### **THEORY (CBA Focus):**
**The Problem (EXAM SCENARIO):**
- NAT and firewalls block direct connections
- Home networks prevent incoming connections
- Players can't connect to host directly

**The Solution:**
- Unity Relay provides public server both players can reach
- All traffic routes through Relay server
- **No port forwarding needed**
- Works through any firewall

**Exam Diagram:**
```
Host → Creates Relay Allocation → Gets Join Code → Stores in Lobby
                ↑                                       ↓
          Relay Server                            Client retrieves
                ↓                                       ↓
Client ← Joins via Join Code ←─┘              Client configures transport
```

### **6.2 Complete Implementation Flow**

#### **HOST WORKFLOW (MUST KNOW STEP-BY-STEP):**
```csharp
// Step 1: Add required namespaces (EXAM: Know these!)
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobby;
using Unity.Services.Lobby.Models;

// Step 2: Create Relay allocation
Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
// maxConnections = players - 1 (e.g., 4-player game → 3)

// Step 3: Get join code
string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

// Step 4: Configure transport with encryption
NetworkManager.Singleton.GetComponent<UnityTransport>()
    .SetRelayServerData(new RelayServerData(allocation, "dtls"));  // "dtls" for encryption

// Step 5: Create Lobby with join code
var options = new CreateLobbyOptions();
options.Data = new Dictionary<string, DataObject>
{
    { 
        "RelayJoinCode", 
        new DataObject(
            DataObject.VisibilityOptions.Member,  // Members can see, public cannot
            joinCode
        ) 
    }
};
Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("Game Lobby", maxPlayers, options);

// Step 6: Start host (MUST BE AFTER SetRelayServerData!)
NetworkManager.Singleton.StartHost();
```

#### **CLIENT WORKFLOW (MUST KNOW STEP-BY-STEP):**
```csharp
// Step 1: Join Lobby (already has join code in data)
Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

// Step 2: Extract join code from Lobby data
if (!lobby.Data.ContainsKey("RelayJoinCode"))
    throw new Exception("No join code in lobby!");  // Common exam error scenario
string joinCode = lobby.Data["RelayJoinCode"].Value;

// Step 3: Join Relay allocation
JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

// Step 4: Configure transport (SAME as host but with joinAlloc)
NetworkManager.Singleton.GetComponent<UnityTransport>()
    .SetRelayServerData(new RelayServerData(joinAlloc, "dtls"));  // Same "dtls"

// Step 5: Start client
NetworkManager.Singleton.StartClient();
```

### **6.3 Key Exam Insights**

1. **Lobby is just a "meeting point"** - Game traffic goes through Relay, not Lobby
2. **Join codes are temporary** - Expire if allocation is released
3. **"dtls" is mandatory** - Exam expects encrypted connections
4. **Order matters** - `SetRelayServerData()` must be called BEFORE `StartHost()`/`StartClient()`
5. **Visibility options** - Join code should be `Member` visibility, not public

---

## **7. Network Performance Concepts - EXAM THEORY**

### **7.1 Critical Metrics (DEFINITIONS)**

#### **Latency (Round-Trip Time - RTT):**
- **Definition:** Time for data to go client → server → client
- **Impact:** Lower RTT = faster responses = better gameplay
- **Factors:** Distance, network hops, traffic, server performance
- **Exam Mitigation:** Regional servers, optimize server code

#### **Bandwidth:**
- **Definition:** Maximum data transfer rate (Mbps/Gbps)
- **Analogy:** Number of lanes on highway (width ≠ speed!)
- **Conversion:** Mbps ÷ 8 = MB/s (800 Mbps = 100 MB/s download)
- **Impact:** Affects how quickly game data can be sent

#### **Jitter:**
- **Definition:** Fluctuations in packet arrival times
- **Effect:** Choppy, uneven player movements
- **Causes:** Network congestion, buffering
- **Mitigation:** QoS (Quality of Service) prioritization

#### **Server Tick Rate:**
- **Definition:** How often server updates game state (Hz)
- **Examples (EXAM REFERENCE):**
  - CS:GO: 64Hz (Matchmaking) / 128Hz (Professional)
  - Valorant: 128Hz consistent
  - Fortnite: 30Hz server (100 players) / 60Hz client
  - Rocket League: 120Hz (physics-heavy)
- **Choosing:** Fast-paced = higher tick rate; Turn-based = lower

### **7.2 Smoothing Techniques (EXAM QUESTION)**

#### **Interpolation:**
- **What:** Smooths between known past positions
- **Advantage:** Always accurate (uses real data)
- **Disadvantage:** Adds latency (uses past data)
- **Exam Use:** When accuracy is critical

#### **Extrapolation:**
- **What:** Predicts future positions
- **Advantage:** Responsive (no added latency)
- **Disadvantage:** Can be wrong (requires corrections)
- **Exam Use:** When responsiveness is critical

#### **Exam Decision:**
- **Question:** "Players are jittery, how to smooth movement?"
- **Answer:** Implement interpolation (most common exam answer)
- **Reason:** Accuracy over responsiveness for most games

---

## **8. Authority & Ownership - EXAM CRITICAL**

### **8.1 Server Authority Pattern**

#### **THEORY (CBA Focus):**
- **Golden Rule:** Server controls all critical game logic
- **Clients:** Only send inputs, never make decisions
- **Why:** Prevents cheating, ensures consistency

#### **PRACTICAL IMPLEMENTATION:**
```csharp
public class PlayerController : NetworkBehaviour
{
    private NetworkVariable<int> health = new NetworkVariable<int>(100);
    
    void Update()
    {
        // CLIENT: Detect input, send to server
        if (IsLocalPlayer && Input.GetKeyDown(KeyCode.Space))
        {
            RequestShootServerRpc();  // Client requests
        }
    }
    
    [ServerRpc]
    private void RequestShootServerRpc(ServerRpcParams rpcParams = default)
    {
        // SERVER: Validates and executes
        if (CanShoot(rpcParams.Receive.SenderClientId))
        {
            Shoot();  // Server decides
            UpdateClientsClientRpc();  // Server notifies
        }
    }
}
```

### **8.2 Ownership Concepts**

#### **THEORY (CBA Focus):**
- **Ownership:** Client owns their player object
- **Only owner** can send input for that object
- **Server** can override any decision

#### **PRACTICAL CHECKS:**
```csharp
// Common checks (EXAM: Know these!):
if (IsServer) { /* Code runs on server */ }
if (IsClient) { /* Code runs on clients */ }
if (IsLocalPlayer) { /* Code runs for local player only */ }
if (IsOwner) { /* Code runs for object owner */ }

// EXAM PATTERN:
void Update()
{
    if (!IsLocalPlayer) return;  // Only local player processes input
    if (!IsOwner) return;        // Only owner controls this object
    
    // Input processing here
}
```

---

## **9. Troubleshooting - EXAM SCENARIOS**

### **9.1 Common Issues Table**

| **Problem** | **Cause** | **Solution (EXAM ANSWER)** |
|-------------|-----------|----------------------------|
| **"Join code not found"** | Relay allocation expired/not created | Host must call `CreateAllocationAsync()` |
| **"Not authenticated"** | Player not signed in | Call `AuthenticationService.Instance.SignInAsync()` first |
| **KeyNotFoundException** | Host didn't store join code in Lobby | Check `CreateLobbyOptions.Data` setup |
| **NetworkManager null** | Not in scene or misconfigured | Add NetworkManager + UnityTransport to scene |
| **High latency** | Distance, network hops | Use regional servers, wired connections |
| **Jitter** | Network congestion | Implement QoS, prioritize game traffic |

### **9.2 Common Code Bugs (EXAM QUESTION FORMAT)**

#### **BUGGY CODE EXAMPLE (TYPICAL EXAM QUESTION):**
```csharp
public class PlayerHealth : MonoBehaviour  // ← ERROR 1: Should be NetworkBehaviour
{
    private int currentHealth = 100;  // ← ERROR 2: Should be NetworkVariable
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;  // ← ERROR 3: Client modifying state directly
    }
    
    public void Heal(int amount)
    {
        currentHealth += amount;  // ← ERROR 4: No server authority
    }
}
```

#### **CORRECTED CODE (EXAM ANSWER):**
```csharp
public class PlayerHealth : NetworkBehaviour  // FIX 1: Inherit from NetworkBehaviour
{
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server  // FIX 2: Server controls
    );
    
    [ServerRpc]  // FIX 3: Client requests, server executes
    public void TakeDamageServerRpc(int damage, ServerRpcParams rpcParams = default)
    {
        currentHealth.Value -= damage;
        if (currentHealth.Value <= 0) Die();
    }
    
    [ServerRpc]  // FIX 4: All state changes via ServerRpc
    public void HealServerRpc(int amount, ServerRpcParams rpcParams = default)
    {
        currentHealth.Value += amount;
    }
}
```

---

## **10. Mock Exam Questions & Answers**

### **Q1: Network Topology Selection**
**Scenario:** "Design a turn-based mobile game for 4 friends"

**Correct Answer:**
- Use **Peer-to-Peer** with one player as host
- **Why:** Turn-based doesn't need dedicated server, reduces costs
- **Implementation:** Use Unity Relay to handle NAT traversal

### **Q2: Unity Relay Diagram**
**Question:** "Draw and explain Unity Relay flow"

**Answer Structure:**
```
Host → Creates Allocation → Gets Join Code → Stores in Lobby
           ↓                          ↑
      Relay Server              Client retrieves
           ↑                          ↓
Client ← Joins with Code ←───────┘
```

**Benefits:**
- NAT traversal (no port forwarding)
- Simplified connection setup

**Drawbacks:**
- Added latency (extra hop)
- Service costs

### **Q3: Movement Smoothing**
**Question:** "Players move choppily, how to fix?"

**Answer:** Implement **interpolation**
- **Reason:** Uses known past positions for accuracy
- **Alternative:** Extrapolation for responsiveness (but less accurate)

### **Q4: NetworkVariable vs RPC Choice**
**Scenario Table (MATCHING QUESTION):**

| **Scenario** | **Correct Choice** | **Reason** |
|--------------|-------------------|------------|
| Player health | NetworkVariable | Persistent state, late-joiners need it |
| Sound effect | ClientRpc | One-time event, no persistence needed |
| Player submitting answer | ServerRpc | Client → Server communication |

### **Q5: Bandwidth Optimization**
**Question:** "Game has 100 players, bandwidth is high, how to optimize?"

**Answer:** **Area of Interest Filtering**
- **Techniques:** 
  1. Delta compression (send only changes)
  2. Area of interest (only nearby players)
  3. Quantization (reduce precision)
  4. Lower update rates for distant objects
  5. Object pooling

### **Q6: Chess Game Architecture**
**Question:** "Design multiplayer chess"

**Answer:**
1. **Topology:** Host-client (one player hosts)
2. **Board State:** `NetworkVariable<ChessBoard>` 
3. **Turn Management:** `NetworkVariable<bool> isWhiteTurn`
4. **Moves:** `[ServerRpc] SubmitMove()` with validation
5. **Updates:** `[ClientRpc] BroadcastMove()` to all clients

---

## **11. Best Practices Checklist (EXAM PREP)**

### **✓ Security & Performance:**
- [ ] Always use **"dtls"** for encrypted Relay connections
- [ ] Implement **server authority** for all critical logic
- [ ] Use `INetworkSerializable` for custom data types
- [ ] Pool `NetworkObject`s for performance
- [ ] Set proper `NetworkVariable` permissions (Server write, Everyone read)

### **✓ Code Organization:**
- [ ] Inherit from `NetworkBehaviour`, not `MonoBehaviour`
- [ ] Use `IsServer`, `IsClient`, `IsLocalPlayer` checks
- [ ] Separate server and client responsibilities
- [ ] Add error handling for network operations
- [ ] Include debug logging for network events

### **✓ Relay Implementation:**
- [ ] Host: `CreateAllocationAsync` → `GetJoinCodeAsync` → Store in Lobby → `SetRelayServerData` → `StartHost`
- [ ] Client: Join Lobby → Get code → `JoinAllocationAsync` → `SetRelayServerData` → `StartClient`
- [ ] Use `DataObject.VisibilityOptions.Member` for join codes
- [ ] Call `SetRelayServerData()` BEFORE starting host/client

### **✓ Testing (EXAM AWARENESS):**
- [ ] Test with two instances (ParrelSync or builds)
- [ ] Verify late-join client behavior
- [ ] Test disconnection/reconnection scenarios
- [ ] Monitor bandwidth usage

---

## **12. Quick Reference & Glossary**

### **Key API Classes (EXAM):**
- `RelayService.Instance` - Main Relay interface
- `LobbyService.Instance` - Main Lobby interface  
- `NetworkManager.Singleton` - Central Netcode manager
- `Allocation` / `JoinAllocation` - Relay connection data
- `RelayServerData` - Transport configuration

### **Permission Types:**
- **Read:** Everyone, Owner
- **Write:** Server, Owner (usually Server!)

### **Connection Types:**
- `"dtls"` - Encrypted (ALWAYS USE THIS)
- `"udp"` - Unencrypted (don't use)
- `"wss"` - WebSocket (web games)

### **Common Ports:**
- `7777` - Default Unity game port
- `80` - HTTP
- `443` - HTTPS

### **Performance Targets:**
- **Good Latency:** < 100ms
- **Competitive Latency:** < 50ms  
- **Tick Rate Range:** 20Hz (battle royale) - 128Hz (competitive FPS)

---

## **13. Exam Strategy & Tips**

### **During Exam:**
1. **Read carefully** - Many questions have subtle "gotchas"
2. **Identify keywords** - "persistent", "late-joining", "real-time", "turn-based"
3. **Follow patterns** - Server authority, encrypted connections, proper permissions
4. **Draw diagrams** - For architecture questions, visual helps
5. **Check order** - Relay setup order is commonly tested

### **Common Patterns (MEMORIZE THESE):**
```
State persistence needed? → NetworkVariable
Client to server? → ServerRpc  
Server to client(s)? → ClientRpc
Real-time action? → UDP/UTP
Critical data? → TCP or reliable UTP
Security needed? → Server authority + dtls
```

### **Red Flags in Questions:**
- Client modifying game state directly → WRONG
- No encryption in Relay → WRONG  
- Public join codes → WRONG (should be Member visibility)
- Client making game decisions → WRONG (server should decide)

---

## **14. Final Review Checklist**

### **Before Exam, Ensure You Can:**
- [ ] Explain Client-Server vs P2P with examples
- [ ] Write proper NetworkVariable declarations
- [ ] Differentiate ServerRpc vs ClientRpc use cases
- [ ] Diagram Unity Relay + Lobby flow
- [ ] Identify and fix common networking bugs
- [ ] Choose correct sync method for given scenarios
- [ ] Explain latency vs bandwidth vs jitter
- [ ] Describe interpolation vs extrapolation
- [ ] List bandwidth optimization techniques
- [ ] Implement server authority pattern

---

**Good luck on your exam! Remember: Server authority, encrypted connections, and proper synchronization methods are key themes throughout the Unity multiplayer networking curriculum.**
