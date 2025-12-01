using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QFSW.QC;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Tutorials.Core.Editor;
using UnityEngine;

namespace Network.Services
{

    public partial class NetworkLobbyManager : MonoBehaviour
    {
        [Command("lobby.create", MonoTargetType.Singleton), CommandDescription("Creates a new lobby.")]
        public static async Task CreateLobbyCommand(string lobbyName = "TestLobby", int maxPlayers = 4)
        {
            var pid = AuthenticationService.Instance?.PlayerId;
            if (string.IsNullOrWhiteSpace(pid))
            {
                Debug.LogWarning($"Cannot create a lobby because the player is not signed in!");
                return; 
            }

            try
            {
                var lobby = await Instance.CreateLobby(lobbyName, maxPlayers, $"Host_{pid[..6]}");
                GUIUtility.systemCopyBuffer = lobby.Id;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to create lobby. Error: {e.Message}");
            }
        }
        [Command("lobby.list", MonoTargetType.Singleton), CommandDescription("List all available lobbies")]
        public static async Task ListLobbiesCommand()
        {
            try
            {
                List<Lobby> lobbies = await Instance.GetUpdatedLobbyList();
                Log(lobbies);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to list all available lobbies: {e.Message}");
            }
        }

        [Command("lobby.join", MonoTargetType.Singleton), CommandDescription("List all available lobbies")]
        public static async Task JoinLobbyCommand(string lobbyId)
        {
            var pid = AuthenticationService.Instance?.PlayerId;
            string playerName = AuthenticationService.Instance.Profile;
            if (string.IsNullOrWhiteSpace(pid) || string.IsNullOrWhiteSpace(playerName))
            {
                Debug.LogWarning($"Cannot join a lobby because the player is not signed in!");
                return;
            }

            try
            {
                // Step 1 Join Lobby
                var lobby = await Instance.JoinLobby(lobbyId, playerName);
                
                if (lobby == null)
                {
                    Debug.LogWarning($"Failed to join a lobby. Error: {lobbyId}");
                    return;
                }
                Debug.Log($"Joined Lobby: {lobby.Name} ({lobby.Id}) as {playerName}");
                // Step 2 Retrieve the Relay Join Code from Lobby Data
                
                // Step 3 Join in the relay session automatically
                
                // Step 4 Register for chat
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to join a lobby. Error: {e.Message}");
            }
            
        }
    }
}