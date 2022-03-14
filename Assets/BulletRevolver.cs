using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Am nevoie pentru Ienumeratoare / timer pentru reload;


public class BulletRevolver : Weapon
{

    public Ammocount ammoCounter;

    public float timeBetweenRicos;
    public float ricosCooldown;

    EffectManager effectManager;
    public Transform gunModel;
    public GameObject flashPointOutside;
    public GameObject flashPointInside;
    public GameObject barrelPoint;

    protected override void Start()
    {
        base.Start();

        effectManager = GameObject.FindObjectOfType<EffectManager>();
        reloadTime = 1f;
        maxAmmo = 6;
        damage = 10f;
        range = 100f;
        FireDelay = 0.5f;
        NextFire = 0f;
    }

    protected override void FixedUpdate()
    {

        //OUTDATED
        // lerp-ul ala turbat practic mapeaza valoarea cooldown-ului (cuprinsa intre 0 si cooldown-ul initial) intre culorile rosu si galben
        // (gen daca e la 1/2 din cooldown va fi portocaliu)
        // effectManager.HighLightColor(Color.Lerp(Color.red, Color.yellow, Mathf.InverseLerp(0, specialCooldownInitial, specialCooldown)));


        //nou
        
        effectManager.HighLightFill(Mathf.InverseLerp(0, specialCooldownInitial, specialCooldown));

        if (!isReloading && reloadInput)
            gunModel.Rotate(+35, 0, 0);

        base.FixedUpdate();

        //intoarcem pistolul la rotatia originala
        gunModel.localRotation = Quaternion.Euler(
            Mathf.MoveTowardsAngle(gunModel.localRotation.eulerAngles.x, 270, Time.deltaTime * 53 / FireDelay), 180, 0
            );
    }

    protected override IEnumerator Reload()
    {

        isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);

        CurrentAmmo = maxAmmo;
        ammoCounter.ChangeAmmo(maxAmmo);

        yield return new WaitForSeconds(0.3f);

