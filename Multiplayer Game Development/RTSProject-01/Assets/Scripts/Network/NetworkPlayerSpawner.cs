using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkPlayerSpawner : NetworkBehaviour
    {
        public static NetworkPlayerSpawner Instance;

        [Header("Player Setup")]
        [SerializeField] private GameObject playerPrefab; // hero character
        [SerializeField]
        private List<Material> playerMaterials;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnRadius = 6f; // outer circle that defines where player spawn
        [SerializeField] private float maxPlayers = 4;

        [Header("Unit Spawning")]
        [SerializeField] private GameObject unitPrefab; // solder
        [SerializeField] private int unitsPerPlayer = 4;
        [SerializeField] private float unitySpawnDistance = 0; //distance from hero to spawnpoint
        [SerializeField] private float unitCircleRadius = 2f; //radius of the cirle units spawn in

        private Dictionary<ulong, GameObject> spawnedPlayers;
        private Dictionary<ulong, List<GameObject>> spawnedUnits;
        private Dictionary<ulong, int> clientIdToSpawnSlot;
        private HashSet<int> usedSpawnSlots;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            spawnedPlayers = new Dictionary<ulong, GameObject>();
            playerMaterials = new List<Material>();
            spawnedUnits = new Dictionary<ulong, List<GameObject>>();
            usedSpawnSlots = new HashSet<int>();
            clientIdToSpawnSlot = new Dictionary<ulong, int>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            //Server code
            if (!IsServer) return;

            //Subscribe to connection events
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;

            //Spawn players
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!spawnedPlayers.ContainsKey(clientId))
                {
                    //spawn the player (host)
                    SpawnPlayerForClient(clientId);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (NetworkManager.Singleton == null) return;

            //Subscribe to connection events
            NetworkManager.Singleton.OnClientConnectedCallback -= ClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= ClientDisconnected;
        }

        private void ClientConnected(ulong clientId)
        {
            //running on Server
            if (!IsServer) return;
            SpawnPlayerForClient(clientId);
        }

        private void ClientDisconnected(ulong clientId)
        {
            //running on Server
            if (!IsServer) return;

            //Clean up spawned player
            if (spawnedPlayers.ContainsKey(clientId))
            {
                if (spawnedPlayers[clientId] != null)
                {
                    Destroy(spawnedPlayers[clientId]);
                }
                spawnedPlayers.Remove(clientId);
                Debug.Log($"Removed player for client {clientId}");
            }
        }

        private void SpawnPlayerForClient(ulong clientId)
        {
            //Run on the server
            if (!IsServer) return;

            if (spawnedPlayers.ContainsKey(clientId))
            {
                Debug.LogWarning($"Player {clientId} already spawned");
                return;
            }

            // assign a spawnslot for the client
            int spawnSlot = AssignSpawnSlot(clientId);
            if (spawnSlot == -1)
            {
                Debug.LogWarning($"No spawn slots available for client {clientId}");
                return;
            }

            if (playerPrefab != null)
            {
                //Spawning the player
                Vector3 spawnPosition = GetSpawnPosition(spawnSlot, spawnRadius);
                GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.SpawnAsPlayerObject(clientId);
                    spawnedPlayers[clientId] = playerInstance;
                    Debug.Log($"Spawned player for client {clientId} at position {spawnPosition}");
                }
                else
                {
                    Debug.LogWarning($"Player {clientId} does not have a NetworkObject attached");
                }
            }
            else
            {
                Debug.LogWarning($"Check the player prefab is set.");
            }
        }

        /// <summary>
        /// calculates a spawn position based on the spawn slot and radius
        /// useful for spawning units at different distances from the center point
        /// </summary>
        /// <param name="spawnSlot"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private Vector3 GetSpawnPosition(int spawnSlot, float radius)
        {
            float agnelStep = 360f / maxPlayers;
            float angle = agnelStep * spawnSlot;
            return new Vector3(
                Mathf.Cos(angle) * radius,
                1f, //spawn it above the ground
                Mathf.Sin(angle) * radius
            );
        }

        /// <summary>
        /// assigns a spawn slot to the client using the private HashSet<int> usedspawnslots. this ensured constent spawning even if the client reconnects</int>
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        private int AssignSpawnSlot(ulong clientId)
        {
            // check if the client already has a spawn slot assigned
            if (clientIdToSpawnSlot.ContainsKey(clientId))
            {
                return clientIdToSpawnSlot[clientId];
            }

            //find the first available spawn slot
            for (int i = 0; i < maxPlayers; i++)
            {
                if (!usedSpawnSlots.Contains(i))
                {
                    usedSpawnSlots.Add(i);
                    clientIdToSpawnSlot[clientId] = i;
                    return i;
                }
            }
            // no spawn slots available
            return -1;
        }

        /// <summary>
        /// used for spawning units for a player
        /// </summary>
        /// <param name="spawnSlot"></param>
        /// <param name="radius"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private Vector3 GetSpawnPositionWithOffset(int spawnSlot, float radius, Vector3 offset)
        {
            return Vector3.zero;
        }
    }
}
