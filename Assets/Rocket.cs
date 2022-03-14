using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rb;
    public float startVelocity;
    

    public float explosionTimer = 1f;
    public float armingTime = 0.1f;
    
    public GameObject explosion;
    EffectManager effectManager;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * startVelocity;
        effectManager = GameObject.FindObjectOfType<EffectManager>();
    }

    bool exploding;
    void FixedUpdate()
    {
        explosionTimer -= Time.fixedDeltaTime;
        if (explosionTimer < 0)
            exploding = true;
        if (exploding)
        {
            effectManager.ExplosionDebris(gameObject.transform.position);
            Instantiate(explosion, gameObject.transform.position , Quaternion.identity);
            Destroy(gameObject);
        }       
        armingTime -= Time.fixedDeltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (armingTime < 0)
                exploding = true;
    }

    

    public Dmg Redirect()
    {
        armingTime = 0f;
        explosionTimer = 5f;
        Dmg[] damagable = GameObject.FindObjectsOfType<Dmg>();
        List<Dmg> damagableList = new List<Dmg>();
        foreach (Dmg element in damagable)
            if (element.tag != "Projectile")
                damagableList.Add(element);

        Dmg secondClosest = null;
        Dmg closest = null;
        Dmg found;
        while (damagableList.Count != 0)
        {
            found = null;
            float closestDistance = Mathf.Infinity;
            
            foreach (Dmg candidate in damagableList) {
                float dist = Vector3.Distance(gameObject.transform.position, candidate.transform.position);
                if (dist < closestDistance) {
                    closestDistance = dist;
                    found = candidate;    
                }
            }

            if (found == null)
                break;

            RaycastHit hit;
            Vector3 origin = rb.transform.position;  Vector3 dest = found.transform.position;
            if (Physics.Raycast(origin, dest - origin, out hit, (dest - origin).magnitude))
                if (hit.transform.position == found.transform.position && closest == null)
                {
                    closest = found;
                }
                else if (hit.transform.position == found.transform.position && secondClosest == null)
                {
                    secondClosest = found;
                    break;
                }


            damagableList.Remove(found);
            if (damagableList.Count == 0) {
                break;
            }
        }

        print(closest.transform.name);

        if (closest == null)
                exploding = true;
        else
            rb.velocity = (closest.transform.position - rb.position).normalized * startVelocity;

        GameObject.FindObjectOfType<EffectManager>().debug = closest.transform.position;
        GameObject.FindObjectOfType<EffectManager>().rayo = rb.position;
        GameObject.FindObjectOfType<EffectManager>().rayd = (closest.transform.position - rb.position).normalized * startVelocity;
        
        return secondClosest;
    }
}