        isReloading = false;
        gunModel.Rotate(0, 0, 180);

    }

    protected override void Special()
    {
        if (specialMode == 1)
        {
            StartCoroutine(Ricoshot());
            specialCooldown = ricosCooldown;
            specialCooldownInitial = ricosCooldown;
        }
           
        CurrentAmmo = maxAmmo;
        ammoCounter.ChangeAmmo(maxAmmo);

        NextFire = Time.time + FireDelay;
        leftClickInput = false;
        specialInput = false;
        gunModel.Rotate(53, 0, 0);
    }

    IEnumerator Ricoshot()
    {
        effectManager.MuzzleFlash(barrelPoint.transform.position, flashPointInside.transform.position, flashPointOutside.transform.position);
        RaycastHit firstHit;
        if (Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, out firstHit, range))
        {
            effectManager.ShootEff(transform.Find("BarrelPoint").position, firstHit.point, firstHit.normal);
            Dmg target = firstHit.transform.GetComponent<Dmg>();
            if (target)
            {
                target.Damage(damage);
                if (target.tag == "Projectile")
                    firstHit.transform.GetComponent<Rocket>().Redirect();


                Dmg[] damagable = GameObject.FindObjectsOfType<Dmg>();
                List<Dmg> damagableList = new List<Dmg>();
                foreach (Dmg element in damagable)
                    damagableList.Add(element);

                damagableList.Remove(target);

                List<Dmg> aux = new List<Dmg>();

                Vector3 origin = firstHit.transform.position;
                Vector3 dest = origin;

                Dmg found;
                int iterations = 0;
                while (iterations < damagableList.Count)
                {

                    float closestDistance = Mathf.Infinity;
                    found = null;
                    foreach (Dmg candidate in damagableList)
                    {

                        if (candidate == null)
                            continue;


                        float dist = Vector3.Distance(origin, candidate.transform.position);
                        if (dist < closestDistance)
                        {
                            closestDistance = dist;
                            found = candidate;
                            dest = found.transform.position;
                        }
                    }

                    if (found == null || origin == dest)
                        break;

                    RaycastHit hit;
                    if (Physics.Raycast(origin, dest - origin, out hit, (dest - origin).magnitude))
                    {
                        //atentie periculos
                        if (hit.transform.position == dest)
                        {
                            effectManager.ShootEff(origin, dest, hit.normal, Color.red * 16);
                            found.Damage(damage * 2);

                            if (found.tag == "Projectile")
                                hit.transform.GetComponent<Rocket>().Redirect();

                            origin = dest;
                            damagableList.Remove(found);
                            iterations = 0;
                            foreach (Dmg element in aux)
                                damagableList.Add(element);
                            aux.Clear();
                            yield return new WaitForSeconds(timeBetweenRicos);
                        }
                        else
                        {
                            iterations++;
                            aux.Add(found);
                            damagableList.Remove(found);
                        }
                    }
                    else
                    {
                        iterations++;
                        aux.Add(found);
                        damagableList.Remove(found);
                    }
                }
            }
        }
        else
            effectManager.ShootEff(transform.Find("BarrelPoint").position,
                transform.position + MainCamera.transform.forward * 100, Vector3.zero);
    }



    public float knockBackForce;
    protected override IEnumerator Shoot()
    {
        leftClickInput = false;
        NextFire = Time.time + FireDelay;
        CurrentAmmo--;

        ammoCounter.ChangeAmmo(CurrentAmmo);

        effectManager.MuzzleFlash(barrelPoint.transform.position, flashPointInside.transform.position, flashPointOutside.transform.position);

        RaycastHit hit;
        if (Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, out hit, range))
        {
            //recul pt animatie
            effectManager.ShootEff(transform.Find("BarrelPoint").position, hit.point, hit.normal);
            //Debug.Log(hit.transform.name);

            //daca obiectul nimerit are rigidbody, atunci adaugam asupra lui o forta in pozitia, si in directia in care a fost nimerit
            //obiectul tre sa nu fie proiectil fiindca altfel o sa ii dea throw off la aim-ul proiectilului, stricand ricoseul
            if (hit.collider.attachedRigidbody && hit.transform.gameObject.transform.tag != "Projectile")
                hit.collider.attachedRigidbody.AddForceAtPosition(-hit.normal * knockBackForce, hit.point, ForceMode.Impulse);

            Dmg target = hit.transform.GetComponent<Dmg>();
            if (target)
            {

                target.Damage(damage);


                if (target.tag == "Projectile")
                {
                    //redirect-ul de la Rocket in primul rand redirecteaza racheta in directia celui mai apropiat inamic
                    //in al doilea rand returneaza al doilea cel mai apropiat obiect pt ca glontele sa ricoseze in el

                    //idee: poate daca devine prea op redirectarea asta am putea face racheta sa fie invulnerabila daca o nimeresti fix din fata
                    //(gen blindata cv), si sa trebuiasca sa o nimeresti din lateral sa o redirectezi
                    Dmg secondClosest = hit.transform.GetComponent<Rocket>().Redirect();
                    if (secondClosest != null)
                    {
                        //Vector3 hitPosition = hit.point; // first hit o sa dispara posibil fiindca racheta face bum
                        RaycastHit secondHit;
                        Vector3 origin = hit.transform.position; Vector3 dest = secondClosest.transform.position;
                        if (Physics.Raycast(origin, dest - origin, out secondHit, (dest - origin).magnitude))
                        {
                            yield return new WaitForSeconds(timeBetweenRicos);
                            if (secondClosest != null)
                            {
                                secondClosest.Damage(damage * 2);
                                effectManager.ShootEff(origin, dest, hit.normal, Color.red * 16);
                            }
                        }        
                    }
                       
                }
                
            }
        }
        else
        {
            //ray pentru cand nu este nimic nimerit
            //ShootEff-ul e facut sa nu mai puna particole atunci cand normala transmisa e -
            effectManager.ShootEff(transform.Find("BarrelPoint").position,
                transform.position + MainCamera.transform.forward * 100, Vector3.zero);
        }
        gunModel.Rotate(53, 0, 0);
    }
}
