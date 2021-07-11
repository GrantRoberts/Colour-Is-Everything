using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
	private ParticleSystem _currentShootingParticle = null;

	private Camera _mainCamera = null;

	[SerializeField] private float _minPitch = -40.0f;

	[SerializeField] private float _maxPitch = 60.0f;

	private float _yaw = 0.0f;

	private float _pitch = 0.0f;

	[SerializeField] private float _mouseXSensitivity = 2.0f;

	[SerializeField] private float _mouseYSensitivity = 2.0f;

	[SerializeField] private float _moveSpeed = 1.5f;

	[SerializeField] private float _jumpForce = 3.0f;
	[SerializeField] private float _maxVelocity = 3.0f;
	[SerializeField] private float _maxVelocityLerpSpeed = 2.0f;
	private float _currentMaxVelocity = 0.0f;
	private float _targetMaxVelocity = 0.0f;

	private Vector3 _movementInput = Vector3.zero;

	private Rigidbody _rigid = null;

	[SerializeField] private Transform _gun = null;

	[SerializeField] private Color _texColour = Color.clear;

	private RaycastHit _rayHit = new RaycastHit();

	private bool _hitPaintable = false;

	private GOOColourManager.HSVColour _checkingColour = new GOOColourManager.HSVColour();

	[SerializeField] private List<ParticleSystem> _gooParticles = new List<ParticleSystem>();

	public Texture _tex;

	private Queue<Texture2D> _tex2DQueue = new Queue<Texture2D>();

	private KeyCode[] _gooHotkeys = new KeyCode[4] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

	[SerializeField] private float _speedGOOModifier = 5.0f;
	[SerializeField] private float _bounceGOOModifier = 5.0f;

	private bool _bounceGOOAffected = false;

	private CapsuleCollider _capCollider = null;

	void Awake()
	{
		_mainCamera = Camera.main;
		_rigid = GetComponent<Rigidbody>();

		Cursor.lockState = CursorLockMode.Locked;

		_currentShootingParticle = _gooParticles[0];
		_currentShootingParticle.Stop();

		_currentMaxVelocity = _maxVelocity;
		_targetMaxVelocity = _currentMaxVelocity;

		_capCollider = GetComponent<CapsuleCollider>();
	}

	void FixedUpdate()
	{
		//----------------------------------------------------------------
		// Keyboard movement.
		//----------------------------------------------------------------
		_movementInput = Vector3.zero;

		_movementInput += transform.right * Input.GetAxis("Horizontal");
		_movementInput += transform.forward * Input.GetAxis("Vertical");

		_movementInput *= _moveSpeed * 100;
		_movementInput *= Time.deltaTime;

		if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, 0.8f))
		{
			if (_bounceGOOAffected)
				_bounceGOOAffected = false;

			if (Input.GetKeyDown(KeyCode.Space))
			{
				Debug.Log("Jump!");
				_rigid.velocity += Vector3.up * _jumpForce * Time.fixedDeltaTime;
			}
		}
		else
		{
			_rigid.velocity += Physics.gravity * Time.fixedDeltaTime;
		}

		if (!_bounceGOOAffected)
		{
			_rigid.AddForce(_movementInput * _rigid.mass, ForceMode.Impulse);

			Vector3 tempVelocity = new Vector3(_rigid.velocity.x, 0.0f, _rigid.velocity.z);
			Vector3 lateralMovement = Vector3.ClampMagnitude(tempVelocity, _currentMaxVelocity);
			_rigid.velocity = lateralMovement + Vector3.up * _rigid.velocity.y;
		}
	}

	void Update()
	{
		//----------------------------------------------------------------
		// Camera look.
		//----------------------------------------------------------------
		_yaw = 0.0f;
		_pitch = 0.0f;

		_yaw += Input.GetAxis("Mouse X") * _mouseXSensitivity;
		_pitch -= Input.GetAxis("Mouse Y") * _mouseYSensitivity;

		_pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

		// Rotate the player with the x movement of the mouse.
		transform.localEulerAngles += new Vector3(0, _yaw, 0);

		// Rotate the camera and gun with the y movement of the mouse.
		_mainCamera.transform.localEulerAngles += new Vector3(_pitch, 0, 0);
		_gun.transform.localEulerAngles += new Vector3(0, 0, _pitch);

		// Inputs.
		// If the player presses left click.
		if (Input.GetMouseButton(0))
		{
			//Debug.Log("Bang!");
			//Make sure particle isn't currently playing.
			if (!_currentShootingParticle.isPlaying)
				_currentShootingParticle.Play();
		}
		else
		{
			//Debug.Log("Stop");
			_currentShootingParticle.Stop();
		}
		
		// GOO selection.
		for(int i = 0; i < _gooHotkeys.Length; ++i)
		{
			if (i < _gooParticles.Count)
			{
				if (Input.GetKeyDown(_gooHotkeys[i]))
				{
					_currentShootingParticle = _gooParticles[i];
				}
			}
		}

		_currentMaxVelocity = Mathf.Lerp(_currentMaxVelocity, _targetMaxVelocity, Time.deltaTime * _maxVelocityLerpSpeed);

		_hitPaintable = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out _rayHit, 0.65f, 1 << 7);
		// If the last raycast didn't hit anything, check in the direction the player is moving.
		if (!_hitPaintable)
		{
			_hitPaintable = Physics.Raycast(transform.position + Vector3.up * 0.5f, _rigid.velocity, out _rayHit, 0.65f, 1 << 7);
		}

		if (_hitPaintable)
		{
			Debug.DrawLine(transform.position + Vector3.up * 0.5f, _rayHit.point);
			Renderer rend = _rayHit.collider.gameObject.GetComponent<Renderer>();
			//Debug.Log(_rayHit.collider.name);
			RenderTexture tex = rend.material.GetTexture("_MaskTexture") as RenderTexture;

			Texture2D t2d = RTexToT2D(tex);
			_tex2DQueue.Enqueue(t2d);

			if (_tex2DQueue.Count > 1)
				Destroy(_tex2DQueue.Dequeue());

			_tex = t2d;

			Vector2 pixelUV = _rayHit.textureCoord;
			pixelUV.x *= t2d.width;
			pixelUV.y *= t2d.height;
			//Debug.Log(pixelUV);
			
			_texColour = t2d.GetPixel((int)pixelUV.x, (int)pixelUV.y);
		}
		else
		{
			_texColour = Color.clear;
			_targetMaxVelocity = _maxVelocity;
		}

		GOOColourManager.eGOOColours type;
		if (GOOColourManager._instance.CheckColour(_texColour, out type))
		{
			switch(type)
			{
				case GOOColourManager.eGOOColours.Bounce:
				// Only bounce while player is in the air.
				if (Mathf.Abs(_rigid.velocity.y) > 0.0f)
					_rigid.velocity = Vector3.Reflect(new Vector3
					(
						_rigid.velocity.x * _bounceGOOModifier,
						_jumpForce * Time.fixedDeltaTime,
						_rigid.velocity.z * _bounceGOOModifier
					), _rayHit.normal);
					_bounceGOOAffected = true;
				break;

				case GOOColourManager.eGOOColours.Speed:
				_targetMaxVelocity = _maxVelocity * _speedGOOModifier;
				break;

				case GOOColourManager.eGOOColours.Sticky:
				Debug.Log("Stick");
				break;

				default:
				_targetMaxVelocity = _maxVelocity;
				break;
			}
		}
		else
		{
			_targetMaxVelocity = _maxVelocity;
		}
	}

	private Texture2D RTexToT2D(RenderTexture rTex)
	{
		Texture2D t2d = new Texture2D(rTex.width, rTex.height);
		RenderTexture.active = rTex;
		t2d.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
		t2d.Apply();

		return t2d;
	}

	void OnDrawGizmos()
	{
		if (_hitPaintable)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawCube(_rayHit.point, Vector3.one * 0.2f);
		}
	}
}