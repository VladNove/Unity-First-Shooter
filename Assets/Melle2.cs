using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melle2 : PhysicsCapsuleEnemy
{
    public Dmg Bomb;
    public GameObject BombObj;
    // Update is called once per frame
    public float detonatingDistance;
    public float detonatingSpeed;
    bool detonate;
    void FixedUpdate()
    {
        if (GetComponent<Dmg>().hp < 0)
        {
            upRightStrenght = 0;
            moveSpeed = 0;
            rotationForce = 0;
        }

        //se detoneaza cand bomba ramane fara hp, ii dam damage ca sa o facem sa explodeze
        if ((distToTarget < detonatingDistance || detonate) && BombObj != null)
            Bomb.Damage(Time.fixedDeltaTime * detonatingSpeed);
        //Debug.Log(distToTarget);

        KeepUpright();
        MoveToTarget();
        RotateToTarget();



    }

    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log(collision.relativeVelocity);

        //pratcic masuram puterea cu care playerul "loveste" inamicul
        //daca impactul e destul de puternic, detonam inamicul
        ContactPoint col = collision.GetContact(collision.contactCount - 1);
        float impact = Vector3.Dot(col.normal, collision.relativeVelocity);
        Debug.Log(collision.gameObject.name + impact);
        if (impact > 2 && collision.gameObject.name.Equals("Player"))
        {
            detonate = true;

            //mai adaugam un pic de timp inainte de explozie
            //ca jucatorul sa aiba timp sa se departeze
            if (BombObj)
                Bomb.hp = 20;

        }
        //Debug.Log(hp);

    }
}
