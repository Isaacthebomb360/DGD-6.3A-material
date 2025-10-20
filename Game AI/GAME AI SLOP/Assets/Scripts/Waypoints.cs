using UnityEngine;

public class Waypoints : MonoBehaviour
{
    [SerializeField]
    GameObject[] waypoints;

    [SerializeField]
    int startWaypoint = 0;

    [SerializeField]
    float stopDistance = 0.8f;

    [SerializeField]
    float turnSpeed = 3f;

    [SerializeField]
    float speed = 10f;

    int currentWaypoint = 0;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waypoints = GameObject.FindGameObjectsWithTag("waypoint");

        currentWaypoint = startWaypoint;
    }

    // Update is called once per frame
    void Update()
    {
        MoveToWaypoint();
    }
    
    private void MoveToWaypoint()
    {
        if (Vector3.Distance(waypoints[currentWaypoint].transform.position,
                            this.transform.position) > stopDistance)
        {
            currentWaypoint++;

            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = 0;
            }
        }

        Vector3 direction = waypoints[currentWaypoint].transform.position -
                            this.transform.position;

        this.transform.rotation = Quaternion.Slerp(
                                    this.transform.rotation,
                                    Quaternion.LookRotation(direction),
                                    turnSpeed * Time.deltaTime);

        this.transform.Translate(0, 0, Time.deltaTime * speed);
    }
}
