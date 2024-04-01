using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[Tooltip("Acceleration in m/s^2.")]
	public float acceleration = 2.0f;
	[Range(0, 1)]
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

	[Header("Look Sensitivity")]
	[Range(0.0f, 2.0f)]
	public float mouseSensetivity = 1.0f;
	[Range(0.0f, 2.0f)]
	public float controllerSensetivity = 1.0f;

	private CharacterController character;
	private Transform virtualCamera;
	private Vector3 velocity;
	private Vector2 inputMove;
	private bool shouldJump = false;

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
		virtualCamera = transform.Find("FreeLook Camera");
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		Vector3 forward = Vector3.Scale(transform.position - virtualCamera.position, new Vector3(1,0,1)).normalized;
		Vector3 playerInputForce = ((forward * inputMove.y) + (Vector3.Cross(Vector3.up, forward) * inputMove.x)) * acceleration * Time.fixedDeltaTime;
		velocity += playerInputForce;

		if (character.isGrounded)
		{
			float speed = velocity.magnitude;

			velocity = Vector3.MoveTowards(velocity, Vector3.zero, speed * scalarFriction + speed * constantFriction * Time.fixedDeltaTime);
		}

		velocity += gravity * Time.fixedDeltaTime * Vector3.down;

		if (character.isGrounded)
		{
			velocity.y = 0;
		}

		if (character.isGrounded && shouldJump)
		{
			velocity += Vector3.up * jumpImpulse;
			shouldJump = false;
		}

		character.Move(velocity);
	}

	public void OnMove(InputValue input)
	{
		inputMove = input.Get<Vector2>();
	}

	public void OnJump()
	{
		shouldJump = true;	
	}

	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Vector3 initialVelocity = velocity;
		velocity = Vector3.Reflect(velocity, hit.normal);
		velocity = Vector3.MoveTowards(velocity, (velocity + initialVelocity) * 0.5f, jumpImpulse * bouncines);
	}
}
