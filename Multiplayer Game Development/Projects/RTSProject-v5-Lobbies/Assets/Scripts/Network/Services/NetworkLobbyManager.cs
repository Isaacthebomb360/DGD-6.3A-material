using System;
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
        public List<Lobby> Lobbies { get; private set; }
        public Lobby ActiveLobby { get; private set; }

        private ILobbyEvents _lobbyEvents;
        
        // public static event Action<Player> OnPlayerJoined;
        // public static event Action<Player> OnPlayerLeft;
        // public static event Action<Player> OnPlayerReadyStateChanged;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Lobbies = new List<Lobby>();
        }

        public async Task<Lobby> CreateLobby(string lobbyName, int maxPlayers, string hostName)
        {
            try
            {
                var callbacks = new LobbyEventCallbacks();
                callbacks.LobbyChanged += OnLobbyChanged;
                callbacks.KickedFromLobby += OnKickedFromLobby;
                callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;
                
                var options = new CreateLobbyOptions();
                ActiveLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,maxPlayers, options);
                _lobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(ActiveLobby.Id, callbacks);
                
                Log(ActiveLobby);
            }
            catch (LobbyServiceException ex)
            {
                switch (ex.Reason) {
                    case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{ActiveLobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
                    case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
                    case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
                    default: throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return ActiveLobby;
        }

        private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
        {
            switch (state)
            {
                case LobbyEventConnectionState.Unsubscribed: /* Update the UI if necessary, as the subscription has been stopped. */ 
                    break;
                case LobbyEventConnectionState.Subscribing: /* Update the UI if necessary, while waiting to be subscribed. */ 
                    break;
                case LobbyEventConnectionState.Subscribed: /* Update the UI if necessary, to show subscription is working. */ 
                    break;
                case LobbyEventConnectionState.Unsynced: /* Update the UI to show connection problems. Lobby will attempt to reconnect automatically. */ 
                    break;
                case LobbyEventConnectionState.Error: /* Update the UI to show the connection has errored. Lobby will not attempt to reconnect as something has gone wrong. */
                    break;
            }
        }

        private void OnKickedFromLobby()
        {
            throw new NotImplementedException();
        }

        private void OnLobbyChanged(ILobbyChanges changes)
        {
            switch (changes)
            {
                // case ILobbyChanges.PlayerJoined:
                //     break;
            }
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


        public async Task<List<Lobby>> GetUpdatedLobbyList()
        {
            try
            {
                var lobbiesQuery = await LobbyService.Instance.QueryLobbiesAsync();
                Lobbies = lobbiesQuery.Results;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return Lobbies;
        }

        //Information related to current lobby
        public static void Log(Lobby lobby)
        {
            if (lobby == null)
            {
                Debug.Log($"No active lobby");
                return;
            }

            // var lobbyData = lobby.Data.Select(kvp => $"{kvp.Key} is {kvp.Value.Value}");
            // var lobbyDataStr = string.Join(", ", lobbyData);

            Debug.Log($"Lobby Named: {lobby.Name}, " +
                      $"Players: {lobby.Players.Count}/{lobby.MaxPlayers}, " +
                      $"IsLocked: {lobby.IsLocked}, " +
                      $"LobbyCode:  {lobby.LobbyCode}, " +
                      $"Id: {lobby.Id}, " +
                      $"Created: {lobby.Created}, " +
                      $"HostId: {lobby.HostId}, " +
                      $"EnvironmentId: {lobby.EnvironmentId}, " +
                      $"Upid: {lobby.Upid}");
        }

        //Information related to list of available lobbies
        public static void Log(List<Lobby> lobbies)
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

        private void OnDestroy()
        {
            _lobbyEvents.UnsubscribeAsync();
        }

    }
}