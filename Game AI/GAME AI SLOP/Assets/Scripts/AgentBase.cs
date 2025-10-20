using UnityEngine;

public class AgentBase : MonoBehaviour
{
    [Header("Perception")]
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float perceptionRange = 10f;
    [SerializeField]
    [Range(0, 360)] private float viewAngle = 120f;
    [SerializeField]
    private float eyeHeight = 1.0f;

    [Header("Movement")]
    [SerializeField]
    private float speed = 3f;
    [SerializeField]
    private float turnSpeed = 3f;
    [SerializeField]
    private float stopDistance = 0.1f;

    protected Vector3 lastSeenPosition;
    protected bool hasLineOfSight;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        Perceive();
        Act();
    }

    protected virtual void Perceive()
    {
        hasLineOfSight = false;
        
        if (target == null)
            return;

        Vector3 direction = target.position - (this.transform.position + Vector3.up * eyeHeight);

        float distance = direction.magnitude;

        //check whether the agent is within the line of vision range

        if (distance > perceptionRange)
            return;

        if (Vector3.Angle(this.transform.forward, direction) > (viewAngle * 0.5f))
            return;

        RaycastHit hit;
        if (Physics.Raycast(this.transform.position + Vector3.up * eyeHeight,
                           direction.normalized,
                           out hit,
                           perceptionRange))
        {
            if(hit.transform == target)
            {
                hasLineOfSight = true;
                lastSeenPosition = hit.point;
            }
        }
    }

    protected virtual void Act()
    {
        if (hasLineOfSight)
        {
            Vector3 direction = (lastSeenPosition - this.transform.position);
            direction.y = 0;    

            if(direction.magnitude > stopDistance)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

                this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                                                           targetRotation,
                                                           turnSpeed * Time.deltaTime);
                this.transform.position += this.transform.forward * speed * Time.deltaTime;
            }
        }
    }
}
