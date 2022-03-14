using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingEffectScript : MonoBehaviour
{
    // Start is called before the first frame update
    public float lineTime = 0.5f;
    float _deflineTIme;

    private void Start()
    {
        _deflineTIme = lineTime;
    }
    // Update is called once per frame
    void Update()
    {
        //miscsoram raza
        lineTime -= Time.deltaTime;
        GetComponent<LineRenderer>().widthMultiplier = lineTime / _deflineTIme;

        if (lineTime < 0) GetComponent<LineRenderer>().enabled = false;

        //schimbam luminozitatea particulelor
        GetComponent<ParticleSystemRenderer>().material.SetColor(
            "_EmissionColor", Color.Lerp(Color.black, Color.white * 25, lineTime * 2));
    }
}
