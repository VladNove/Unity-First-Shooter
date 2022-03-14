using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShake : MonoBehaviour
{
    public float shake;
    public float MaxShake;
    public float SmoothShake;

    private Vector3 InitialPosition;

    void Start()
    {
        InitialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float movementX = -Input.GetAxis("Mouse X") * shake;
        float movementY = -Input.GetAxis("Mouse Y") * shake;

        // Cap pentru shake
        movementX = Mathf.Clamp(movementX, -MaxShake, MaxShake);
        movementY = Mathf.Clamp(movementY, -MaxShake, MaxShake);

        Vector3 finalPosition = new Vector3(movementX, movementY, 0);
        //interpolare intre pozitia initiala si cea finala + viteza sau timpul de tranzitie de la pozitii
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + InitialPosition, Time.deltaTime * SmoothShake);
        
    }
}
