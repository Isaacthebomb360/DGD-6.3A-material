using UnityEngine;

public class CubeController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 10f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 10f;

        var newXPos = this.transform.position.x + x;
        var newZPos = this.transform.position.z + z;

        this.transform.position = new Vector3(newXPos, 0, newZPos);


    }
}
