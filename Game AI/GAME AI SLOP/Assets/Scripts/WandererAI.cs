using UnityEngine;
using UnityEngine.Rendering;

public class WandererAI : MonoBehaviour
{
    [SerializeField]
    private float speed = 3f;


    [SerializeField]
    private float turnSpeed = 2f;

    [SerializeField]
    private float wanderRadius = 15f; // area within the npc will wander around

    [SerializeField]
    private float goalDistance = 0.5f;

    private Vector3 goalPosition;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateNewGoalPosition();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, goalPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, goalPosition) < goalDistance)
        {
            GenerateNewGoalPosition();
        }

        Vector3 lookDirection = (goalPosition - transform.position).normalized;

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                                                   Quaternion.LookRotation(lookDirection),
                                                   turnSpeed * Time.deltaTime);

        Debug.DrawRay(this.transform.position, lookDirection, Color.red);

    }

    void GenerateNewGoalPosition()
    {
        goalPosition = transform.position + Random.insideUnitSphere * wanderRadius;
        goalPosition.y = this.transform.position.y;  
    }
}
