using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee1 : PhysicsCapsuleEnemy
{


    public ParticleSystem weaponEffect;
    public Transform weapon;

    public float AttackRange;
    public float AttackCharge;
    float _attackCharge;
    public float AttackSpeed;
    float _attackSpeed;
    public float AttackCooldown;
    float _attackCooldown;
    bool attacking;
    bool _appliedDamage;

    protected override void Start()
    {
        base.Start();
        weaponEffect.Stop();
        
    }
    private void Update()
    {
        //TODO: un refactor

        //daca atacam
        if (attacking && GetComponent<Dmg>().hp > 0) {
            
            //practic folosesc _attackspeed si ca sa controlez animatia, atunci cand e pozitiv are loc "lovitura"
            //cand e negativ sabia se intoarce la pozitia initiala
            if (_attackSpeed < - AttackSpeed)
                attacking = false;

            if (_attackSpeed >=0 )
            {
                //rotesc sabia in fata
                weapon.localRotation = Quaternion.Euler(Mathf.LerpAngle(90, -30, _attackSpeed / AttackSpeed),
                    Mathf.LerpAngle(-15, 0, _attackSpeed / AttackSpeed), 0);
                
                //raycast in fata, in 3 diriectii
                //totusi nu dau raycast chiar de la inceputul atacului sa aiba timp playerul sa dea dodge
                if (!_appliedDamage && _attackSpeed < AttackSpeed/2)
                {
                    AttackInDir(transform.forward);
                    AttackInDir(transform.forward + transform.up);
                    AttackInDir(transform.forward + transform.up / 2);
                }
            }
            else
            {
               // Debug.Log(_attackSpeed);
                weaponEffect.Stop();

                //rotim sabia inapoi
                weapon.localRotation = Quaternion.Euler(Mathf.LerpAngle(90, 0, -_attackSpeed / AttackSpeed),
                    Mathf.LerpAngle(-15, 0, -_attackSpeed / AttackSpeed), 0);
            }
            _attackSpeed -= Time.deltaTime;
        }
        else {
            //Debug.Log(distToTarget);

            //daca tinta e aproape si a trecut cooldown-ul
            if (distToTarget < AttackRange && _attackCooldown < 0)
            {

                //incepem sa dam charge la atac
                _attackCharge -= Time.deltaTime;

                //rotim sabia un pic inapoi
                weapon.localRotation = Quaternion.Euler(Mathf.LerpAngle(-30, 0, _attackCharge / AttackCharge), 0, 0);

                //odata ce s-a terminat timpul de charge, atacam
                if (_attackCharge < 0)
                {
                    attacking = true;
                    _appliedDamage = false;
                    _attackCharge = AttackCharge;
                    _attackCooldown = AttackCooldown;
                    _attackSpeed = AttackSpeed;
                    weaponEffect.Play();
                }
            }
            else
            {
                _attackCharge = AttackCharge;

            }
            _attackCooldown -= Time.deltaTime;
        }
    }
    void FixedUpdate()
    {
        if (GetComponent<Dmg>().hp < 0)
        {
            upRightStrenght = 0;
            moveSpeed = 0;
            rotationForce = 0;
        }
        
        //Debug.Log(distToTarget);
        KeepUpright();
        MoveToTarget();
        RotateToTarget();

        

    }

    public float knockBackForce;
    void AttackInDir(Vector3 dir)
    {
        Debug.DrawRay(transform.position, dir.normalized * AttackRange, Color.red);
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, AttackRange) && !_appliedDamage)
        {
            //Debug.Log(hit.collider.gameObject.name);
            if (hit.collider.attachedRigidbody)
            if (hit.collider.attachedRigidbody.name.Equals("Player"))
            {
                hit.collider.attachedRigidbody.AddExplosionForce(
                    knockBackForce * 2 * 4, transform.position, 0);
                GameObject.FindObjectOfType<EffectManager>().DarkenScreen();
                _appliedDamage = true;
            }
        }

    }

    
}
