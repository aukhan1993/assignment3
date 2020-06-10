using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckMyVision : MonoBehaviour
{
    // How sensitive we are about vision/line of sight 
    // Start is called before the first frame update

    public enum enmSensitivity { HIGH, LOW};

    // variable to check sensitivity
    public enmSensitivity sensitivity = enmSensitivity.HIGH;

    // Are we able to see the target right now?
    public bool targetInSight = false;

    //Field of Vision
    public float fieldOfVision = 45f;

    //we need a reference to our target here as well
    private Transform target = null;

    //reference to our eyes - yet to add
    public Transform myEyes = null;

    // My transform component?
    public Transform npcTransform = null;

    //My sphere collider
    private SphereCollider sphereCollider = null;

    // Last knwon sighting of object?
    public Vector3 lastKnownSighting = Vector3.zero;

    private void Awake()
    {
        npcTransform = GetComponent<Transform>();
        sphereCollider = GetComponent<SphereCollider>();
        lastKnownSighting = npcTransform.position;
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>(); // we shall tag this later
        
    
    }

    bool InMyFieldOfVision()
    {
        Vector3 dirToTarget = target.position - myEyes.position;

        //Get angle between forward and view direction
        float angle = Vector3.Angle(myEyes.forward, dirToTarget);

        //Let us checck if within field of view

        if (angle <= fieldOfVision)
            return true;
        else
            return false;
    }

    //We need a function to check line of sight

    bool ClearLineOfSight()
    {
        RaycastHit hit;

        if (Physics.Raycast(myEyes.position, (target.position - myEyes.position).normalized, out hit, sphereCollider.radius))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;

    }


    void UpdateSight()
    {
        switch (sensitivity)
        {
            case enmSensitivity.HIGH:
                targetInSight = InMyFieldOfVision() && ClearLineOfSight();
                break;
            case enmSensitivity.LOW:
                targetInSight = InMyFieldOfVision() || ClearLineOfSight();
                break;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        UpdateSight();

        //Update last knwon sighting
        if (targetInSight)
            lastKnownSighting = target.position;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        targetInSight = false;
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
