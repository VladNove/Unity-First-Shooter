using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleHook : MonoBehaviour
{
    public bool Hooked;
    SpringJoint sj;
    float defSpring;
    float defDampner;
    public float retractSpeed;

    void Start()
    {
        sj = GetComponentInParent<SpringJoint>();
        defSpring = sj.spring;
        defDampner = sj.damper;
        sj.spring = 0;
        sj.damper = 0;
        sj.autoConfigureConnectedAnchor = false;
    }

    // Update is called once per frame

    private void Update()
    {
        if (Hooked)
        {
            var lr = GetComponent<LineRenderer>();
            lr.SetPosition(0, transform.position - Vector3.up);

            //acel connected anchor reprezinta pozitia relativa la corpul conectat
            //de aceea daca avem vreun corp conectat tre sa transformam pozitia relativa in globala
            //ca sa putem folosi line rendererul
            if (!sj.connectedBody)
                lr.SetPosition(1, sj.connectedAnchor);
            else
                lr.SetPosition(1, sj.connectedBody.transform.TransformPoint(sj.connectedAnchor));
        }
    }

    void FixedUpdate()
    {
        //scadem lungimea "sforii"
        if (Hooked)
        {
            sj.maxDistance = Mathf.MoveTowards(sj.maxDistance, sj.minDistance, Time.fixedDeltaTime*retractSpeed);
            
        }
        if (Input.GetKey(KeyCode.Q) && !Hooked)
            LaunchHook();
        else if (Hooked && !Input.GetKey(KeyCode.Q))
            RetractHook();


    }

    void LaunchHook()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 100f))
        {
            Hooked = true;
            sj.connectedAnchor = hit.point;
            //setam corpul la care este conectat si pozitia "conexiunii" relativa la acelcorp
            if (hit.rigidbody)
            { 
                sj.connectedBody = hit.rigidbody;
                sj.connectedAnchor = hit.transform.InverseTransformPoint(hit.point);
            }
            else
            {
                sj.connectedBody = null;
            }
            
            sj.maxDistance = (hit.point - transform.position).magnitude + 1;
            sj.spring = defSpring;
            sj.damper = defDampner;
        }
    }

    void RetractHook()
    {
        Hooked = false;
        //sj.connectedAnchor = hit.point;
        sj.spring = 0;
        sj.damper = 0;
    }
}
