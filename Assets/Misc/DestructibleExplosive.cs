using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleExplosive : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject explosion;
    public float explosionRadius;
    EffectManager effectManager;

    private void Start()
    {
        effectManager = GameObject.FindObjectOfType<EffectManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //if(!exploded)
        if (GetComponent<Dmg>().hp<0)
        {
            effectManager.ExplosionDebris(gameObject.transform.position);
            GameObject newExplosion = Instantiate(explosion, gameObject.transform.position, Quaternion.identity);
            GetComponent<Dmg>().enabled = false;
            newExplosion.GetComponent<EnemyExplosion>().explosionRadius = explosionRadius;
            Destroy(gameObject);
        }
    }
}
