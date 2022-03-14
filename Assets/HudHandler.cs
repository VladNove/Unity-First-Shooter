using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudHandler : MonoBehaviour
{

    public GameObject dashMeter;
    PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        playerMovement.updateDashMeter += UpdateDashMeter;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    void UpdateDashMeter() {
        Slider slider = (Slider)dashMeter.GetComponent(typeof(Slider));
        slider.value = playerMovement.GetDashMeter();
            
    }
}
