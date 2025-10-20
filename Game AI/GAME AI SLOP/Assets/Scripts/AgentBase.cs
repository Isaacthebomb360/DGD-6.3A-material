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

    private Vector3 lastSeenPosition;
    private bool hasLineOfSight;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Perceive();
        Act();
    }

    protected virtual void Perceive()
    {

    }

    protected virtual void Act()
    {
        
    }
}
