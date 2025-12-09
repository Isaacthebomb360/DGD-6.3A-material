using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace Network
{
    public class NetworkPlayerSpawner : NetworkBehaviour
    {
        public static NetworkPlayerSpawner Instance { get; private set; }
        
        [Header("Player Setup")]
        [SerializeField] private GameObject playerPrefab;
        
        [Header("Player Materials")]
        [SerializeField] private List<Material> playerMaterials = new List<Material>();
        
        [Header("Spawn Settings")]
        [SerializeField] private float spawnRadius = 5f;
        [SerializeField] private int maxPlayers = 4;
        
        [Header("Unit Spawning")]
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private int unitsPerPlayer = 3;
        [SerializeField] private float unitSpawnDistance = 2f; // Distance from player spawn point
        [SerializeField] private float unitCircleRadius = 1f; // Radius of the circle units spawn in
        
        private Dictionary<ulong, GameObject> spawnedPlayers = new Dictionary<ulong, GameObject>();
        private Dictionary<ulong, int> clientIdToSpawnSlot = new Dictionary<ulong, int>();
        private HashSet<int> usedSpawnSlots = new HashSet<int>();
        private Dictionary<ulong, List<GameObject>> spawnedUnits = new Dictionary<ulong, List<GameObject>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (!IsServer) return;
            
            // Subscribe to connection events
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            
            // Spawn players for already connected clients (like the host)
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!spawnedPlayers.ContainsKey(clientId))
                {
                    SpawnPlayerForClient(clientId);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;
            SpawnPlayerForClient(clientId);
        }

        //Server code to spawn player
        private void SpawnPlayerForClient(ulong clientId)
        {
            if (!IsServer) return;
            
            // Don't spawn if already spawned
            if (spawnedPlayers.ContainsKey(clientId))
            {
                Debug.LogWarning($"Player for client {clientId} already spawned!");
                return;
            }
            
            // Assign a spawn slot for this client
            int spawnSlot = AssignSpawnSlot(clientId);
            if (spawnSlot == -1)
            {
                Debug.LogError($"Cannot spawn player for client {clientId} - no available spawn slots!");
                return;
            }
            
            // Spawn player prefab
            if (playerPrefab != null)
            {
                Vector3 spawnPosition = GetSpawnPosition(spawnSlot);
                GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                
                // Get or add NetworkObject component
                NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
                if (networkObject == null)
                {
                    networkObject = playerInstance.AddComponent<NetworkObject>();
                }
                
                // Setup material BEFORE spawning the network object
                // This ensures the NetworkVariable is set before clients see it
                NetworkPlayerMaterial playerMaterial = playerInstance.GetComponent<NetworkPlayerMaterial>();
                
                // Calculate material index using spawn slot
                int materialIndex = spawnSlot % playerMaterials.Count;
                Material selectedMaterial = playerMaterials != null && materialIndex < playerMaterials.Count ? playerMaterials[materialIndex] : null;
                
                // Spawn the network object
                networkObject.SpawnAsPlayerObject(clientId);
                
                // Apply material AFTER spawning (this will sync to all clients via NetworkVariable)
                if (selectedMaterial != null)
                {
                    Debug.Log($"[Server] Setting material index {materialIndex} for client {clientId} (spawn slot {spawnSlot})");
                    playerMaterial.SetMaterialIndex(materialIndex, selectedMaterial);
                }
                else
                {
                    Debug.LogWarning($"Material at index {materialIndex} is null or materials list is empty!");
                }
                
                // Store reference
                spawnedPlayers[clientId] = playerInstance;
                
                Debug.Log($"Spawned player for client {clientId} at spawn slot {spawnSlot}, position {spawnPosition}");
                
                // Spawn units for this player
                if (unitPrefab != null && unitsPerPlayer > 0)
                {
                    SpawnUnitsForPlayer(clientId, spawnSlot, materialIndex);
                }
            }
            else
            {
                Debug.LogWarning("PlayerPrefab is not assigned in NetworkPlayerSpawner!");
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;
            
            // Clean up spawned units
            if (spawnedUnits.ContainsKey(clientId))
            {
                foreach (var unit in spawnedUnits[clientId])
                {
                    if (unit != null)
                    {
                        Destroy(unit);
                    }
                }
                spawnedUnits.Remove(clientId);
                Debug.Log($"Removed units for client {clientId}");
            }
            
            // Clean up spawned player
            if (spawnedPlayers.ContainsKey(clientId))
            {
                if (spawnedPlayers[clientId] != null)
                {
                    Destroy(spawnedPlayers[clientId]);
                }
                spawnedPlayers.Remove(clientId);
                Debug.Log($"Removed player for client {clientId}");
            }
            
            // Free up the spawn slot
            if (clientIdToSpawnSlot.ContainsKey(clientId))
            {
                int spawnSlot = clientIdToSpawnSlot[clientId];
                usedSpawnSlots.Remove(spawnSlot);
                clientIdToSpawnSlot.Remove(clientId);
                Debug.Log($"Freed spawn slot {spawnSlot} for client {clientId}");
            }
        }

        /// <summary>
        /// Spawns units for a player in a circle formation.
        /// </summary>
        private void SpawnUnitsForPlayer(ulong clientId, int spawnSlot, int materialIndex)
        {
            if (!IsServer) return;
            
            List<GameObject> units = new List<GameObject>();
            Vector3 playerSpawnPos = GetSpawnPosition(spawnSlot);
            
            // Calculate the center point where units will spawn (offset from player spawn)
            float angleStep = 360f / maxPlayers;
            Quaternion playerRotation = Quaternion.Euler(0, (spawnSlot * angleStep) + 180f, 0);
            Vector3 unitSpawnCenter = playerSpawnPos + playerRotation * new Vector3(0, 0, unitSpawnDistance);
            
            // Spawn units in a circle around the center point
            for (int i = 0; i < unitsPerPlayer; i++)
            {
                float angle = (i * 360f / unitsPerPlayer) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * unitCircleRadius,
                    0f,
                    Mathf.Sin(angle) * unitCircleRadius
                );
                
                Vector3 spawnPosition = unitSpawnCenter + offset;
                GameObject unitInstance = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
                
                // Setup network object
                NetworkObject networkObject = unitInstance.GetComponent<NetworkObject>();
                if (networkObject == null)
                {
                    networkObject = unitInstance.AddComponent<NetworkObject>();
                }
                
                // Apply the same material as the player
                NetworkPlayerMaterial unitMaterial = unitInstance.GetComponent<NetworkPlayerMaterial>();
                if (unitMaterial != null && playerMaterials != null && materialIndex < playerMaterials.Count)
                {
                    Material selectedMaterial = playerMaterials[materialIndex];
                    networkObject.Spawn();
                    unitMaterial.SetMaterialIndex(materialIndex, selectedMaterial);
                }
                else
                {
                    networkObject.Spawn();
                }
                
                units.Add(unitInstance);
                Debug.Log($"Spawned unit {i + 1}/{unitsPerPlayer} for client {clientId} at position {spawnPosition}");
            }
            
            spawnedUnits[clientId] = units;
        }

        /// <summary>
        /// Public method to get a material by index - used by NetworkPlayerMaterial
        /// </summary>
        public Material GetMaterialByIndex(int index)
        {
            if (playerMaterials == null || index < 0 || index >= playerMaterials.Count)
            {
                return null;
            }
            return playerMaterials[index];
        }

        /// <summary>
        /// Assigns a spawn slot to a client. This ensures consistent spawning even if client reconnects.
        /// </summary>
        private int AssignSpawnSlot(ulong clientId)
        {
            // Check if this client already has a spawn slot assigned
            if (clientIdToSpawnSlot.ContainsKey(clientId))
            {
                return clientIdToSpawnSlot[clientId];
            }
            
            // Find the first available spawn slot
            for (int i = 0; i < maxPlayers; i++)
            {
                if (!usedSpawnSlots.Contains(i))
                {
                    usedSpawnSlots.Add(i);
                    clientIdToSpawnSlot[clientId] = i;
                    return i;
                }
            }
            
            // No available slots
            return -1;
        }

        /// <summary>
        /// Gets the spawn position for a player based on their spawn slot.
        /// Players spawn in a circle around the origin.
        /// </summary>
        private Vector3 GetSpawnPosition(int spawnSlot)
        {
            return GetSpawnPosition(spawnSlot, spawnRadius);
        }

        /// <summary>
        /// Gets the spawn position for a player with a custom radius.
        /// Useful for spawning units at different distances from center.
        /// </summary>
        public Vector3 GetSpawnPosition(int spawnSlot, float radius)
        {
            float angleStep = 360f / maxPlayers;
            float angle = (spawnSlot * angleStep) * Mathf.Deg2Rad;
            
            return new Vector3(
                Mathf.Cos(angle) * radius,
                1f, // Spawn slightly above ground
                Mathf.Sin(angle) * radius
            );
        }

        /// <summary>
        /// Gets a spawn position with an offset from the base spawn point.
        /// Useful for spawning multiple units for a player.
        /// </summary>
        /// <param name="spawnSlot">The player's spawn slot</param>
        /// <param name="offset">Local offset from the spawn point (relative to player's facing direction)</param>
        /// <param name="radius">Custom radius (optional, uses default if not specified)</param>
        public Vector3 GetSpawnPositionWithOffset(int spawnSlot, Vector3 offset, float radius = -1f)
        {
            if (radius < 0)
            {
                radius = spawnRadius;
            }
            
            Vector3 basePosition = GetSpawnPosition(spawnSlot, radius);
            
            // Calculate rotation based on spawn slot (player faces inward toward center)
            float angleStep = 360f / maxPlayers;
            Quaternion rotation = Quaternion.Euler(0, (spawnSlot * angleStep) + 180f, 0);
            
            // Apply rotated offset
            return basePosition + rotation * offset;
        }

        /// <summary>
        /// Gets the spawn slot assigned to a specific client.
        /// Returns -1 if client has no assigned slot.
        /// </summary>
        public int GetSpawnSlot(ulong clientId)
        {
            if (clientIdToSpawnSlot.ContainsKey(clientId))
            {
                return clientIdToSpawnSlot[clientId];
            }
            return -1;
        }
    }
}
