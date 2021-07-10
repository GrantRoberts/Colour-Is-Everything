using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	private Vector3 _movementInput = Vector3.zero;

	private Rigidbody _rigid = null;

	[SerializeField] private Transform _gun = null;

	[SerializeField] private Color _texColour = Color.clear;

	[SerializeField] private int _radius = 3;

	private RaycastHit _rayHit = new RaycastHit();

	private Texture2D _flatTexture = null;

	private bool _hitPaintable = false;

	private GOOColourManager.HSVColour _checkingColour = new GOOColourManager.HSVColour();

	[SerializeField] private List<ParticleSystem> _gooParticles = new List<ParticleSystem>();

	public Texture _tex;

	private RenderTexture _rTex;

	[SerializeField] private Camera uvCamera;

	void Awake()
	{
		_mainCamera = Camera.main;
		_rigid = GetComponent<Rigidbody>();

		Cursor.lockState = CursorLockMode.Locked;

		_currentShootingParticle = _gooParticles[0];
		_currentShootingParticle.Stop();
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

		_rigid.AddForce(_movementInput * _rigid.mass, ForceMode.Impulse);

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
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			_currentShootingParticle.Stop();
			_currentShootingParticle = _gooParticles[0];
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			_currentShootingParticle.Stop();
			_currentShootingParticle = _gooParticles[1];
		}

		_hitPaintable = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out _rayHit, 0.55f, 1 << 7);
		if (_hitPaintable)
		{
			Debug.DrawLine(transform.position + Vector3.up * 0.5f, _rayHit.point);
			Renderer rend = _rayHit.collider.gameObject.GetComponent<Renderer>();
			Debug.Log(_rayHit.collider.name);
			RenderTexture tex = rend.material.GetTexture("_MaskTexture") as RenderTexture;

			Texture2D t2d = RTexToT2D(tex);

			_tex = t2d;

			Vector2 pixelUV = _rayHit.textureCoord;
			pixelUV.x *= t2d.width;
			pixelUV.y *= t2d.height;
			Debug.Log(pixelUV);
			
			_texColour = t2d.GetPixel((int)pixelUV.x, (int)pixelUV.y);
			//_checkingColour.value = Vector3.zero;
			//int averageCounter = 0;
			////Debug.Log("--------New Check!--------");
			//for (int y = -_radius; y <= _radius; ++y)
			//{
			//	for (int x = -_radius; x <= _radius; ++x)
			//	{
			//		GOOColourManager.HSVColour tempColour = new GOOColourManager.HSVColour();
			//		int pixelX = (int)pixelUV.x + x;
			//		int pixelY = (int)pixelUV.y + y;
			//		//Debug.Log("_______________________");
			//		Color.RGBToHSV(t2d.GetPixel(pixelX, pixelY), out tempColour.value.x, out tempColour.value.y, out tempColour.value.z);
			//		_checkingColour.value.x += tempColour.value.x;
			//		_checkingColour.value.y += tempColour.value.y;
			//		_checkingColour.value.z += tempColour.value.z;
			//		averageCounter++;
			//	}
			//}
			//_checkingColour.value /= averageCounter;
			//_texColour = Color.HSVToRGB(_checkingColour.value.x, _checkingColour.value.y, _checkingColour.value.z);
			//Debug.Log(new Vector3(_texColour.r, _texColour.g, _texColour.b));

			//StartCoroutine("CheckGround");
		}
	}

	private Texture2D RTexToT2D(RenderTexture rTex)
	{
		Texture2D t2d = new Texture2D(rTex.width, rTex.height);
		Graphics.CopyTexture(rTex, 0, 0, t2d, 0, 0);
		//RenderTexture.active = rTex;
		//t2d.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
		//t2d.Apply();
		//RenderTexture.active = null;

		return t2d;
	}

	IEnumerator CheckGround()
	{
		yield return new WaitForEndOfFrame();

		if (_hitPaintable)
		{
			Renderer rend = _rayHit.collider.gameObject.GetComponent<Renderer>();
			Debug.Log(_rayHit.collider.name);
			RenderTexture tex = rend.material.GetTexture("_MaskTexture") as RenderTexture;

			Texture2D t2d = RTexToT2D(tex);

			_tex = t2d;

			//_flatTexture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
			//_flatTexture.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
			//_flatTexture.Apply();


			if (_flatTexture != null)
			{
				//Vector2 pixelUV = _rayHit.textureCoord;
				//Debug.Log(pixelUV);
				//pixelUV.x *= _flatTexture.width;
				//pixelUV.y *= _flatTexture.height;

				//uvCamera.targetTexture = tex;
				//uvCamera.Render();
				//RenderTexture.active = tex;

				//_flatTexture.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
				//RenderTexture.active = null;


				//_texColour = _flatTexture.GetPixel((int)pixelUV.x, (int)pixelUV.y);
				// _checkingColour.value = Vector3.zero;
				// int averageCounter = 0;
				// //Debug.Log("--------New Check!--------");
				// for (int y = -_radius; y <= _radius; ++y)
				// {
				// 	for (int x = -_radius; x <= _radius; ++x)
				// 	{
				// 		GOOColourManager.HSVColour tempColour = new GOOColourManager.HSVColour();
				// 		int pixelX = (int)pixelUV.x + x;
				// 		int pixelY = (int)pixelUV.y + y;
				// 		//Debug.Log("_______________________");
				// 		Color.RGBToHSV(_flatTexture.GetPixel(pixelX, pixelY), out tempColour.value.x, out tempColour.value.y, out tempColour.value.z);
				// 		_checkingColour.value.x += tempColour.value.x;
				// 		_checkingColour.value.y += tempColour.value.y;
				// 		_checkingColour.value.z += tempColour.value.z;
				// 		averageCounter++;
				// 	}
				// }
				// _checkingColour.value /= averageCounter;
				// _texColour = Color.HSVToRGB(_checkingColour.value.x, _checkingColour.value.y, _checkingColour.value.z);
				//Debug.Log(new Vector3(_texColour.r, _texColour.g, _texColour.b));

				GOOColourManager.eGOOColours type = new GOOColourManager.eGOOColours();

				if (GOOColourManager._instance.CheckColour(_texColour, out type))
				{
					//Debug.Log("Standing on goo!");
				}
				//else
				//Debug.Log("Not standing on goo!");
			}
			else
			{
				_texColour = Color.clear;
			}
		}
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