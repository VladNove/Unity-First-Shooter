using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public float damage;
    public float range;


    public float FireDelay;
    protected float NextFire;
    public Camera MainCamera;


    public int maxAmmo;
    protected int CurrentAmmo;
    public float reloadTime;
    protected bool isReloading;


    protected int specialMode;
    protected float specialCooldown;

    protected float specialCooldownInitial;

    protected bool leftClickInput;
    protected bool rightClickInput;
    protected bool reloadInput;
    protected bool specialInput;

    protected virtual void Start()
    {
        CurrentAmmo = maxAmmo;

        specialMode = 1;
        specialCooldown = 0;
        isReloading = false;

        leftClickInput = false;
        rightClickInput = false;
        reloadInput = false;
        specialInput = false;

        specialCooldownInitial = 0;
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) leftClickInput = true;
        if (Input.GetKeyUp(KeyCode.Mouse0)) leftClickInput = false;
       // if (Input.GetKeyDown(KeyCode.Mouse1)) rightClickInput = true;
      //  if (Input.GetKeyUp(KeyCode.Mouse1)) rightClickInput = false;
        if (Input.GetKeyDown(KeyCode.R)) reloadInput = true;
        if (Input.GetKeyUp(KeyCode.R)) reloadInput = false;
        if (Input.GetKeyDown(KeyCode.Mouse1)) specialInput = true;
        if (Input.GetKeyUp(KeyCode.Mouse1)) specialInput = false;

    }

    protected virtual void FixedUpdate()
    {
        specialCooldown = Mathf.MoveTowards(specialCooldown, 0, Time.fixedDeltaTime);

        if (!isReloading)
        {

            if (CurrentAmmo == 0 || reloadInput == true)
                StartCoroutine(Reload());

            if (Time.timeSinceLevelLoad > NextFire)
            {

                if (leftClickInput)
                {
                    StartCoroutine(Shoot());
                }
                else if (specialInput && specialCooldown == 0)
                    Special();
            }

        }
    }
    protected virtual IEnumerator Reload()
    {
        yield return 0;
    }

    protected virtual IEnumerator Shoot (){
        yield return 0;
    }

    protected virtual void Special()
    {
        return;
    }

}

