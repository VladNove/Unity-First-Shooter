using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;
    public GameObject body;
    public CapsuleCollider capsuleCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        dashMeter = dashCount;
    }


    bool lockMovement = false;
    bool lockAbilities = false;

    bool movementInput;
    bool spaceInput;
    bool ctrlInput;
    bool shftInput;

    Vector3 moveDir;
    Vector3 lastMoveDir;
    void ReadInputMovement() {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        moveDir = Vector3.ClampMagnitude(moveDir, 1);   // we limit diagonal speed
        moveDir = transform.right * moveDir.x + transform.forward * moveDir.z;  // movement is relative to the camera
        if (moveDir.magnitude > 0.01f) 
            movementInput = true;
    }

    void ReadInputUtility()
    {
        if (Input.GetKeyDown(KeyCode.Space)) spaceInput = true;
        if (Input.GetKeyUp(KeyCode.Space)) spaceInput = false;
        if (Input.GetKeyDown(KeyCode.LeftControl)) ctrlInput = true;
        if (Input.GetKeyUp(KeyCode.LeftControl)) { ctrlInput = false; SlideEnd(); }
        if (Input.GetKeyDown(KeyCode.LeftShift)) shftInput = true;
        if (Input.GetKeyUp(KeyCode.LeftShift))  shftInput = false;
    }


    public bool touchingGround;
    bool groundIsClose;
    Vector3 GroundNormal;
    void GroundCheck()
    {
        //Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance);    // debugging
        Ray groundCheck = new Ray(transform.position, -transform.up);
        touchingGround = Physics.Raycast(groundCheck, 1.3f);

        RaycastHit hitInfo;
        groundIsClose = Physics.Raycast(groundCheck, out hitInfo, 2f);
        GroundNormal = hitInfo.normal;
    }
    

    private void Update()
    {
        movementInput = false;

        if (!lockMovement)
            ReadInputMovement();

        if (!lockAbilities)
            ReadInputUtility();
    }


    private void FixedUpdate()
    {

        planarVelocity = Vector3.Scale(rb.velocity, new Vector3(1, 0, 1));  // XOZ velocity
        GroundCheck();

        Movement();

        if (!isSliding)  // if not sliding
            DashRecharge();

        if (ctrlInput) {
            if (!isSliding  && touchingGround && slideCooldownValue < 0.01f)
                SlideStart();

            if (isSliding)
                Slide();
            else if (!touchingGround) // dunking
                Dunk();
        }   

        if (spaceInput)
        {
            Jump();
            timeSinceJump = 0;
        }


        //treaba asta lipeste player-ul de suprafete
        if (groundIsClose && timeSinceJump > 0.6f)
        {
            //daca suntem pe sol sol, normala va fi drept in sus, si fix inainte de aterizari
            // o sa para ca ne lipim de sol, asadar am pus acest if
            if (!GroundNormal.Equals(Vector3.up))
            rb.AddForce(-GroundNormal * 200);//aici era 800
        }

        //poate ar trebuii facut ceva cu masa aia

        //adaugam gravitatie doar daca viteza de urcare e prea mare
        if (!groundIsClose && rb.velocity.y > 10f)
            rb.AddForce(-transform.up * 132);


        if (shftInput && dashMeter > 1 && isDashing == false)
            DashStart();
        
        if (isDashing == true && timeSinceDash < dashTime)
            Dash();

        timeSinceJump += Time.fixedDeltaTime;
        lastMoveDir = moveDir;

        if (slideCooldownValue > 0f & !isSliding)
            slideCooldownValue -= Time.fixedDeltaTime;
        else if (slideCooldownValue < 0f)
            slideCooldownValue = 0f;

    }

    public float softCapSpeed;
    public float hardCapSpeedVertical;
    public float moveForceCoef; // controls player acceleration
    public float moveForceCoefAir; // controls air friction

    Vector3 velocityForce;
    Vector3 planarVelocity;
    void Movement()
    {

        if (rb.velocity.y > hardCapSpeedVertical)
            rb.velocity = new Vector3(rb.velocity.x, hardCapSpeedVertical, rb.velocity.z);

        if (groundIsClose)
        {
            Vector3 desiredVelocity = moveDir * softCapSpeed;

            // we get the XOY component
            velocityForce = desiredVelocity - planarVelocity;

            // We add a force so that the speed vector closes in on the desired direction and magnitude
            rb.AddForce(velocityForce * moveForceCoef);
        }
        else if (movementInput)
        {

            Vector3 desiredVelocity = moveDir * softCapSpeed;
            velocityForce = desiredVelocity - planarVelocity;

            if ((desiredVelocity.x > 0f && desiredVelocity.x < planarVelocity.x) || (desiredVelocity.x < 0f && desiredVelocity.x > planarVelocity.x))
                velocityForce.x = 0f;
            if ((desiredVelocity.z > 0f && desiredVelocity.z < planarVelocity.z) || (desiredVelocity.z < 0f && desiredVelocity.z > planarVelocity.z))
                velocityForce.z = 0f;

            rb.AddForce(velocityForce * moveForceCoefAir);
        }
    }


    public float jumpStrenght;

    float timeSinceJump = 1;
    void Jump()
    {
        if (touchingGround) //we only add upward force if the player is not mid-air
        {
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Max(0, rb.velocity.y), rb.velocity.z);
            spaceInput = false;
            
            if (isSliding) {
                rb.AddForce(transform.up * jumpStrenght / 1.5f, ForceMode.Impulse); 
                ctrlInput = false;          
                slideCooldownValue = slideCooldown / 3f;
                SlideEnd();
            }
            else if (isDashing)
                DashJump();
            else
                rb.AddForce(transform.up * jumpStrenght, ForceMode.Impulse);
        }
    }


    public float slideStrenght;
    public float sideSlideStrenght;
    public float slideCooldown;

    float slideCooldownValue = 0f;
    bool shouldSideSlide = false;  // prevents accidental side-sliding when strafe-sliding
    bool isSliding = false;

    Vector3 crouchVelocity;

    // the direction of the initial slide, needed for moving side to side (side-sliding) ,
    // without altering the direction after keypress 
    Vector3 slideDirection;    
    void SlideStart()
    {
        // makes hitbox smaller while sliding
        capsuleCollider.height = 1;
        body.transform.localScale = new Vector3(body.transform.localScale.x, 0.5f, body.transform.localScale.z);

        //  set slide direction
        //  if no WASD inputs are pressed, slide in the direction of where the player looks (look-sliding)
        //  if any WASD inputs are pressed, slide in the direction of strafing (strafe-sliding)
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.1f && Mathf.Abs(Input.GetAxisRaw("Vertical")) < 0.1f) {
            shouldSideSlide = true;
            slideDirection = new Vector3(transform.forward.x, 0, transform.forward.z);
        }
        else {
            slideDirection = moveDir; 
            shouldSideSlide = false;
        }

        crouchVelocity = slideDirection.normalized * slideStrenght;
        isSliding = true;
        slideCooldownValue = slideCooldown;
    }

    void Slide()
    {
        GameObject.FindObjectOfType<EffectManager>().DashEffect(0.0005f); //To DO Cooldown

        if (lastMoveDir.magnitude - moveDir.magnitude > 0.01f)  // if WASD state changed, allow side-sliding (slide is a strafe-slide)
            shouldSideSlide = true;

        // performs slide
        rb.velocity = new Vector3(crouchVelocity.x, rb.velocity.y, crouchVelocity.z);

        // enables side-sliding
        Vector3 rotatedVector;
        if (Input.GetAxisRaw("Horizontal") > 0.1f && shouldSideSlide) {
            rotatedVector = Quaternion.AngleAxis(90, Vector3.up) * slideDirection.normalized;
            rb.AddForce(rotatedVector * 100 * sideSlideStrenght);
        }
        if (Input.GetAxisRaw("Horizontal") < -0.1f && shouldSideSlide) {
            rotatedVector = Quaternion.AngleAxis(-90, Vector3.up) * slideDirection.normalized;
            rb.AddForce(rotatedVector * 100 * sideSlideStrenght);
        }
    }

    void SlideEnd()
    {
        isSliding = false;

        // we readjust the hitbox of the player, after sliding
        capsuleCollider.height = 2;
        body.transform.localScale = new Vector3(body.transform.localScale.x, 1, body.transform.localScale.z);
    }

    public float dunkStrenght;
    void Dunk()
    {   
        rb.velocity = new Vector3(rb.velocity.x, -dunkStrenght, rb.velocity.z);
    }


    void DashJump()
    {
        switch (Mathf.FloorToInt(dashMeter))
        {
            case 0:
                rb.velocity = new Vector3(0, 0, 0);
                rb.AddForce(transform.up * jumpStrenght / 1.65f + moveDir * dashJumpStrenght * 0.33f, ForceMode.Impulse);
                DashEnd();
                break;
            case 1:
                rb.velocity = new Vector3(0, 0, 0);
                rb.AddForce(transform.up * jumpStrenght / 1.70f + moveDir * dashJumpStrenght * 0.66f, ForceMode.Impulse);
                dashMeter--;
                DashEnd();
                break;
            case 2:
                rb.velocity = new Vector3(0, 0, 0);
                rb.AddForce(transform.up * jumpStrenght / 1.75f + moveDir * dashJumpStrenght, ForceMode.Impulse);
                dashMeter -= 2;
                DashEnd();
                break;
        }
    }

    public float dashTime;
    public float dashSpeed;
    public float dashCount;
    public float dashRechargeRate; // in seconds
    public float dashJumpStrenght;

    public event System.Action updateDashMeter;

    bool isDashing;
    float dashMeter;
    float timeSinceDash;
    void DashStart()
    {
        GameObject.FindObjectOfType<EffectManager>().DashEffect();
        shftInput = false;
        isDashing = true;
        lockMovement = true;
        timeSinceDash = 0;
        dashMeter--;

        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.1f && Mathf.Abs(Input.GetAxisRaw("Vertical")) < 0.1f)
            moveDir = new Vector3(transform.forward.x, 0, transform.forward.z);
    }

    void Dash()
    {
        timeSinceDash += Time.fixedDeltaTime;
        rb.velocity = new Vector3(moveDir.x * dashSpeed, 0, moveDir.z * dashSpeed);
        if (timeSinceDash > dashTime)
            DashEnd();
    }

    void DashEnd()
    {
        if (!touchingGround)
            rb.velocity = new Vector3(rb.velocity.x / 2f, rb.velocity.y, rb.velocity.z / 2f);
        lockMovement = false;
        isDashing = false;
    }

    void DashRecharge()
    {
        if (dashMeter < dashCount) {
            dashMeter += Time.fixedDeltaTime / dashRechargeRate;

            if (updateDashMeter != null && dashMeter < dashCount)
                updateDashMeter();
        }
        else
        {
            if (updateDashMeter != null)
                updateDashMeter();
            dashMeter = dashCount;
        }
            
    }

    public float GetDashMeter() {
        return dashMeter;
    }
}