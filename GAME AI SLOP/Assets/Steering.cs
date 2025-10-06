using UnityEngine;

public class Steering : MonoBehaviour
{

    [SerializeField]
    private GameObject goal;

    [SerializeField]
    private float npcSpeed = 10f;

    [SerializeField]
    private float stopDistance = 0.1f;

    [SerializeField]
    private float rotationSpeed = 1.5f;

    [SerializeField]
    Vector3 direction = new Vector3(0, 0, 0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 goalRepositioned = new Vector3(goal.transform.position.x,
                                             this.transform.position.y,
                                             goal.transform.position.z);

        direction = goalRepositioned - this.transform.position;

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                                                   Quaternion.LookRotation(direction),
                                                   rotationSpeed * Time.deltaTime);

        Debug.DrawRay(this.transform.position, direction, Color.red);


        if (direction.magnitude > stopDistance)
        {
            this.transform.Translate(direction.normalized * npcSpeed * Time.deltaTime, Space.World);
        }
    }
}
