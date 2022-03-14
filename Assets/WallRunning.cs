using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rb;
    PlayerMovement pm;
    PlayerLook pl;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        pl = GetComponent<PlayerLook>();
        _jumpcooldown = wallJumpCoolDown;
    }
    bool jump;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) jump = true;
        if (Input.GetKeyUp(KeyCode.Space)) jump = false;
    }
    public bool WallRunEnabled;
    public bool wallOnLeft;
    public bool wallOnRight;
    public bool isWallRunning;
    bool startedWallRunning;
    public float stickForce;
    public float upForce;
    public float fowdForce;
    public float wallJumpStrenght;
    public float camTilt;
    public float wallJumpCoolDown;
    float _jumpcooldown = 0;
    int _lastjumpwall;
    Vector3 leftWallNormal;
    Vector3 rightWallNormal;
    // Update is called once per frame
    void FixedUpdate()
    {
        WallRunCheck();

        if (_jumpcooldown < wallJumpCoolDown)
            _jumpcooldown += Time.fixedDeltaTime;
        if (pm.touchingGround)
            _jumpcooldown = wallJumpCoolDown;

        //Debug.Log(_jumpcooldown);

        isWallRunning = false;
        pl.cameraTilt = 0;
        if (WallRunEnabled && !pm.touchingGround)
        { 
            if (wallOnLeft && !wallOnRight && Input.GetAxisRaw("Horizontal") <= 0 )
            {
                rb.AddForce(-leftWallNormal * stickForce);
                isWallRunning = true;
                pl.cameraTilt = -camTilt;
            }
            if (wallOnRight && !wallOnLeft && Input.GetAxisRaw("Horizontal") >= 0 )
            {
                rb.AddForce(-rightWallNormal * stickForce);
                isWallRunning = true;
                pl.cameraTilt = camTilt;
            }
            if (isWallRunning)
            {
                if (!startedWallRunning)
                {
                    rb.velocity = new Vector3(rb.velocity.x, Mathf.Clamp(rb.velocity.y, 0, rb.velocity.y / 2), rb.velocity.z);
                }
                startedWallRunning = true;
                rb.AddForce(transform.up * upForce);
                if (wallOnRight)
                    rb.AddForce(Quaternion.AngleAxis(90, Vector3.up) * rightWallNormal * fowdForce);
                if (wallOnLeft)
                    rb.AddForce(Quaternion.AngleAxis(-90, Vector3.up) * leftWallNormal * fowdForce);
            }
            else
                startedWallRunning = false;
        }

        if (jump && (_jumpcooldown >= wallJumpCoolDown || JumpingOnDifferentWall()))
        {
            jump = false;

            if (isWallRunning)
            {
                WallJump();
                _jumpcooldown = 0;
                if (wallOnLeft) _lastjumpwall = -1;
                else _lastjumpwall = 1;
            }
            
        }
    }

    //verificam daca exista ziduri pe dreapta sau pe stanga
    void WallRunCheck()
    {
        //Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance);
        
        Ray checkdir;

        bool auxWallCheck = false;
        checkdir = new Ray(transform.position, 1*transform.forward + transform.right);
        auxWallCheck = auxWallCheck || RunnableWall(checkdir,1);
        checkdir = new Ray(transform.position, 0*transform.forward + transform.right);
        auxWallCheck = auxWallCheck || RunnableWall(checkdir,1);
        checkdir = new Ray(transform.position, -1*transform.forward + transform.right);
        auxWallCheck = auxWallCheck || RunnableWall(checkdir,1);

        wallOnRight = auxWallCheck;

        auxWallCheck = false;
        checkdir = new Ray(transform.position, 1 * transform.forward - transform.right);
        auxWallCheck = auxWallCheck || RunnableWall(checkdir,-1);
        checkdir = new Ray(transform.position, 0 * transform.forward - transform.right);
        auxWallCheck = auxWallCheck || RunnableWall(checkdir,-1);
        checkdir = new Ray(transform.position, -1 * transform.forward - transform.right);
        auxWallCheck = auxWallCheck || RunnableWall(checkdir,-1);

        wallOnLeft = auxWallCheck;
    }
    public float jumpUpForce;
    void WallJump()
    {
            if (wallOnLeft)     
                rb.AddForce(leftWallNormal * wallJumpStrenght + transform.forward + transform.up * jumpUpForce, ForceMode.Impulse);
            if (wallOnRight)
                rb.AddForce(rightWallNormal * wallJumpStrenght + transform.forward + transform.up * jumpUpForce, ForceMode.Impulse);
    }

    //raycast si verificam daca zidurile sunt perpendiculare cu solul
    bool RunnableWall(Ray checkdir, int dir)
    {
        RaycastHit hit;
        if (Physics.Raycast(checkdir, out hit, 1.1f))
            if (Mathf.Abs(hit.normal.y) < 0.01f)
            {
                if (dir > 0)
                    rightWallNormal = hit.normal;
                if (dir < 0)
                    leftWallNormal = hit.normal;
                return true;
            }

        return false;
    }

    //verificam daca ultima saritura a fost pe zidul opus
    bool JumpingOnDifferentWall()
    {
        if (_lastjumpwall == -1 && wallOnRight)
            return true;
        else if (_lastjumpwall == 1 && wallOnLeft)
            return true;
        return false;
    }
}
