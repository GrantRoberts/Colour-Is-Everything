using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PaintManager : MonoBehaviour
{
	[SerializeField] private static PaintManager _instance = null;
	[SerializeField] private Shader _texturePaint;
	[SerializeField] private Shader _extendIslands;

	private int _prepareUVID = Shader.PropertyToID("_PrepareUV");
	private int _positionID = Shader.PropertyToID("_PainterPosition");
	private int _hardnessID = Shader.PropertyToID("_Hardness");
	private int _strengthID = Shader.PropertyToID("_Strength");
	private int _radiusID = Shader.PropertyToID("_Radius");
	private int _blendOpID = Shader.PropertyToID("_BlendOp");
	private int _colorID = Shader.PropertyToID("_PainterColor");
	private int _textureID = Shader.PropertyToID("_MainTex");
	private int _uvOffsetID = Shader.PropertyToID("_OffsetUV");
	private int _uvIslandsID = Shader.PropertyToID("_UVIslands");

	private Material _paintMaterial;
	private Material _extendMaterial;

	private CommandBuffer _command;

	public void Awake()
	{
		_instance = this;
		_paintMaterial = new Material(_texturePaint);
		_extendMaterial = new Material(_extendIslands);
		_command = new CommandBuffer();
		_command.name = "CommmandBuffer - " + gameObject.name;
	}

	public void InitTextures(Paintable paintable)
	{
		RenderTexture mask = paintable.GetMask();
		RenderTexture uvIslands = paintable.GetUVIslands();
		RenderTexture extend = paintable.GetExtend();
		RenderTexture support = paintable.GetSupport();
		Renderer rend = paintable.GetRenderer();

		_command.SetRenderTarget(mask);
		_command.SetRenderTarget(extend);
		_command.SetRenderTarget(support);

		_paintMaterial.SetFloat(_prepareUVID, 1);
		_command.SetRenderTarget(uvIslands);
		_command.DrawRenderer(rend, _paintMaterial, 0);

		Graphics.ExecuteCommandBuffer(_command);
		_command.Clear();
	}

	public void Paint(Paintable paintable, Vector3 pos, float radius = 1f, float hardness = .5f, float strength = .5f, Color? colour = null)
	{
		RenderTexture mask = paintable.GetMask();
		RenderTexture uvIslands = paintable.GetUVIslands();
		RenderTexture extend = paintable.GetExtend();
		RenderTexture support = paintable.GetSupport();
		Renderer rend = paintable.GetRenderer();

		_paintMaterial.SetFloat(_prepareUVID, 0);
		_paintMaterial.SetVector(_positionID, pos);
		_paintMaterial.SetFloat(_hardnessID, hardness);
		_paintMaterial.SetFloat(_strengthID, strength);
		_paintMaterial.SetFloat(_radiusID, radius);
		_paintMaterial.SetTexture(_textureID, support);
		_paintMaterial.SetColor(_colorID, colour ?? Color.red);
		_extendMaterial.SetFloat(_uvOffsetID, paintable.GetExtendsIslandOffset());
		_extendMaterial.SetTexture(_uvIslandsID, uvIslands);

		_command.SetRenderTarget(mask);
		_command.DrawRenderer(rend, _paintMaterial, 0);

		_command.SetRenderTarget(support);
		_command.Blit(mask, support);

		_command.SetRenderTarget(extend);
		_command.Blit(mask, extend, _extendMaterial);

		Graphics.ExecuteCommandBuffer(_command);
		_command.Clear();
	}

	public static PaintManager GetInstance()
	{
		return _instance;
	}
}