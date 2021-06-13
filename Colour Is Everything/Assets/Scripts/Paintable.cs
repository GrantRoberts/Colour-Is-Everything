using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paintable : MonoBehaviour
{
	const int TEXTURE_SIZE = 1024;

	[SerializeField] private float _extendsIslandOffset = 1;

	[SerializeField] private GameObject _gridObject = null;

	private RenderTexture _extendIslandsRenderTexture;
	private RenderTexture _uvIslandsRenderTexture;
	private RenderTexture _maskRenderTexture;
	private RenderTexture _supportTexture;

	private Renderer _rend;

	private int _maskTextureID = Shader.PropertyToID("_MaskTexture");

	public RenderTexture GetMask() => _maskRenderTexture;
	public RenderTexture GetUVIslands() => _uvIslandsRenderTexture;
	public RenderTexture GetExtend() => _extendIslandsRenderTexture;
	public RenderTexture GetSupport() => _supportTexture;
	public float GetExtendsIslandOffset() => _extendsIslandOffset;
	public Renderer GetRenderer() => _rend;

	void Start() 
	{
		_maskRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
		_maskRenderTexture.filterMode = FilterMode.Bilinear;

		_extendIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
		_extendIslandsRenderTexture.filterMode = FilterMode.Bilinear;

		_uvIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
		_uvIslandsRenderTexture.filterMode = FilterMode.Bilinear;

		_supportTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
		_supportTexture.filterMode = FilterMode.Bilinear;

		_rend = GetComponent<Renderer>();
		_rend.material.SetTexture(_maskTextureID, _extendIslandsRenderTexture);

		PaintManager.GetInstance().InitTextures(this);

		// Need to get this to spawn these blocks all over the paintable surface.
		// Could do this by using the collider - spawning tiles all along the outside, but I'm too smooth brain to know how to do that☹️
		BoxCollider boxCollider = GetComponent<BoxCollider>();

		Instantiate(_gridObject, transform.TransformPoint(boxCollider.center) + (Vector3.up * (boxCollider.size.y / 2)), Quaternion.identity, transform);
	}

	void OnDisable()
	{
		_maskRenderTexture.Release();
		_uvIslandsRenderTexture.Release();
		_extendIslandsRenderTexture.Release();
		_supportTexture.Release();
	}
}
