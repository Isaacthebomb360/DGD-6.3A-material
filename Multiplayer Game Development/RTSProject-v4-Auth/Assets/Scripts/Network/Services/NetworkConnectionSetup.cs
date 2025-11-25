using System;
using QFSW.QC;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Network.Services
{
    public class NetworkConnectionSetup : MonoBehaviour
    {
        private UnityTransport _transport;
        public static NetworkConnectionSetup Instance;
        
        //Singleton setup
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            //Subscribing to Client connected and Client disconnected events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        private void OnDestroy()
        {
            //Unsubscribing to Client connected and Client disconnected events
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
                //HOST
                int totalConnectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;
                Debug.Log($"New player joined! Client ID: {clientId}. Total connected players: {totalConnectedPlayers}");
            }
            else if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                //CLIENT WHO JOINED
                Debug.Log($"Successfully connected to host! Your Client ID: {clientId}");
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                //HOST
                int totalConnectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;
                Debug.Log($"Player disconnected! Client ID: {clientId}. Total connected players: {totalConnectedPlayers}");
            }
            else if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                //CLIENT WHO DISCONNECTED
                Debug.LogWarning($"Connection to host failed or disconnected.");
            }
        }

        [Command("host.start","Starts the game as host", MonoTargetType.Singleton)]
        public void StartHost(ushort port = 7777)
        {
            _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (_transport != null)
            {
                _transport.SetConnectionData("0.0.0.0", port);
                NetworkManager.Singleton.StartHost();
                Debug.Log($"Host started on port: {port}");
            }
        }
        
        [Command("host.stop","Stops the game and disconnects all players", MonoTargetType.Singleton)]
        public void StopHost()
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log($"Host stopped. All players disconnected.");
        }
        
        [Command("join","Joins the game as a client", MonoTargetType.Singleton)]
        public void JoinGame(string ip = "127.0.0.1", ushort port = 7777)
        {
            _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (_transport != null)
            {
                _transport.SetConnectionData(ip, port);
                NetworkManager.Singleton.StartClient();
                Debug.Log($"Connecting to {ip}:{port}");
            }
        }

        [Command("disconnect","Disconnects the game", MonoTargetType.Singleton)]
        public void Disconnect()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError($"No NetworkManager instance found. Disabling.");
                return;
            }

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Not currently connected to the host!");
                return;
            }

            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("Disconnecting as host. All players will be disconnected.");
            }else if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("Disconnecting from game.");
            }
            NetworkManager.Singleton.Shutdown();
        }
    }
}