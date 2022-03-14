using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneEnemy : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public Transform target;
    public float sprayLenght;
    public float sprayTime;
    public float sprayFireRate;
    float fireRate;
    float sprayVal;
    bool shooting;
    Vector3 sprayBegin;
    Vector3 sprayEnd;
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            shooting = true;
            sprayVal = 0;
            Vector3 targetPlanePos = new Vector3(transform.position.x, target.position.y, transform.position.z);
            sprayBegin = (targetPlanePos - target.position).normalized * sprayLenght + target.position;
            
            sprayEnd = 2 * target.position - sprayBegin;
            fireRate = 0;
        }
        Debug.DrawRay(sprayBegin, Vector3.up, Color.red);
        if (shooting)
            Shooting();
    }

    void Shooting()
    {
        sprayVal = Mathf.MoveTowards(sprayVal, 1, Time.fixedDeltaTime / sprayTime);

        if (fireRate <= 0)
        {
            fireRate = sprayFireRate;
            Vector3 shootPoint = Vector3.Lerp(sprayBegin, sprayEnd, sprayVal);
            if (Physics.Raycast(transform.position, shootPoint - transform.position, out RaycastHit hit))
            {
                GameObject.FindObjectOfType<EffectManager>().ShootEff(transform.position, hit.point, hit.normal);
            }
        }
        fireRate -= Time.fixedDeltaTime;

        
        if (sprayVal == 1)
            shooting = false;
    }

}
