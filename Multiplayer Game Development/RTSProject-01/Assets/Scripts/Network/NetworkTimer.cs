using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetworkTimer : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private NetworkVariable<float> timerValue = new NetworkVariable<float>(
        8f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool timerStarted = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            timerValue.Value = 8f;
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (!timerStarted)
        {
            int playerCount = NetworkManager.Singleton != null ? NetworkManager.Singleton.ConnectedClients.Count : 0;
            if (playerCount >= 2)
            {
                timerStarted = true;
                timerValue.Value = 8f;
            }
            else
            {
                return;
            }
        }

        if (timerValue.Value > 0f)
        {
            timerValue.Value -= Time.deltaTime;
            if (timerValue.Value <= 0f)
            {
                timerValue.Value = 0f;
            }
        }
    }

    private void LateUpdate()
    {
        if (timerText != null)
        {
            timerText.text = $"{timerValue.Value:F2}s";
        }
    }
}