using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementStats", menuName = "ScriptableObjects/PlayerMovementStats", order = 1)]
public class PlayerMovementStats : ScriptableObject
{
    [Header("Movement")]
    [Range(0.25f, 100f)] public float maxMoveSpeed = 12f;
    [Range(0.25f, 50f)] public float groundAcceleration = 5f;
    [Range(0.25f, 50f)] public float groundDeceleration = 20f;
    [Range(0.25f, 50f)] public float airAcceleration = 5f;
    [Range(0.25f, 50f)] public float airDeceleration = 5f;
    [Range(0.25f, 50f)] public float dashDeceleration = 5f;

    [Header("Ground/collision checks")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    [Range(0.1f, 1)]public float groundCheckRadius = 0.2f;

    [Header("Jumping")]
    [Range(1f, 20f)] public float jumpHeight = 6f;
    [Range(1f, 1.1f)] public float jumpHeightCompensationFactor = 1.05f;
    public float timeToReachApex = 0.4f;
    [Range(1f, 50f)] public float onReleaseOffset = 2f;
    public float maxFallSpeed = 20f;
    

    

    [Header("Jump Apex")]
    [Range(0.1f, 1f)] public float apexThreshold = 0.2f;
    [Range(0.01f, 1f)] public float apexHangTime = 0.1f;

    [Header("Coyote Time")]
    [Range(0.01f, 0.3f)] public float coyoteTime = 0.1f;

    [Header("Jump Buffering")]
    [Range(0.01f, 0.3f)] public float jumpBufferTime = 0.1f;

    [Header("Dash")]
    [Range(1f, 100f)] public float dashStrength = 20f;
    [Range(0.1f, 5f)] public float dashSlowDownDelay = 1f;
    [Range(0.1f, 5f)] public float dashWithNoMovementSpeedFactor = 0.5f;
    
    [Header("Jump Grace Time")]
    [Range(0.01f, 0.3f)] public float jumpGraceTime = 0.1f;

    [Header("Jump Visualization")]
    public bool showVisuals = true;
    public bool stopOnCollision = true;
    public bool drawRight = true;
    [Range(5, 100)] public int arcRes = 20;
    [Range(0f, 500f)] public int visualizationSteps = 90;

    public float Gravity { get; private set; }
    public float initialJumpVelocity { get; private set; }


    private void OnValidate()
    {   
        CalculateJumpVariables();
    }

    private void OnEnable()
    {
        CalculateJumpVariables();
    }

    private void CalculateJumpVariables()
    {   
        Gravity = -(2 * jumpHeight * jumpHeightCompensationFactor) / (timeToReachApex * timeToReachApex);
        initialJumpVelocity = Mathf.Abs(Gravity) * timeToReachApex;
    }
}
 