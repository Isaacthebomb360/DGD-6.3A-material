# Multiplayer Networking & Unity Integration - Master Guide

## 1. Networking Fundamentals

### 1.1 IP Addressing & NAT
1. **Public IP**: Assigned by ISP, globally unique, required for external communication
2. **Private IP**: Used within local networks, hidden externally via NAT
3. **NAT Problem**: Home routers block incoming connections, preventing direct hosting

### 1.2 Network Ports & Forwarding
1. **Network Ports**: Act as "door numbers" for applications (e.g., port 80 = HTTP, 443 = HTTPS)
2. **Port Forwarding**: Redirects external traffic to specific local devices
   - **Steps**: Identify port → Access router → Configure rule → Save and test
   - **Security Risk**: Opens network to potential attacks

### 1.3 Communication Models
1. **Client-Server Model**
   - Central server authorizes game state
   - Clients only send inputs
   - **Advantages**: Security, consistency, scalability
   - **Ideal for**: MMOs, competitive games (Fortnite, WoW)

2. **Peer-to-Peer Model**
   - Direct communication between players
   - **Advantages**: Lower costs, potential lower latency
   - **Disadvantages**: Security vulnerabilities, synchronization issues
   - **Ideal for**: Small, informal games

### 1.4 OSI Model Layers (Relevant to Gaming)
1. **Network Layer (3)**: IP addressing and routing
2. **Transport Layer (4)**: TCP/UDP protocols
3. **Session Layer (5)**: Connection management (lobbies, matchmaking)
4. **Presentation Layer (6)**: Data formatting, encryption
5. **Application Layer (7)**: Game code, custom protocols

### 1.5 Transport Protocols
1. **TCP**
   - Connection-oriented, reliable delivery
   - Slower with more overhead
   - **Best for**: Chat, login, critical data

2. **UDP**
   - Connectionless, unreliable
   - Faster with minimal overhead
   - **Best for**: Real-time gameplay

3. **UTP (Unity Transport Protocol)**
   - UDP-based with selective reliability
   - Supports both reliable and unreliable modes

### 1.6 Performance Metrics
1. **Bandwidth**: Maximum data rate (Mbps/Gbps)
2. **Latency**: Data travel time
3. **Jitter**: Fluctuation in latency
4. **Round Trip Time (RTT)**: Client→Server→Client time
5. **Tick Rate**: Server update frequency
6. **Network Hops**: Intermediate devices between client and server

---

## 2. Unity Relay Integration

### 2.1 Why Use Relay?
1. **Problem**: NAT blocks incoming connections
2. **Solution**: Relay provides public servers both players can reach
3. **No port forwarding** required
4. **All traffic** routes through Relay server

### 2.2 Relay + Lobby Architecture
1. **Lobby**: Handles player discovery and matchmaking
2. **Relay**: Handles actual network connections
3. **Join Codes**: Unique strings stored in Lobby data for clients to connect

### 2.3 Host Implementation Steps

#### Step 1: Add Required Namespaces
```csharp
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
```

#### Step 2: Create Relay Allocation
```csharp
// maxConnections = maxPlayers - 1
Allocation allocation = await RelayService.Instance
    .CreateAllocationAsync(maxConnections);
```

#### Step 3: Get Relay Join Code
```csharp
string relayJoinCode = await RelayService.Instance
    .GetJoinCodeAsync(allocation.AllocationId);
```

#### Step 4: Store Join Code in Lobby Data
```csharp
var options = new CreateLobbyOptions();
options.Data = new Dictionary<string, DataObject> {
    {"RelayJoinCode", new DataObject(
        DataObject.VisibilityOptions.Member, 
        relayJoinCode
    )}
};
```

#### Step 5: Configure Transport
```csharp
NetworkManager.Singleton.GetComponent<UnityTransport>()
    .SetRelayServerData(new RelayServerData(allocation, "dtls"));
```

#### Step 6: Start Host
```csharp
NetworkManager.Singleton.StartHost();
```

### 2.4 Client Implementation Steps

#### Step 1: Retrieve Join Code from Lobby
```csharp
string relayJoinCode = ActiveLobby.Data["RelayJoinCode"].Value;
```

#### Step 2: Join Relay Allocation
```csharp
JoinAllocation joinAllocation = await RelayService.Instance
    .JoinAllocationAsync(relayJoinCode);
```

#### Step 3: Configure Transport & Start Client
```csharp
NetworkManager.Singleton.GetComponent<UnityTransport>()
    .SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
NetworkManager.Singleton.StartClient();
```

---

## 3. Unity Netcode for GameObjects

### 3.1 Data Types for Networking
1. **Unmanaged Types** (by Value): Easy to transfer
   - Int, Float, Bool, Enum, Struct

2. **Managed Types** (by Reference): Require serialization
   - Class, Object, Array, String

### 3.2 Synchronization Methods
1. **NetworkVariables**: Automatic state synchronization
2. **Remote Procedure Calls (RPCs)**: Event-based communication
3. **Custom Messages**: Low-level communication

### 3.3 NetworkVariables
1. **Declaration**:
```csharp
private NetworkVariable<float> timer = new NetworkVariable<float>(
    8f, 
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
);
```

2. **Usage**:
```csharp
// Reading (anywhere)
float current = timer.Value;

// Writing (server only)
if (IsServer) {
    timer.Value = 5f;
}
```

