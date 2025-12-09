using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkTimer_1 : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI textprint;

    NetworkVariable<float> timer = new NetworkVariable<float>( 8f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );

    private bool timerStarted = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer) timer.Value = 8f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (!timerStarted)
        {
            int playerCount = NetworkManager.Singleton != null ? NetworkManager.Singleton.ConnectedClients.Count : 0;

            if (playerCount >= 2)
            {
                timerStarted = true;
                timer.Value = 8f;
            }
            else
                return;
        }

        if (timer.Value > 0)
        {
            timer.Value -= Time.deltaTime;
            if (timer.Value <= 0) timer.Value = 0; // Timer finished!
        }
    }

    private void LateUpdate()
    {
        if (timerStarted)
            textprint.text = $"timer: {timer.Value:F2}s - started";
        else
            textprint.text = $"timer: {timer.Value:F2}s - not started";
    }
}
