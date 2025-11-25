using QFSW.QC;
using UnityEngine;

public class NetworkSpawner : MonoBehaviour
{
    [Command("spawn.cube")]
    public void SpawnCubeCommand(string position)
    {
        string[] coordinates = position.Split(',');
        if (coordinates.Length != 3) return;
        
        float x,  y, z;
        if (float.TryParse(coordinates[0], out x) && float.TryParse(coordinates[1], out y) &&
            float.TryParse(coordinates[2], out z))
        {
            SpawnCube(new Vector3(x, y, z));
        }
    }
    public void SpawnCube(Vector3 pos)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = pos;
        cube.AddComponent<Rigidbody>();
    }
}