### 3.4 Remote Procedure Calls
1. **ServerRPC**: Client → Server
```csharp
[ServerRpc]
void MyServerRpc(ServerRpcParams params) {
    Debug.Log($"Called by {params.Receive.SenderClientId}");
}
```

2. **ClientRPC**: Server → Client
```csharp
[ClientRpc]
void MyClientRpc(ClientRpcParams params) {
    Debug.Log("Executed on all clients!");
}
```

### 3.5 Network Timer Implementation
```csharp
public class NetworkTimer : NetworkBehaviour {
    private NetworkVariable<float> timerValue = new NetworkVariable<float>(8f);
    private bool timerStarted = false;
    
    void Update() {
        if (!IsServer) return;
        
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2 && !timerStarted) {
            timerStarted = true;
        }
        
        if (timerStarted && timerValue.Value > 0) {
            timerValue.Value -= Time.deltaTime;
            if (timerValue.Value <= 0) {
                timerValue.Value = 0;
                // Timer finished logic
            }
        }
    }
}
```

---

## 4. 2D Space Shooter - Practical Example

### 4.1 Project Structure
1. **NetworkManagerHud.cs**: Connection UI and flow
2. **RandomPositionPlayerSpawner.cs**: Player spawning logic
3. **NetworkObjectPool.cs**: Prefab pooling for performance
4. **ShipControl.cs**: Core player controller with NetworkVariables
5. **Bullet.cs, Asteroid.cs, Powerup.cs**: Networked gameplay objects

### 4.2 Key Design Patterns
1. **Server Authority**: All gameplay logic runs on server
2. **Client Prediction**: Local input handling with server reconciliation
3. **State Synchronization**: NetworkVariables for shared game state
4. **Event Communication**: RPCs for one-time events

### 4.3 Gameplay Flow Examples

#### Movement:
1. Client reads input
2. Client sends ServerRPC with input data
3. Server applies physics
4. Server updates NetworkVariables
5. Clients render updated positions

#### Firing:
1. Client sends fire command via ServerRPC
2. Server checks energy, spawns bullet via pool
3. Server sends ClientRPC for visual/audio feedback
4. Clients play effects locally

---

## 5. Troubleshooting Guide

| # | Problem | Cause | Solution |
|---|---------|-------|----------|
| 1 | "Join code not found" | Allocation expired or never created | Ensure host calls CreateAllocationAsync |
| 2 | Authentication errors | Player not signed in | Call auth.signin before lobby commands |
| 3 | Connection failures | Transport misconfiguration | Use same "dtls" on host and client |
| 4 | Missing RelayJoinCode key | Host didn't store in Lobby Data | Check CreateLobbyOptions.Data setup |
| 5 | NetworkManager null | Not in scene or misconfigured | Add NetworkManager + UnityTransport |
| 6 | High latency | Distance, network hops | Use region-based servers, wired connections |
| 7 | Jitter | Network congestion | Implement QoS, prioritize game traffic |

---

## 6. Best Practices Checklist

### 6.1 Security & Performance
1. [ ] Always use "dtls" for encrypted Relay connections
2. [ ] Implement server authority for critical logic
3. [ ] Use NetworkSerializable for complex data types
4. [ ] Pool NetworkObjects for better performance
5. [ ] Compress game data for faster transmission

### 6.2 Code Organization
1. [ ] Separate server and client responsibilities
2. [ ] Create helper methods for Relay operations
3. [ ] Use proper null checking for Lobby data
4. [ ] Implement proper error handling for network operations
5. [ ] Add debug logging for network events

### 6.3 Testing
1. [ ] Test with two instances (ParrelSync or builds)
2. [ ] Test with different network conditions
3. [ ] Verify late-join client behavior
4. [ ] Test disconnection and reconnection scenarios
5. [ ] Monitor bandwidth usage

---

## 7. Key Resources

### 7.1 Documentation
1. **Unity Relay**: docs.unity.com/ugs/manual/relay
2. **Netcode for GameObjects**: docs.unity3d.com/Packages/com.unity.netcode.gameobjects
3. **Unity Lobby**: docs.unity.com/ugs/manual/lobby

### 7.2 Sample Projects
1. **Game Lobby Sample**: github.com/Unity-Technologies/com.unity-services.samples-game-lobby
2. **2D Space Shooter**: github.com/Unity-Technologies/com.unity.multiplayer.samples.bitesize

### 7.3 Key API Classes
1. **RelayService.Instance** - Main Relay interface
2. **LobbyService.Instance** - Main Lobby interface
3. **NetworkManager.Singleton** - Central Netcode manager
4. **Allocation/JoinAllocation** - Relay connection data
5. **RelayServerData** - Transport configuration

---

## 8. Quick Reference

### 8.1 Host Workflow
```
CreateAllocationAsync → GetJoinCodeAsync → SetRelayServerData → 
StoreInLobbyData → StartHost()
```

### 8.2 Client Workflow
```
JoinLobby → ExtractJoinCode → JoinAllocationAsync → 
SetRelayServerData → StartClient()
```

### 8.3 NetworkVariable Permissions
- **Read**: Everyone, Owner
- **Write**: Server, Owner

### 8.4 RPC Targets
- **ServerRpc**: SendTo.Server
- **ClientRpc**: SendTo.ClientsAndHost, SendTo.Everyone

### 8.5 Connection Types for Relay
- **"dtls"**: Encrypted (recommended)
- **"udp"**: Unencrypted
- **"wss"**: WebSocket
