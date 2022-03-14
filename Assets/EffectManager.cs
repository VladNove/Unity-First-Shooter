using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class EffectManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject shootEffObject;
    public GameObject bulletHole;
    public VolumeProfile ppProfile;
    public GameObject dashPS;
    public MeshRenderer weaponRenderer;
    public ParticleSystem explosionDebris;
    public ParticleSystem muzzleFlash;
    public Light muzzleFlashLightInside;
    public Light muzzleFlashLightOutside;
    Vignette ppVignette;
    ChromaticAberration ppChro;
    float defaultVignette;
    float defaultChro;
    public float variable;

    void Start()
    {

        ppProfile.TryGet<Vignette>(out ppVignette);
        ppProfile.TryGet <ChromaticAberration>(out ppChro);
        defaultVignette = ppVignette.intensity.value;
        defaultChro = ppChro.intensity.value;
    }

    public Vector3 debug  = Vector3.zero;
    public Vector3 rayo;
    public Vector3 rayd;
    private void Update()
    {
        //resetam valoarea vignettei la cea initiala
        ppVignette.intensity.value = Mathf.MoveTowards(ppVignette.intensity.value, defaultVignette, Time.deltaTime/4);
    }

    public void ExplosionDebris(Vector3 position)
    {
        Instantiate(explosionDebris, position, Quaternion.Euler(-90, 0, 0));
    }

    public void MuzzleFlash(Vector3 barrelPosition, Vector3 flashPositionInside, Vector3 flashPositionOutside)
    {
        ParticleSystem auxobj = Instantiate(muzzleFlash, barrelPosition, Quaternion.identity);
        Destroy(auxobj, 3);

        //de ce e aici new Vector3(coor alea)? nu poti pune direct flashPosition?
        Light lightInside = Instantiate(muzzleFlashLightInside, new Vector3(flashPositionInside.x, flashPositionInside.y, flashPositionInside.z), Quaternion.identity);
        Light lightOutside = Instantiate(muzzleFlashLightOutside, new Vector3(flashPositionOutside.x, flashPositionOutside.y, flashPositionOutside.z), Quaternion.identity);
        
        Destroy(lightInside, muzzleFlash.main.duration / 3f);
        Destroy(lightOutside, muzzleFlash.main.duration / 3f);


    }

    public void ShootEff(Vector3 gunPos, Vector3 hitPos, Vector3 normal)
    {
        ShootEff(gunPos, hitPos, normal, Color.yellow / 2);
    }

    public void ShootEff(Vector3 gunPos, Vector3 hitPos, Vector3 normal, Color x)
    {
        //Debug.Log(normal);

        //adaugam raza
        GameObject newEffect;

        if (!normal.Equals(Vector3.zero))
            newEffect = Instantiate(shootEffObject, hitPos + normal / 10, Quaternion.LookRotation(normal, Vector3.up));
        else
            newEffect = Instantiate(shootEffObject, hitPos + normal / 10, Quaternion.LookRotation(Vector3.up));

        //randam raza
        var lr = newEffect.GetComponent<LineRenderer>();
        lr.SetPosition(0, gunPos);
        lr.SetPosition(1, hitPos);
        lr.material.SetColor(
            "_EmissionColor", x * 16);

        //daca nu am nimerit nimic oprim particulele
        if (normal.Equals(Vector3.zero))
            newEffect.GetComponent<ParticleSystem>().Stop();

        //distrugem particulele ca sa nu adaugam clutter
        Destroy(newEffect, 3);

        //daca am nimerit ceva adaugam bulletHole
        /*
        if (!normal.Equals(Vector3.zero))
        {
            //TODO: ar trebuii ca parent-ul bullet-hole-ului sa fie obiectul nimerit
            GameObject newBulletHole = Instantiate(bulletHole, hitPos + normal / 10, Quaternion.LookRotation(normal, Vector3.up));
            Destroy(newBulletHole, 10);
        }
        */
    }

    public void DarkenScreen()
    {
        ppVignette.intensity.value += 0.2f;
    }

    public void ChroAbrEnable()
    {
        ppChro.intensity.value += 1f;
    }

    public void ChroAbrDisable()
    {
        ppChro.intensity.value -= 1f;
    }

    //USE: GameObject.FindObjectOfType<EffectManager>().DashEffect();
    public void DashEffect(float duration)
    {
        GameObject player = GameObject.Find("Player");
        GameObject newPS = Instantiate(dashPS,player.transform);

        //sistemul de particule trebuie oprit pentru a-i putea schimba durata
        newPS.GetComponent<ParticleSystem>().Stop();
        var mainPS = newPS.GetComponent<ParticleSystem>().main;
        mainPS.duration = duration;
        newPS.GetComponent<ParticleSystem>().Play();


        Destroy(newPS, 0.5f);
        
    }

    public void DashEffect()
    {
        DashEffect(0.2f);

    }

    public void OnDestroy()
    {
        //era un bug ca daca inchizi jocul inainte sa se reseteze "damage-ul" vignietta ramanea
        ppVignette.intensity.value = defaultVignette;
    }

    /*
    public void HighLightColor(Color x)
    {
        weaponRenderer.material.SetColor(
            "_EmissionColor", x * 16);
    }
    */

    public void HighLightFill(float fill)
    {
        
        weaponRenderer.material.SetFloat("FIll",fill);
    }
}
