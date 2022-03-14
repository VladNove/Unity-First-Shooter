using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCapsuleEnemy : MonoBehaviour
{
    Transform target;
    Rigidbody rb;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        target = GameObject.Find("Player").transform;
        rb = GetComponent<Rigidbody>();
        distToTarget = Mathf.Infinity;
    }

    // Update is called once per frame
    protected float distToTarget;

    public float targetDist;
    public float mintargetDist;
    public float moveSpeed;

    //cele 3 functii de mai jos functioneaza pe baza PID-urilor
    //TODO: de comentat
    protected void MoveToTarget()
    {
        if (Detected() && SafeToMove())
        {
            distToTarget = Vector3.Distance(transform.position, target.position);
            //Debug.Log(distToTarget);
            Vector3 targetPos = Vector3.Scale((transform.position - target.position).normalized, new Vector3(1, 0, 1));
            if (distToTarget > targetDist + 0.1f)
                targetPos *= targetDist;
            else if (distToTarget < mintargetDist - 0.1f)
                targetPos *= mintargetDist;
            else
                targetPos *= 0;

            if (targetPos != Vector3.zero)
            {
                targetPos += target.position;
                float upright = Mathf.Clamp01(Vector3.Dot(Vector3.up, transform.up));
                rb.MovePosition(Vector3.MoveTowards(rb.position, targetPos, Time.fixedDeltaTime * moveSpeed * upright));
            }
        }
    }

    public float rotationForce = 0.005f;
    public float rotationDampner = 0.005f;
    protected void RotateToTarget()
    {
        float angleToTarget = Vector3.SignedAngle(transform.forward, target.position - transform.position, transform.up);
        float PDvalue = rotationForce * angleToTarget - rotationDampner * rb.angularVelocity.y;
        rb.AddRelativeTorque(Vector3.up * PDvalue);
    }

    public float upRightStrenght = 1f;
    public float upRightDampner = 0.2f;
    protected void KeepUpright()
    {
        Vector3 uprightDif = Vector3.up - transform.up;
        uprightDif = Vector3.ProjectOnPlane(uprightDif, transform.up);
        Vector3 PDValue = uprightDif * upRightStrenght -
            rb.GetRelativePointVelocity(Vector3.up) * upRightDampner;


        Debug.DrawRay(transform.position + transform.up,
            uprightDif * upRightStrenght, Color.blue);
        Debug.DrawRay(transform.position + transform.up,
           -rb.GetRelativePointVelocity(Vector3.up) * upRightDampner, Color.red);


        rb.AddForceAtPosition(PDValue, transform.position + transform.up);
    }


    //testam daca playerul este vizibil
    protected bool Detected()
    {
        if (Physics.Raycast(transform.position, target.position - transform.position, out RaycastHit hit))
        {
            if (hit.transform.gameObject.name.Equals("Player"))
                return true;

        }
        return false;
    }

    //functia testeaza daca exista teren in fata, ca gen... sa nu cada in abis
    protected bool SafeToMove()
    {
        return Physics.Raycast(transform.position + transform.forward, Vector3.down, 3f);
    }
}