using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderDemo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<MeshRenderer>().material.SetFloat(
                    "Vector1_56dab2fbc6ed4d759135a10c9ada3163",
                    (Mathf.Sin(3*Time.time)+1)/2);
    }
}
