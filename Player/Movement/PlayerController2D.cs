using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController2D : MonoBehaviour
{
    // References
    public PlayerMovementStats moveStats;
    private Rigidbody2D rb;
    public Text velocityText;
    [SerializeField] private Animator anm;

    // Movement variables
    public float moveInput;
    private bool isFacingRight = true;
    private Vector2 moveVelocity;
    private bool isGrounded;
    private bool isMoving;

    // Jump variables
    private float verticalVelocity;
    private bool isJumping;
    private bool jumpWasReleased;
    private bool canJump;
    private bool isFalling;
    private float numberOfConsecutiveJumps;
    private bool isJumpHeld;

    private bool isPastApexThreshold;
    private float timePastApexThreshold;

    // Dash Variables
    private bool isDashing;
    private float dashTimer;

    private bool isCrouched = false;
    
    // Timers
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float jumpGraceTimer; // 0.1 seconds of gravity delay after jump 

    // Visualization
    private Color arcColor = new Color(0.3f, 0.8f, 1f, 0.6f);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        moveStats.groundCheck = GameObject.FindWithTag("GroundCheck").transform;
    }

    private void Update()
    {   
        // Set animator speed parameter based on horizontal velocity
        anm.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        // Set velocity
        velocityText.text = $"Velocity: {verticalVelocity}";

        // Call Functions
        BufferAndCoyote();
        JumpWasPressed();
        JumpWasReleased();
        MovementInput();
    }

    private void FixedUpdate()
    { 
        GroundCheck();
        ApplyGravity();

        if (isGrounded)
        {
            Move(moveStats.groundAcceleration, moveStats.groundDeceleration, moveInput, moveStats.maxMoveSpeed);
        }
        else if (isGrounded)
        {
            Move(moveStats.groundAcceleration, moveStats.groundDeceleration, moveInput, moveStats.maxMoveSpeed * moveStats.dashWithNoMovementSpeedFactor);
        }
        else
            Move(moveStats.airAcceleration, moveStats.airDeceleration, moveInput, moveStats.maxMoveSpeed);
    }

    #region Jump
    
    private void BufferAndCoyote()
    {
         // Handle timers
        if (jumpBufferTimer > 0)
            jumpBufferTimer -= Time.deltaTime;

        if (isGrounded && verticalVelocity <= 0) // Velocity check prevents coyote time from refreshing due to the few framed after jump where ground check still collides with ground...
            coyoteTimer = moveStats.coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // Jump if a jump input was buffered and we are within coyote time i.e are or were on ground for the specefied time
        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            Jump(); 
            jumpBufferTimer = 0; // clear jump buffer after jumping
            coyoteTimer = 0; // clear coyote timer after jumping
        }  
    }
    
    
    private void JumpWasPressed()
    {   
        // Add a jump to the buffer if the jump button was pressed this frame for the specefied time
        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = moveStats.jumpBufferTime;
    }

    private void JumpWasReleased()
    {
        if (Input.GetButtonUp("Jump"))
        {
            jumpWasReleased = true;
            Debug.Log("Jump Released");
        }
    }
    private void Jump()
    {   
        jumpGraceTimer = moveStats.jumpGraceTime;
    
        isJumping = true;
        
        verticalVelocity = moveStats.initialJumpVelocity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalVelocity);
        
        anm.SetTrigger("Jump");
    }

    private void ApplyGravity()
    {
        if (jumpGraceTimer > 0)
        {
            jumpGraceTimer -= Time.deltaTime;
            return; // skip gravity this frame
        }

        // --- IN AIR ---
        if (!isGrounded)
        {
            verticalVelocity += moveStats.Gravity * Time.deltaTime;

            // Apply jump cut (short hop)
            if (jumpWasReleased && verticalVelocity > 0 && isJumping)
            {
                jumpWasReleased = false;
                Debug.Log("Jump Cut Applied");
                verticalVelocity -= moveStats.onReleaseOffset;
                isFalling = true;
            }

            // Cap fall speed
            verticalVelocity = Mathf.Max(verticalVelocity, -moveStats.maxFallSpeed);
        }
        // --- LANDED ---
        else if (isGrounded && isFalling)
        {   
            isJumping = false;
            isFalling = false;
            isPastApexThreshold = false;

            Debug.Log("Landed");
            anm.SetTrigger("Land");

            verticalVelocity = 0f; 
        }
        else if (isGrounded)
        {
            jumpWasReleased = false;    
        }

        // --- PEAK OF THE JUMP ---
        if (verticalVelocity > 0 && Mathf.Abs(verticalVelocity) <= moveStats.apexThreshold)
        {
            if (!isPastApexThreshold)
            {
                isPastApexThreshold = true;
                timePastApexThreshold = 0f;
            }
        }

        // --- APEX HANG EFFECT ---
        if (isPastApexThreshold)
        {
            timePastApexThreshold += Time.fixedDeltaTime;
            if (timePastApexThreshold < moveStats.apexHangTime)
            {
                verticalVelocity = 0f;
            }
            else
            {
                verticalVelocity = -0.01f;
                isPastApexThreshold = false;
                isJumping = false;
                isFalling = true;
            }
        }
        
        // --- ON LAND ---
        if  (isGrounded && isJumping == false && isFalling == false)
        {
            verticalVelocity = 0f;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalVelocity);
    }
    #endregion

    #region Movement
    
    private void MovementInput()
    {
        // INPUT
        moveInput = Input.GetAxisRaw("Horizontal");
        
        // --- DASH WITHOUT MOVEMENT ---
        if (moveInput == 0 && Input.GetKeyDown(KeyCode.LeftShift) && isGrounded)
        {
            anm.SetTrigger("Dash");
            
            isCrouched = true;
            isDashing = false; // Set to false to prevent dash movement, but still trigger animation 
        }
        
        // --- DASH WITH MOVEMENT ---
        if (moveInput != 0 && Input.GetKeyDown(KeyCode.LeftShift) && isGrounded)
        {
            anm.SetBool("Dash", true);
            
            isCrouched = false;
            isDashing = true;
        }
        
        // --- STOP DASH ---
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isDashing = false;
            isCrouched = false;
            dashTimer = 0f;
            
            anm.SetBool("Dash", false);
            
            // If player releases dash while not moving, enter IDLE 
            if (moveInput == 0)
            {
                anm.SetTrigger("DashToIdle");
            }
            
            if (moveInput != 0)
            {
                anm.SetTrigger("DashToWalk");
            }
        }
    }
    private void Move(float acceleration, float deceleration, float input, float maxMoveSpeed)
    {   
        Vector2 targetVelocity = Vector2.zero;
        
        // --- NORMAL MOVEMENT ---
        if (input != 0 && !isDashing)
        {   
            // Set moving state
            isMoving = true;
            
            // Flip player
            FlipPlayer();
            
            // Accelerate towards target velocity based on input
            targetVelocity = new Vector2(input, 0f) * maxMoveSpeed;
            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
       
        // --- DASH ---
        if (isDashing && isGrounded)
        {
            // Increase movement speed
            if (dashTimer < moveStats.dashSlowDownDelay)
            {
                targetVelocity = new Vector2((input * maxMoveSpeed) + moveStats.dashStrength * input, 0);
            }

            // Increment dash duration
            dashTimer += 1 * Time.deltaTime;
            
            // If dash duration exceeds slow down delay, start decelerating
            if (dashTimer > moveStats.dashSlowDownDelay)
            {   
                targetVelocity = Vector2.Lerp(targetVelocity, Vector2.zero, moveStats.dashDeceleration * Time.deltaTime);
            }
            
            // If velocity reaches 0, enter crouch state  
            if (targetVelocity.magnitude < 0.1f)
            {
                isCrouched = true;
                isDashing = false;
                targetVelocity = Vector2.zero;
            }
            
            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        
        if (maxMoveSpeed != moveStats.maxMoveSpeed)
        {
            Debug.Log($"Max Move Speed: {maxMoveSpeed}");
        }



        else if (input == 0)
        {       
            // Set moving state
            isMoving = false;
            
            // Decelerate towards 0
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.deltaTime);
        }
        
        Debug.Log($"Move Velocity: {moveVelocity} Target Velocity: {targetVelocity} dashTimer: {dashTimer} isDashing: {isDashing} isCrouched: {isCrouched}");
        rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
    }

    private void FlipPlayer()
    {
        if (moveInput > 0 && !isFacingRight)
            Flip();
        else if (moveInput < 0 && isFacingRight)
            Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    #endregion

    #region Checks and visualization
    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(
            moveStats.groundCheck.position,
            moveStats.groundCheckRadius,
            moveStats.groundLayer
        );
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(moveStats.groundCheck.position, moveStats.groundCheckRadius);


        if (moveStats == null || !moveStats.showVisuals) return;

        // Visualization parameters
        int resolution = moveStats.arcRes;
        float steps = moveStats.visualizationSteps;
        float timeStep = moveStats.timeToReachApex * 2f / steps; // full jump duration

        Vector3 startPos = moveStats.groundCheck != null
            ? moveStats.groundCheck.position
            : transform.position;

        Vector3 prevPoint = startPos;
        Vector3 currentPoint;

        // Use the precomputed physics values
        float v0 = moveStats.initialJumpVelocity;
        float g = moveStats.Gravity;

        // Choose direction for visualization
        Vector3 horizontalDir = moveStats.drawRight ? Vector3.right : Vector3.left;
        float horizontalSpeed = moveStats.maxMoveSpeed;

        // Color of arc (you can set this in your class)
        Color arcColor = Color.yellow;
        Gizmos.color = arcColor;

        // Compute apex time and hang duration
        float apexTime = moveStats.timeToReachApex;
        float hangTime = moveStats.apexHangTime;

        // Total simulated time = rise + hang + fall
        float totalTime = (apexTime * 2f) + hangTime;

        for (int i = 1; i <= resolution; i++)
        {
            float t = i * (totalTime / resolution);
            float y;

            // Rising phase
            if (t <= apexTime)
            {
                y = v0 * t + 0.5f * g * t * t;
            }
            // Hang phase
            else if (t > apexTime && t <= apexTime + hangTime)
            {
                y = v0 * apexTime + 0.5f * g * apexTime * apexTime;
            }
            // Falling phase
            else
            {
                float fallT = t - (apexTime + hangTime);
                y = (v0 * apexTime + 0.5f * g * apexTime * apexTime) + (0.5f * g * fallT * fallT);
            }

            float x = horizontalSpeed * t;
            currentPoint = startPos + (horizontalDir * x) + (Vector3.up * y);

            if (moveStats.stopOnCollision && currentPoint.y < startPos.y)
                break;

            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
    }
    #endregion
}
    