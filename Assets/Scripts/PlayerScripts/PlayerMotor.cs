using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

/// <summary>
/// Takes Player input from Look, Move, and Jump methods. Has different air and ground accels and recognizes rigidbodies as skidding if they're moving too quickly. Wall jumps add force normal to the wall and local up
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    [SerializeField] private PlayerManager pm;
    private Rigidbody rb;
    
    [Header("Camera and Looking")]
    [SerializeField] private Transform cameraTransform;
    private Vector3 playerRotation = Vector3.zero, localCameraRotation;
    public float lookspeedMult = .4f;
    private const float MAX_LOOK_Y=90, MIN_LOOK_Y=-90;


    [Header("Grounded Movement")]
    public LayerMask groundLayers;
    [Tooltip("Acceleration values depentant on direction")] public float groundForwardAccel = 18000, groundBackAccel = 10000, groundSideAccel = 15000;


    [Tooltip("Speed the player skids out at")] public float skidVelocity = 30f;
    public float skidFriction = 500f, groundFriction = 800f;
    [Tooltip("Maximum angle the player can be at before losing grip on the surface")] public float groundAngleDifference = 70f;
    private const float PLAYER_GROUNDED_RADIUS = .35f, PLAYER_GROUNDED_HEIGHT = .87f;
    Vector3 moveInputs;//Value that stores player joystick or WASD


    [Header("Air Movement")]
    public float airResistance = .001f;
    public float airAccel = .1f;

    [Header("Jumping and Wall Jumping")]
    public LayerMask wallLayers;
    private bool isPressingJump, alreadyJumped=false;
    private Vector3 lastWallHit = Vector3.down;
    public float jumpForce = 1500f, wallJumpForce = 500f, wallJumpUp = 1000f, localJumpReduction = 2f;
    [Tooltip("Time the player is considered grounded after walking off a ledge")] public float coyoteTime=.25f;
    public float lastFrameGrounded;


    void Start()
    {
        if (pm == null)
        {
            Debug.LogWarning("No connected Player Manager");
        }
        else
        {
            pm.lookDelegate += Look;
            pm.moveDelegate += Move;
            pm.jumpDelegate += Jump;
        }
        rb = GetComponent<Rigidbody>();
        localCameraRotation.y = cameraTransform.rotation.eulerAngles.y;
    }

    void Look(CallbackContext lr)
    {
        Vector2 lookRot = lr.ReadValue<Vector2>();
        playerRotation.y += lookRot.x*lookspeedMult;
        localCameraRotation.x = Mathf.Clamp(localCameraRotation.x-lookRot.y * lookspeedMult,MIN_LOOK_Y,MAX_LOOK_Y);
        rb.MoveRotation(Quaternion.Euler(playerRotation));
        cameraTransform.eulerAngles = localCameraRotation+playerRotation;
    }

    void Move(CallbackContext mi)
    {
        Vector2 mis = mi.ReadValue<Vector2>();
        moveInputs.x = mis.x;
        moveInputs.z = mis.y;
    }

    void Jump(CallbackContext jc)
    {
        if (!(isPressingJump = jc.ReadValueAsButton()))
        {
            alreadyJumped = false;
        }
    }

    private void FixedUpdate()
    {
        RaycastHit rayHit;
        bool isSkidding;
        bool isGrounded;
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);

        //Check if Grounded
        {
            if (!Physics.SphereCast(transform.position, PLAYER_GROUNDED_RADIUS, -transform.up, out rayHit, PLAYER_GROUNDED_HEIGHT - PLAYER_GROUNDED_RADIUS / 2, groundLayers, QueryTriggerInteraction.Ignore))
            {
                isGrounded = false;
                isSkidding = false;
            }
            else
            {
                Vector2 localRBXZVelocity = new Vector2(localVelocity.x, localVelocity.z);

                if(isGrounded = Vector3.Angle(rayHit.normal, transform.up) < groundAngleDifference &&
                    (rayHit.distance - PLAYER_GROUNDED_RADIUS < Mathf.Abs(Mathf.Cos(Vector3.Angle(gameObject.transform.up, rayHit.normal)))))
                {
                    //Reset last wall hit and coyote time last frame if grounded
                    lastWallHit = -transform.up;
                    lastFrameGrounded = Time.fixedTime;
                }
                isSkidding = localRBXZVelocity.magnitude > skidVelocity;
            }
        }


        float tempJump = 0;
        Vector3 wallJump = Vector3.zero;
        if(isPressingJump && (isGrounded || ((lastFrameGrounded + coyoteTime) > Time.fixedTime)) && !alreadyJumped)
        {
            lastFrameGrounded = 0;//Resetting the last grounded frame makes sure you can't coyote time jump after already jumping
            tempJump = jumpForce;
            alreadyJumped = true;
        }else if (isPressingJump && !alreadyJumped)
        {
            //Try wall jump
            bool couldWallJump = false;
            Vector3 combinedAngle = Vector3.zero;
            var physResult = Physics.OverlapCapsule(transform.position + transform.up * .9f,
                transform.position - transform.up * .9f, 1.55f, wallLayers, QueryTriggerInteraction.Ignore);//This is likely unnecessary; I just don't know if foreach would run the physics sim multiple times. I doubt it
            foreach (Collider col in physResult)
            {
                Vector3 tempAngle = transform.position - col.ClosestPoint(transform.position);
                if (Vector3.Angle(tempAngle, lastWallHit) >= 85)
                {
                    combinedAngle += tempAngle;
                    couldWallJump = true;
                }
            }
            if (couldWallJump)
            {
                lastWallHit = combinedAngle;
                wallJump = combinedAngle.normalized * wallJumpForce + transform.up * (wallJumpUp - localVelocity.y*localJumpReduction);
            }
            else
            {
                wallJump = Vector3.zero;
            }
            alreadyJumped = true;//Eating the jump here seems to make the game play better than it otherwise would with bunnyhopping
        }
        //TODO Jetpack stuff maybe
        //shouldJump = false;


        //Apply Forces 
        {
            float tempX, tempZ;
            if (isSkidding)
            {//Skidding on the ground
                tempX = (moveInputs.x * groundSideAccel) * airAccel - localVelocity.x * skidFriction;
                tempZ = moveInputs.z * ((moveInputs.z > 0) ? groundForwardAccel : groundBackAccel) * airAccel - localVelocity.z * skidFriction;
            }
            else if (isGrounded)
            {
                tempX = (moveInputs.x * groundSideAccel) - localVelocity.x * groundFriction;
                tempZ = moveInputs.z * ((moveInputs.z > 0) ? groundForwardAccel : groundBackAccel) - localVelocity.z * groundFriction;
            }
            else//In Air
            {
                tempX = moveInputs.x  * airAccel - localVelocity.x * airResistance;
                tempZ = moveInputs.z * airAccel - localVelocity.z * airResistance;
            }

            rb.AddForce(wallJump + transform.TransformDirection(Time.fixedDeltaTime * tempX, tempJump - localVelocity.y, Time.fixedDeltaTime * tempZ));
        }
    }

}