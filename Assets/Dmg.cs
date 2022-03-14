using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dmg : MonoBehaviour
{
    /* Asta e un exemplu de clasa pentru inamici/obiecte care isi pot lua damage
     * 
     * Pentru utilizare, odata gasit gameobject-ul (notat x sa zicem)
     * cu care s-a lovit ray-ul spre exemplu
     * 
     * 
     * Dmg damageScript = x.GetComponent<Dmg>()
     * if (damageScript != null)
     * damageScriptDamage(damageValue);
     */


    public bool ignoreHp;
    public float hp = 100;
    float _starthp;
    public bool resetHp;
    public GameObject externModel;

    private void Start()
    {
        _starthp = hp;
        if (ignoreHp)
            hp = Mathf.Infinity;

        if (externModel == null)
            externModel = this.gameObject;
    }
    public void Damage(float damage)
    {

        if (ignoreHp)
            return;

        if (hp < 0 && resetHp)
            hp = 100;

        hp -= damage;
        externModel.GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.white, hp / _starthp);
        if (hp < 0)
        {
            externModel.GetComponent<Renderer>().material.color = Color.black;
            this.enabled = false;
        }
        


    }
}
