using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Network.Services
{
    public class NetworkConnectionSetup : MonoBehaviour
    {
        [Header("Default Connection Settings")]
        [SerializeField] private string defaultIp = "127.0.0.1";
        [SerializeField] private int defaultPort = 7777;

        private UnityTransport _transport;
        public static NetworkConnectionSetup Instance;

        // Singleton setup
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Subscribing to Client connected and Client disconnected events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribing to Client connected and Client disconnected events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                // HOST
                int totalConnectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;
                Debug.Log($"New player joined! Client ID: {clientId}. Total connected players: {totalConnectedPlayers}");
            }
            else if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                // CLIENT WHO JOINED
                Debug.Log($"Successfully connected to host! Your Client ID: {clientId}");
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                // HOST
                int totalConnectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;
                Debug.Log($"Player disconnected! Client ID: {clientId}. Total connected players: {totalConnectedPlayers}");
            }
            else if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                // CLIENT WHO DISCONNECTED
                Debug.LogWarning("Connection to host failed or disconnected.");
            }
        }

        // -----------------------
        // BUTTON-FRIENDLY METHODS
        // -----------------------

        /// <summary>
        /// Call this from a UI Button to start the host using the defaultPort.
        /// </summary>
        public void StartHostButton()
        {
            StartHost(defaultPort);
        }

        /// <summary>
        /// Call this from a UI Button to join using defaultIp and defaultPort.
        /// </summary>
        public void JoinGameButton()
        {
            JoinGame(defaultIp, defaultPort);
        }

        /// <summary>
        /// Call this from a UI Button to disconnect.
        /// </summary>
        public void DisconnectButton()
        {
            Disconnect();
        }

        // -----------------------
        // CORE NETWORK METHODS
        // -----------------------

        /// <summary>
        /// Starts the game as host on the given port.
        /// You can also call this from other scripts.
        /// </summary>
        public void StartHost(int port = 7777)
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("No NetworkManager instance found. Cannot start host.");
                return;
            }

            _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (_transport != null)
            {
                // Clamp port to valid range and cast to ushort
                port = Mathf.Clamp(port, 0, 65535);
                ushort uPort = (ushort)port;

                _transport.SetConnectionData("0.0.0.0", uPort);
                NetworkManager.Singleton.StartHost();
                Debug.Log($"Host started on port: {uPort}");
            }
            else
            {
                Debug.LogError("UnityTransport component not found on NetworkManager.");
            }
        }

        /// <summary>
        /// Stops the game and disconnects all players.
        /// Button-friendly as-is.
        /// </summary>
        public void StopHost()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("No NetworkManager instance found. Cannot stop host.");
                return;
            }

            NetworkManager.Singleton.Shutdown();
            Debug.Log("Host stopped. All players disconnected.");
        }

        /// <summary>
        /// Joins the game as a client.
        /// </summary>
        public void JoinGame(string ip = "127.0.0.1", int port = 7777)
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("No NetworkManager instance found. Cannot join game.");
                return;
            }

            _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (_transport != null)
            {
                port = Mathf.Clamp(port, 0, 65535);
                ushort uPort = (ushort)port;

                _transport.SetConnectionData(ip, uPort);
                NetworkManager.Singleton.StartClient();
                Debug.Log($"Connecting to {ip}:{uPort}");
            }
            else
            {
                Debug.LogError("UnityTransport component not found on NetworkManager.");
            }
        }

        /// <summary>
        /// Disconnects from the game (host or client).
        /// </summary>
        public void Disconnect()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("No NetworkManager instance found. Cannot disconnect.");
                return;
            }

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Not currently connected to a host!");
                return;
            }

            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("Disconnecting as host. All players will be disconnected.");
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("Disconnecting from game.");
            }

            NetworkManager.Singleton.Shutdown();
        }
    }
}
