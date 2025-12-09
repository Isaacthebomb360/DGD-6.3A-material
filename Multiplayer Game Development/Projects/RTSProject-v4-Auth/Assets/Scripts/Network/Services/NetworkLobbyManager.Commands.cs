using System;
using System.Threading.Tasks;
using QFSW.QC;
using Unity.Services.Authentication;
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
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to create lobby. Error: {e.Message}");
            }
        }
    }
}