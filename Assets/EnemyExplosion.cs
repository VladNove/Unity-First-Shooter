using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyExplosion : MonoBehaviour
{
    public float explosionRadius;
    public float explosionForce;

    private void Start()
    {
        GetComponent<SphereCollider>().radius = 0;

    }

    private void FixedUpdate()
    {
        Exploding();
    }

    void Exploding()
    {
        //gameObject.GetComponent<Dmg>().enabled = false;

        //TODO: un refactor

        //rb.velocity = Vector3.zero;

        GetComponent<MeshRenderer>().enabled = true;
        if (transform.localScale.y < explosionRadius)
        {
            transform.localScale += Vector3.one * Time.fixedDeltaTime * 10;
            GetComponent<SphereCollider>().radius = 0.5f;
            // de schimbat transparenta sferei 

            var mat = GetComponent<MeshRenderer>().material;
            float transparency = mat.GetFloat(
                "Vector1_56dab2fbc6ed4d759135a10c9ada3163");
            // Debug.Log(transparency);
            if (transform.localScale.y >= explosionRadius / 2)
                mat.SetFloat(
                    "Vector1_56dab2fbc6ed4d759135a10c9ada3163",
                    Mathf.MoveTowards(transparency, 0, Time.fixedDeltaTime * 2));
            else
                mat.SetFloat(
                    "Vector1_56dab2fbc6ed4d759135a10c9ada3163", 1);
        }
        else Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        var damage = other.gameObject.GetComponent<Dmg>();
        //print(damage);
        //print(damage.transform.name);
        if (damage != null)
        {
            damage.Damage(50);
        }

        if (other.attachedRigidbody)
        {
            if (other.attachedRigidbody.gameObject.name.Equals("Player"))
            {
                other.attachedRigidbody.AddExplosionForce(
                explosionForce * 2 * 4, transform.position, explosionRadius);
                GameObject.FindObjectOfType<EffectManager>().DarkenScreen();

            }
            else
            {
                other.attachedRigidbody.AddExplosionForce(
                explosionForce, transform.position, explosionRadius);

            }
        }
    }
}
