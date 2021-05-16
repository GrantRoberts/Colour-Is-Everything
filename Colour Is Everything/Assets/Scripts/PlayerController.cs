using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private ParticleSystem _particle = null;

	private Camera _mainCamera = null;

	[SerializeField] private float _minPitch = -40.0f;

	[SerializeField] private float _maxPitch = 60.0f;

	private float _yaw = 0.0f;

	private float _pitch = 0.0f;

	[SerializeField] private float _mouseXSensitivity = 2.0f;

	[SerializeField] private float _mouseYSensitivity = 2.0f;

	[SerializeField] private float _moveSpeed = 1.5f;

	private Vector3 _movementInput = Vector3.zero;

	private Rigidbody _rigid = null;

	void Awake()
	{
		_mainCamera = Camera.main;
		_rigid = GetComponent<Rigidbody>();

		Cursor.lockState = CursorLockMode.Locked;
	}

	void FixedUpdate()
	{
		//----------------------------------------------------------------
		// Keyboard movement.
		//----------------------------------------------------------------
		_movementInput = Vector3.zero;
		_rigid.velocity = Vector3.zero;

		_movementInput += transform.right * Input.GetAxis("Horizontal");
		_movementInput += transform.forward * Input.GetAxis("Vertical");

		_movementInput *= _moveSpeed * 100;
		_movementInput *= Time.deltaTime;

		_rigid.AddForce(_movementInput, ForceMode.Impulse);
	}
	void Update()
	{
		//--------------------------------------------------------------------
		// Camera look.
		//--------------------------------------------------------------------
		_yaw = 0.0f;
		_pitch = 0.0f;

		_yaw += Input.GetAxis("Mouse X") * _mouseXSensitivity;
		_pitch -= Input.GetAxis("Mouse Y") * _mouseYSensitivity;

		_pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

		// Rotate the player with the x movement of the mouse.
		transform.localEulerAngles += new Vector3(0, _yaw, 0);
		// Rotate the camera with the y movement of the mouse.
		_mainCamera.transform.localEulerAngles += new Vector3(_pitch, 0, 0);

		// Inputs.
		// If the player presses left click.
		if (Input.GetMouseButtonDown(0))
		{
			Debug.Log("Bang!");
			// Make sure particle isn't currently playing.
			if (!_particle.isPlaying)
				_particle.Play();
		}
	}
}