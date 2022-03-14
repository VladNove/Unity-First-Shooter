using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float lookSensitivity;
    public bool mouselock;
    public float cameraTilt;
    float currentCameraTilt;
    float verticalLookRange = 180;

    ///<summary> directia in care se uita jucatorul</summary>
    public Vector3 lookdir;
    public Transform playerCamera;
    PlayerMovement playerMovement;
    public float lookLockSpeed;
    public float tiltSpeed;

    public bool flipped;
    void Start()
    {
        MouseLockUpdate();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            mouselock = !mouselock;
            MouseLockUpdate();
        }

        
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;

        //float rotX = playerCamera.localRotation.eulerAngles.x;

       // if (flipped && (rotX < 80 || rotX > 280)) mouseX *= -1;


        float mouseY = - Input.GetAxis("Mouse Y") * lookSensitivity;

        Vector3 rotTilt = new Vector3(mouseX, mouseY, 0);
        rotTilt = Quaternion.AngleAxis(-currentCameraTilt, Vector3.forward) * rotTilt;

        mouseX = rotTilt.x;
        mouseY = rotTilt.y;
     
        playerCamera.Rotate(mouseY, 0, 0);
        transform.Rotate(0, mouseX, 0);

        Vector3 rot = playerCamera.localRotation.eulerAngles;

        FlipCheck(rot);

        //daca atingem solul sau inclinam camera(pt wallrun) micsoram limita unghiului vertical
        //200 de viteza in grade pe secunda cu care limitam unghiul
        if (playerMovement.touchingGround || currentCameraTilt != 0)
        {
            if (!flipped)
                verticalLookRange = 90;
            verticalLookRange = Mathf.MoveTowards(verticalLookRange, 89, Time.deltaTime * lookLockSpeed);
        }
        else
            verticalLookRange = 180;

        //Debug.Log(rot + " " + verticalLookRange);

        //limitam unghiul vertical (si dam flip la y,z daca e necesar)
        rot = LimitAngle(rot);

        //aproiem tilt-ul camerei de tilt-ul dorit (daca unghiul a fost limitat)
        currentCameraTilt = Mathf.MoveTowards(currentCameraTilt, cameraTilt, Time.deltaTime * tiltSpeed);
        if (verticalLookRange < 90)
            rot.z = currentCameraTilt;

        
        //aplicam rotatile de corectie
        playerCamera.localRotation = Quaternion.Euler(rot);
        lookdir = playerCamera.forward;
    }

    void MouseLockUpdate()
    {
        Cursor.visible = !mouselock;
        Cursor.lockState = (mouselock) ? CursorLockMode.Locked : CursorLockMode.None;

    }

    void FlipCheck(Vector3 rot)
    {
        if (rot.y > 170 && rot.y < 190)
            flipped = true;
        else
            flipped = false;
    }
    Vector3 LimitAngle(Vector3 rot)
    {
        //multe giumbuslucuri geometrice pe aici, m-a durut capul tare scriind acest cod
        if (verticalLookRange <= 90)
        {

            if (rot.x > verticalLookRange && rot.x < 180)
            rot.x = verticalLookRange;

            else if (rot.x < 360 - verticalLookRange && rot.x >= 180)
            rot.x = 360 - verticalLookRange;

        
            rot.y = 0;
            rot.z = 0;
        }
        else if (flipped)
        {
            if (rot.x < 180 - verticalLookRange && rot.x < 180)
                rot.x = 180 - verticalLookRange;

            else if ( rot.x > 360 - (180 - verticalLookRange) && rot.x >= 180)
                rot.x = 360 - (180 - verticalLookRange);
        }
        return rot;
    }
}
