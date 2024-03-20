using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Tooltip("Acceleration in m/s^2.")]
    public float acceleration = 2.0f;
    [Range(0,1)]
    public float bouncines = 0.8f;
    [Header("Friction Parameters")]
    [Tooltip("Friction force applied while on ground in m/s^2.")]
    public float constantFriction = 0.5f;
    [Tooltip("Percentage of the velocity lost to friction every frame.")]
    public float scalarFriction = 0.1f;

    [Header("Jump Parameters")]
    [Range(0.1f, 10.0f)]
    public float jumpHeight = 1.0f;
    [Range(0.1f, 5.0f)]
    public float jumpTime = 2.0f;
    public float gravity = 2.0f;
    public float jumpImpulse = 5.0f;


    private CharacterController character;
    private Vector3 velocity;
    private Vector3 inputForce;

    void OnValidate()
    {
        float fudge = 1.0f / 48.0f;
        gravity = (8.0f * jumpHeight * fudge) / Mathf.Pow(jumpTime, 2);
        jumpImpulse = (4.0f * jumpHeight * fudge) / (jumpTime);
    }
    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        velocity += inputForce;

        if (character.isGrounded && velocity.y < 0.0f)
        {
            velocity.y = 0;
        }

        if (character.isGrounded)
        {
            float speed = velocity.magnitude;

            velocity = Vector3.MoveTowards(velocity, Vector3.zero, speed * scalarFriction + speed * constantFriction * Time.fixedDeltaTime);
        }

        velocity += gravity * Time.fixedDeltaTime * Vector3.down;

        character.Move(velocity);
    }

    public void OnMove(InputValue input)
    {
        Vector2 moveDir = input.Get<Vector2>();
        inputForce = new Vector3(moveDir.x, 0, moveDir.y) * acceleration * Time.fixedDeltaTime;
    }

    public void OnJump()
    {
        if (character.isGrounded)
        {
            velocity += Vector3.up * jumpImpulse;
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Vector3 initialVelocity = velocity;
        velocity = Vector3.Reflect(velocity, hit.normal);
        velocity = Vector3.MoveTowards(velocity, (velocity + initialVelocity) * 0.5f, jumpImpulse * bouncines);
    }
}
