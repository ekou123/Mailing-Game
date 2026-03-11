using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    [SerializeField] private float minGroundNormalY = 0.6f; // ~53 degrees slope limit
    public bool IsGrounded { get; private set; }

    private int groundedFrames;

    private void FixedUpdate()
    {
        // Reset each physics step; collisions will set it back to true
        IsGrounded = groundedFrames > 0;
        groundedFrames = 0;
    }

    private void OnCollisionStay(Collision collision)
    {
        // Any contact with an "up-ish" normal counts as ground
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y >= minGroundNormalY)
            {
                groundedFrames++;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Let FixedUpdate handle reset; no need to do anything here
    }
}