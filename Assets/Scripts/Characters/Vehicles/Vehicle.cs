using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Vehicle : MonoBehaviour
{
    [Header("Engine")]
    public float motorForce = 1500f;
    public float maxSpeed = 30f;           // m/s (~108 km/h)
    public float reverseSpeed = 10f;

    [Header("Braking")]
    public float brakeForce = 3000f;
    public float handbrakeForce = 5000f;

    [Header("Drifting")]
    public float driftSidewaysFriction = 0.2f;  // lower = more slide (0.1 wild, 0.4 mild)
    public float driftYawAssist = 800f;          // torque that rotates the car into the corner
    public float driftDragForce = 25f;           // speed scrubbed per m/s of lateral slip

    [Header("Steering")]
    public float maxSteerAngle = 35f;
    public float steerSpeed = 5f;          // how fast steering responds
    public AnimationCurve steerCurve;      // reduces steer angle at high speed
    public float turnAssist = 10f;

    [Header("Landing")]
    public float landingBounceForce = 0.5f;
    public float airTimeThreshold = 0.2f;

    [Header("Stability")]
    public float shakeAmount = 0.3f;
    public float downforce = 100f;         // keeps car planted at speed
    public Vector3 centerOfMassOffset = new Vector3(0f, -0.5f, 0f);
    public float uprightStrength = 5f;
    public float uprightDeadzone = 15f;
    public float flipThreshold = 100f;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    [Header("Wheel Meshes")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;

    [Header("Seating")]
    public Transform driverSeat;
    public Transform exitPoint;
    public Transform cameraTarget;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public bool isOccupied;
    [HideInInspector] public Character currentDriver;

    private float currentSteerAngle;
    private float currentSpeed;
    private bool isAirborne;
    private float airTime;
    private float timeSinceEntered;
    private bool settling;
    private RigidbodyConstraints baseConstraints;
    private WheelFrictionCurve originalRearSidewaysFriction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMassOffset;
        rb.sleepThreshold = 0f;

        // Default steer curve if none assigned — reduces steering at high speed
        if (steerCurve == null || steerCurve.length == 0)
        {
            steerCurve = new AnimationCurve(
                new Keyframe(0f,    1f),   // full steering at 0 speed
                new Keyframe(20f,  0.7f), // half steering at 15 m/s
                new Keyframe(30f,  0.3f)  // minimal steering at 30 m/s
            );
        }
    }

    private void Start()
    {
        originalRearSidewaysFriction = rearLeft.sidewaysFriction;
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        // Debug.Log($"[VehicleFixed START] vel={rb.velocity} mag={rb.velocity.magnitude:F3}");
        
        StabilizeRotation();
        // Debug.Log($"[After Stabilize] vel={rb.velocity} mag={rb.velocity.magnitude:F3}");
        
        TrackAirborne();
        // Debug.Log($"[After Airborne] vel={rb.velocity} mag={rb.velocity.magnitude:F3}");
    }

    private void StabilizeRotation()
    {
        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);

        if (tiltAngle < uprightDeadzone) return;

        if (tiltAngle > flipThreshold)
        {
            rb.angularVelocity = new Vector3(
                rb.angularVelocity.x * 0.1f,
                rb.angularVelocity.y,
                rb.angularVelocity.z * 0.1f
            );
        }

        Vector3 correction = Vector3.Cross(transform.up, Vector3.up);
        float strength = Mathf.InverseLerp(uprightDeadzone, 180f, tiltAngle);
        rb.AddTorque(correction * strength * uprightStrength, ForceMode.Acceleration);
    }

    private bool IsGrounded()
    {
        WheelHit hit;

        int groundedWheels = 0;
        foreach (var wc in new[] {frontLeft, frontRight, rearLeft, rearRight})
        {
            if (wc.GetGroundHit(out hit)) groundedWheels++;
        }
        return groundedWheels >= 2;
    }

    private void TrackAirborne()
    {
        timeSinceEntered += Time.fixedDeltaTime;

        bool grounded = IsGrounded();

        if (!grounded)
        {
            airTime += Time.fixedDeltaTime;
            isAirborne = airTime >= airTimeThreshold;
        }
        else if (isAirborne)
        {
            OnLand();
            isAirborne = false;
            airTime = 0f;
        }
        else
        {
            airTime = 0f;
        }
    }

    // private void OnLand()
    // {
    //     // Scale bounce with how fast we were falling
    //     float fallSpeed    = Mathf.Abs(rb.velocity.y);
    //     float bounceAmount = fallSpeed * landingBounceForce;

    //     rb.AddForce(Vector3.up * bounceAmount, ForceMode.Impulse);

    //     if (fallSpeed > 5f)
    //     {
    //         rb.velocity *= 0.7f;
    //         Debug.Log($"Hard landing! Fall speed: {fallSpeed:F1}");
    //     }
    //     else
    //     {
    //         Debug.Log($"Soft landing. Fall speed: {fallSpeed:F1}");
    //     }

    //     // Optional: camera shake here later
    //     Debug.Log($"Landed! Fall speed: {fallSpeed:F1} m/s");
    // }

    private void OnLand()
    {
        Debug.Log($"[OnLand FIRED] vel={rb.velocity} isOccupied={isOccupied}");

        float fallSpeed = Mathf.Abs(rb.velocity.y);

        // Kill vertical velocity — suspension absorbs it
        Vector3 v = rb.velocity;
        v.y = 0f;
        rb.velocity = v;

        // Shake intensity scales with fall speed
        

        rb.angularVelocity = new Vector3(
            rb.angularVelocity.x * 0.1f,   // kill X tilt
            rb.angularVelocity.y * 0.5f,   // reduce Y spin
            rb.angularVelocity.z * 0.1f    // kill Z tilt
        );

        float updatedShakeAmount = Mathf.Clamp(fallSpeed * shakeAmount, 0f, 3f);
        rb.AddTorque(-transform.right * updatedShakeAmount, ForceMode.VelocityChange);

        // Random torque in all directions for a shudder effect
        // Vector3 randomTorque = new Vector3(
        //     Random.Range(-1f, 1f),
        //     Random.Range(-0.2f, 0.2f),  // small Y so it doesn't spin
        //     Random.Range(-1f, 1f)
        // ) * shakeAmount;

        // rb.AddTorque(randomTorque, ForceMode.VelocityChange);

        if (fallSpeed > 5f)
        {
            rb.velocity *= 0.7f;
            Debug.Log($"Hard landing! Fall speed: {fallSpeed:F1}");
        }
        else
        {
            Debug.Log($"Soft landing. Fall speed: {fallSpeed:F1}");
        }
    }

    public void ApplyInput(float accel, float steer, bool handbrake)
    {
        currentSpeed = Vector3.Dot(rb.velocity, transform.forward);
        currentSpeed = Mathf.Abs(currentSpeed);

        // Release Y freeze once physics has settled
        if (settling && timeSinceEntered >= 0.3f)
        {
            rb.constraints = baseConstraints;
            settling = false;
        }

        HandleMotor(accel);
        HandleSteering(steer);
        HandleBraking(accel, steer, handbrake);
        ApplyDownforce();
        UpdateWheelMeshes();
    }

    

    private void HandleMotor(float accel)
    {
        // Cap speed
        if (accel > 0 && currentSpeed >= maxSpeed)
        {
            frontLeft.motorTorque = 0f;
            frontRight.motorTorque = 0f;
            rearLeft.motorTorque  = 0f;
            rearRight.motorTorque = 0f;
            return;
        }
        if (accel < 0 && currentSpeed >= reverseSpeed)
        {
            frontLeft.motorTorque = 0f;
            frontRight.motorTorque = 0f;
            rearLeft.motorTorque  = 0f;
            rearRight.motorTorque = 0f;
            return;
        }

        float rearTorque = accel * motorForce * 0.6f;
        float frontTorque = accel * motorForce * 0.4f; // 0.4 strength for front

        rearLeft.motorTorque  = accel * rearTorque;
        rearRight.motorTorque = accel * rearTorque;
        frontLeft.motorTorque = frontTorque;
        frontRight.motorTorque = frontTorque;
    }

    private void HandleSteering(float steer)
    {
        // Speed-sensitive steering
        float speedFactor  = steerCurve.Evaluate(currentSpeed);
        float targetAngle  = steer * maxSteerAngle * speedFactor;

        // Smoothly move to target angle
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle, steerSpeed * Time.deltaTime);

        frontLeft.steerAngle  = currentSteerAngle;
        frontRight.steerAngle = currentSteerAngle;

        if (currentSpeed > 1f && Mathf.Abs(steer) > 0.1f)
        {
            float turnHelp = steer * turnAssist * Mathf.InverseLerp(20f, 0f, currentSpeed);
            rb.AddTorque(0f, turnHelp, 0f, ForceMode.Acceleration);
        }
    }

    private void HandleBraking(float accel, float steer, bool handbrake)
    {
        if (handbrake)
        {
            WheelFrictionCurve driftCurve = originalRearSidewaysFriction;
            driftCurve.stiffness = driftSidewaysFriction;
            rearLeft.sidewaysFriction  = driftCurve;
            rearRight.sidewaysFriction = driftCurve;

            rearLeft.brakeTorque  = handbrakeForce;
            rearRight.brakeTorque = handbrakeForce;
            rearLeft.motorTorque  = 0f;
            rearRight.motorTorque = 0f;

            // Rotate the car into the corner so it doesn't just slide straight
            if (Mathf.Abs(steer) > 0.1f)
                rb.AddTorque(transform.up * steer * driftYawAssist, ForceMode.Acceleration);

            // Scrub speed based on how much the car is sliding sideways (tire friction loss)
            float lateralSlip = Mathf.Abs(Vector3.Dot(rb.velocity, transform.right));
            rb.AddForce(-rb.velocity.normalized * lateralSlip * driftDragForce, ForceMode.Force);

            return;
        }

        rearLeft.sidewaysFriction  = originalRearSidewaysFriction;
        rearRight.sidewaysFriction = originalRearSidewaysFriction;

        bool movingForward = Vector3.Dot(rb.velocity, transform.forward) > 0;

        if (accel < 0 && movingForward)
        {
            frontLeft.brakeTorque  = brakeForce;
            frontRight.brakeTorque = brakeForce;
            rearLeft.brakeTorque   = brakeForce;
            rearRight.brakeTorque  = brakeForce;
            rearLeft.motorTorque   = 0f;
            rearRight.motorTorque  = 0f;
        }
        else if (Mathf.Abs(accel) < 0.01f && currentSpeed < 0.5f)
        {
            // Idle — keep brakes held so WheelColliders stay settled
            frontLeft.brakeTorque  = brakeForce;
            frontRight.brakeTorque = brakeForce;
            rearLeft.brakeTorque   = brakeForce;
            rearRight.brakeTorque  = brakeForce;
        }
        else
        {
            frontLeft.brakeTorque  = 0f;
            frontRight.brakeTorque = 0f;
            rearLeft.brakeTorque   = 0f;
            rearRight.brakeTorque  = 0f;
        }
    }

    private void ApplyDownforce()
    {
        // Keeps car planted — more downforce at higher speeds
        rb.AddForce(-transform.up * downforce * rb.velocity.magnitude);
    }

    private void UpdateWheelMeshes()
    {
        UpdateWheel(frontLeft,  frontLeftMesh);
        UpdateWheel(frontRight, frontRightMesh);
        UpdateWheel(rearLeft,   rearLeftMesh);
        UpdateWheel(rearRight,  rearRightMesh);
    }

    private void UpdateWheel(WheelCollider col, Transform mesh)
    {
        if (mesh == null) return;
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.SetPositionAndRotation(pos, rot);

    }

    public float GetSpeedKPH() => rb.velocity.magnitude * 3.6f;

    public void OnDriverEnter(Character driver)
    {
        isOccupied       = true;
        currentDriver    = driver;
        timeSinceEntered = 0f;

        // Lock Y so the WheelCollider suspension spike can't launch us
        baseConstraints  = rb.constraints;
        rb.constraints   = baseConstraints | RigidbodyConstraints.FreezePositionY;
        settling         = true;
    }

    public void OnDriverExit()
    {
        isOccupied    = false;
        currentDriver = null;

        foreach (var wc in new[] { frontLeft, frontRight, rearLeft, rearRight })
        {
            wc.motorTorque = 0f;
            wc.brakeTorque = brakeForce;
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var wc in new[] { frontLeft, frontRight, rearLeft, rearRight })
        {
            if (wc == null) continue;

            Vector3 worldCenter = wc.transform.TransformPoint(wc.center);

            // Suspension top (where spring starts)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(worldCenter, 0.05f);

            // Suspension bottom (fully extended)
            Vector3 bottom = worldCenter - wc.transform.up * wc.suspensionDistance;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bottom, 0.05f);

            // Wheel at full extension (this should just touch the ground)
            Vector3 wheelContact = bottom - wc.transform.up * wc.radius;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(wheelContact, 0.05f);

            // Draw the suspension line
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(worldCenter, wheelContact);
        }
    }
}
