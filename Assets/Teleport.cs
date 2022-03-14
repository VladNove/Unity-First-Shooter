using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    // Start is called before the first frame update
    bool teleporting;
    public GameObject psEffect;
    GameObject instantiatedEffect;
    Transform lastpos;
    EffectManager em;
    bool enemyTel;
    GameObject enemyTelObject;
    void Start()
    {
        em = GameObject.FindObjectOfType<EffectManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //daca player-ul apasa T si tinteste spre ceva
        if (Input.GetKeyDown(KeyCode.T) && Physics.Raycast(transform.position, transform.forward, 20f))
            StartTeleport();
        
        if (teleporting)
            Teleporting();

        if (Input.GetKeyUp(KeyCode.T) && teleporting)
            EndTeleport();

    }

    void StartTeleport()
    {
        teleporting = true;
        //se instantiaza particulele
        instantiatedEffect = Instantiate(psEffect);
        //se adauga efectul de slow motion
        em.ChroAbrEnable();
        //se incetineste timpul
        Time.timeScale = 0.2f;
        enemyTel = false;
    }

    void Teleporting()
    {
        //testam daca tintim spre ceva
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 20f))
        {
            //daca tintim spre un inamic PhysicsCapsule
            if (hit.rigidbody && hit.transform.tag.Contains("PC"))
            {
                //mutam efectul de particule un pic sub pozitia inamicului, si il orientam in sus
                instantiatedEffect.transform.position = hit.transform.position - Vector3.up;
                instantiatedEffect.transform.rotation = Quaternion.LookRotation(Vector3.up);

                //retinem inamicului tintit
                enemyTel = true;
                enemyTelObject = hit.transform.gameObject;
            }
            else
            {

                //daca nu tintim spre un inamic, mutam efectul de particule in punctul nimerit
                //si il orientam spre normala suprafetei
                instantiatedEffect.transform.position = hit.point;
                instantiatedEffect.transform.rotation = Quaternion.LookRotation(hit.normal);
                enemyTel = false;
            }
        }
    }

    void EndTeleport()
    {
        teleporting = false;
        //oprim slow motion-ul
        em.ChroAbrDisable();
        Time.timeScale = 1f;

        GameObject Player = GameObject.Find("Player");

        //daca a fost "selectat un inamic, il teleportam la loctia player-ului
        if (enemyTel)
            enemyTelObject.transform.position = Player.transform.position;
        

        //teleportam Player-ul la locatia particulelor
            Player.transform.position = instantiatedEffect.transform.position + Vector3.up;
        
        //distrugem particulele un pic dupa teleportare
        Destroy(instantiatedEffect, 0.5f);
    }


}
