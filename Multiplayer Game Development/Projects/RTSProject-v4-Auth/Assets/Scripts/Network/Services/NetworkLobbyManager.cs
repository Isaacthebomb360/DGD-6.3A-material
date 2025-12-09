using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Network.Services
{
    [DisallowMultipleComponent]
    public partial class NetworkLobbyManager : MonoBehaviour
    {
        public static NetworkLobbyManager Instance { get; private set; }

        public Lobby ActiveLobby { get; private set; }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public async Task<Lobby> CreateLobby(string lobbyName, int maxPlayers, string hostName)
        {
            var options = new CreateLobbyOptions();
            ActiveLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,maxPlayers, options);
            return ActiveLobby;
        }

        public async Task<Lobby> JoinLobby(string lobbyId, string playerName)
        {
            var options = new JoinLobbyByIdOptions();
            ActiveLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            return ActiveLobby;
        }
        
        public async Task<Lobby> JoinPrivateLobby(string lobbyJoinCode, string playerName)
        {
            var options = new JoinLobbyByCodeOptions();
            ActiveLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyJoinCode, options);
            return ActiveLobby;
        }

        private void UpdateLobby(Lobby updatedLobby)
        {
            if (ActiveLobby == null || updatedLobby == null) return;
        }

        // public async Task LeaveLobby(string lobbyId)
        // {
        //     
        // }

        // public async Task ToggleReadyState()
        // {
        //     
        // }

        //Information related to current lobby
        public static void Log(Lobby lobby)
        {
            if (lobby == null)
            {
                Debug.Log($"No active lobby");
                return;
            }

            var lobbyData = lobby.Data.Select(kvp => $"{kvp.Key} is {kvp.Value.Value}");
            var lobbyDataStr = string.Join(", ", lobbyData);

            Debug.Log($"Lobby Named: {lobby.Name}, " +
                      $"Players: {lobby.Players.Count}/{lobby.MaxPlayers}, " +
                      $"IsLocked: {lobby.IsLocked}, " +
                      $"LobbyCode:  {lobby.LobbyCode}, " +
                      $"Id: {lobby.Id}, " +
                      $"Created: {lobby.Created}, " +
                      $"HostId: {lobby.HostId}, " +
                      $"EnvironmentId: {lobby.EnvironmentId}, " +
                      $"Upid: {lobby.Upid}, " +
                      $"Lobby.Data: {lobbyDataStr}");
        }

        //Information related to list of available lobbies
        public static void Log(string message, List<Lobby> lobbies)
        {
            if (lobbies.Count == 0)
            {
                Debug.Log($"No lobbies found");
            }
            else
            {
                Debug.Log("Lobbies list:");
                foreach (var lobby in lobbies)
                {
                    Debug.Log($"  Lobby: {lobby.Name}, "+
                              $"Players: {lobby.Players.Count}/{lobby.MaxPlayers}, " +
                              $"id: {lobby.Id}");
                }
            }
        }

    }
}