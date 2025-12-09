using UnityEngine;
using Unity.Netcode;

namespace Network
{
    /// <summary>
    /// Synchronizes player material across all clients
    /// </summary>
    public class NetworkPlayerMaterial : NetworkBehaviour
    {
        private NetworkVariable<int> materialIndex = new NetworkVariable<int>(
            0, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server);

        private Renderer[] renderers;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Subscribe to material index changes
            materialIndex.OnValueChanged += OnMaterialIndexChanged;
            
            // Apply the current material immediately (for all clients including late joiners)
            ApplyMaterialFromIndex(materialIndex.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            materialIndex.OnValueChanged -= OnMaterialIndexChanged;
        }

        /// <summary>
        /// Server-only: Set the material index for this player
        /// </summary>
        public void SetMaterialIndex(int index, Material material)
        {
            if (!IsServer) return;
            
            materialIndex.Value = index;
            ApplyMaterial(material);
        }

        private void OnMaterialIndexChanged(int previousValue, int newValue)
        {
            // This is called on clients when the server changes the value
            if (!IsServer)
            {
                ApplyMaterialFromIndex(newValue);
            }
        }

        private void ApplyMaterialFromIndex(int index)
        {
            // Request the material from the spawner
            if (NetworkPlayerSpawner.Instance != null)
            {
                Material mat = NetworkPlayerSpawner.Instance.GetMaterialByIndex(index);
                if (mat != null && renderers != null && renderers.Length > 0)
                {
                    ApplyMaterial(mat);
                }
                else
                {
                    Debug.LogWarning($"[Client {NetworkManager.Singleton.LocalClientId}] Failed to apply material index {index}. Mat null: {mat == null}, Renderers: {renderers?.Length ?? 0}");
                }
            }
            else
            {
                Debug.LogWarning($"[Client {NetworkManager.Singleton.LocalClientId}] NetworkPlayerSpawner.Instance is null when trying to apply material {index}");
            }
        }

        private void ApplyMaterial(Material material)
        {
            if (renderers == null || renderers.Length == 0) return;
            
            foreach (var r in renderers)
            {
                if (r != null)
                {
                    r.material = material;
                }
            }
        }
    }
}
