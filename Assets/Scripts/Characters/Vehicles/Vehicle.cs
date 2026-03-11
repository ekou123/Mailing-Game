using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Vehicle : MonoBehaviour
{
    [Header("Vehicle Object")]
    public Transform cameraTarget;

    [Header("Stats")]
    public float motorForce = 1500f;
    public float brakeForce = 3000f;
    public float maxSteerAngle = 35f;

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
    public Transform driverSeat;       // where the player sits
    public Transform exitPoint;        // where they spawn on exit

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public bool isOccupied;
    [HideInInspector] public Character currentDriver;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f); // lower = more stable
    }

    public void ApplyInput(float accel, float steer, bool handbrake)
    {
        float motor = motorForce * accel;
        float brake = handbrake ? brakeForce : 0f;
        float steerAngle = maxSteerAngle * steer;

        // Rear-wheel drive
        rearLeft.motorTorque  = motor;
        rearRight.motorTorque = motor;

        // Front-wheel steering
        frontLeft.steerAngle  = steerAngle;
        frontRight.steerAngle = steerAngle;

        // Braking
        if (accel < 0f)
        {
            // Reverse / brake
            rearLeft.brakeTorque  = brakeForce;
            rearRight.brakeTorque = brakeForce;
            rearLeft.motorTorque  = 0f;
            rearRight.motorTorque = 0f;

            rearLeft.motorTorque  = motorForce * accel;
            rearRight.motorTorque = motorForce * accel;
        }
        else
        {
            rearLeft.brakeTorque  = brake;
            rearRight.brakeTorque = brake;
        }

        UpdateWheelMesh(frontLeft,  frontLeftMesh);
        UpdateWheelMesh(frontRight, frontRightMesh);
        UpdateWheelMesh(rearLeft,   rearLeftMesh);
        UpdateWheelMesh(rearRight,  rearRightMesh);
    }

    private void UpdateWheelMesh(WheelCollider col, Transform mesh)
    {
        if (mesh == null) return;
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.SetPositionAndRotation(pos, rot);
    }

    public void OnDriverEnter(Character driver)
    {
        isOccupied   = true;
        currentDriver = driver;
    }

    public void OnDriverExit()
    {
        isOccupied    = false;
        currentDriver = null;

        // Kill any remaining torque
        foreach (var wc in new[] { frontLeft, frontRight, rearLeft, rearRight })
        {
            wc.motorTorque = 0f;
            wc.brakeTorque = brakeForce;
        }
    }
}