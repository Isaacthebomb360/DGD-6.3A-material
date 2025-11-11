using UnityEngine;

public class NetworkSpawner : MonoBehaviour
{
    /// <summary>
    /// Call this from a UI Button and pass a string like "0,1,2".
    /// </summary>
    public void SpawnCubeFromString(string position)
    {
        if (string.IsNullOrWhiteSpace(position))
        {
            Debug.LogWarning("SpawnCubeFromString called with empty position string.");
            return;
        }

        string[] coordinates = position.Split(',');
        if (coordinates.Length != 3)
        {
            Debug.LogWarning($"SpawnCubeFromString: '{position}' is invalid. Expected format: 'x,y,z'");
            return;
        }

        if (float.TryParse(coordinates[0], out float x) &&
            float.TryParse(coordinates[1], out float y) &&
            float.TryParse(coordinates[2], out float z))
        {
            SpawnCube(new Vector3(x, y, z));
        }
        else
        {
            Debug.LogWarning($"SpawnCubeFromString: could not parse '{position}'. Expected format: 'x,y,z'");
        }
    }

    /// <summary>
    /// Simple button-friendly method to spawn a cube at the origin.
    /// </summary>
    public void SpawnCubeAtOrigin()
    {
        SpawnCube(Vector3.zero);
    }

    /// <summary>
    /// Core spawn function – can be called from other scripts.
    /// </summary>
    public void SpawnCube(Vector3 pos)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = pos;
        cube.AddComponent<Rigidbody>();
    }
}
