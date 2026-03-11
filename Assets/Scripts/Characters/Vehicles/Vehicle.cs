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

    [Header("Steering")]
    public float maxSteerAngle = 35f;
    public float steerSpeed = 5f;          // how fast steering responds
    public AnimationCurve steerCurve;      // reduces steer angle at high speed

    [Header("Stability")]
    public float downforce = 100f;         // keeps car planted at speed
    public Vector3 centerOfMassOffset = new Vector3(0f, -0.5f, 0f);

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMassOffset;

        // Default steer curve if none assigned — reduces steering at high speed
        if (steerCurve == null || steerCurve.length == 0)
        {
            steerCurve = new AnimationCurve(
                new Keyframe(0f,    1f),   // full steering at 0 speed
                new Keyframe(15f,  0.5f), // half steering at 15 m/s
                new Keyframe(30f,  0.2f)  // minimal steering at 30 m/s
            );
        }
    }

    public void ApplyInput(float accel, float steer, bool handbrake)
    {
        currentSpeed = rb.velocity.magnitude;

        HandleMotor(accel);
        HandleSteering(steer);
        HandleBraking(accel, handbrake);
        ApplyDownforce();
        UpdateWheelMeshes();
    }

    private void HandleMotor(float accel)
    {
        // Cap speed
        if (accel > 0 && currentSpeed >= maxSpeed)
        {
            rearLeft.motorTorque  = 0f;
            rearRight.motorTorque = 0f;
            return;
        }
        if (accel < 0 && currentSpeed >= reverseSpeed)
        {
            rearLeft.motorTorque  = 0f;
            rearRight.motorTorque = 0f;
            return;
        }

        rearLeft.motorTorque  = accel * motorForce;
        rearRight.motorTorque = accel * motorForce;
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
    }

    private void HandleBraking(float accel, bool handbrake)
    {
        // Handbrake locks rear wheels (drift)
        if (handbrake)
        {
            rearLeft.brakeTorque  = handbrakeForce;
            rearRight.brakeTorque = handbrakeForce;
            rearLeft.motorTorque  = 0f;
            rearRight.motorTorque = 0f;
            return;
        }

        // Brake when pressing opposite direction
        bool movingForward = Vector3.Dot(rb.velocity, transform.forward) > 0;

        if (accel < 0 && movingForward)
        {
            // Braking
            frontLeft.brakeTorque  = brakeForce;
            frontRight.brakeTorque = brakeForce;
            rearLeft.brakeTorque   = brakeForce;
            rearRight.brakeTorque  = brakeForce;
            rearLeft.motorTorque   = 0f;
            rearRight.motorTorque  = 0f;
        }
        else
        {
            // Release brakes
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
        isOccupied    = true;
        currentDriver = driver;
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
}
