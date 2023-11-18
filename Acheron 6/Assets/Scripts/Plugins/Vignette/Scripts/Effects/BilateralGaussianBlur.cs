// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

namespace Colorful
{
	using UnityEngine;

	[HelpURL("http://www.thomashourdel.com/colorful/doc/blur-effects/bilateral-gaussian-blur.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Blur Effects/Bilateral Gaussian Blur")]
	public class BilateralGaussianBlur : BaseEffect
	{
		[Range(0, 10), Tooltip("Add more passes to get a smoother blur. Beware that each pass will slow down the effect.")]
		public int Passes = 1;

		[Range(0.04f, 1f), Tooltip("Adjusts the blur \"sharpness\" around edges")]
		public float Threshold = 0.05f;

		protected override void Start()
		{
			base.Start();
			GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		}

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			Material.SetFloat("_Threshold", Threshold / 10000f);

			if (Passes == 0)
				Graphics.Blit(source, destination);
			else if (Passes == 1)
				OnePassBlur(source, destination);
			else
				MultiPassBlur(source, destination);
		}

		protected virtual void OnePassBlur(RenderTexture source, RenderTexture destination)
		{
			RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

			Material.SetVector("_Direction", new Vector2(1f / (float)source.width, 0f));
			Graphics.Blit(source, rt, Material);
			Material.SetVector("_Direction", new Vector2(0f, 1f / (float)source.height));
			Graphics.Blit(rt, destination, Material);

			RenderTexture.ReleaseTemporary(rt);
		}

		protected virtual void MultiPassBlur(RenderTexture source, RenderTexture destination)
		{
			Vector2 horizontal = new Vector2(1f / (float)source.width, 0f);
			Vector2 vertical = new Vector2(0f, 1f / (float)source.height);
			RenderTexture rt1 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
			RenderTexture rt2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

			Material.SetVector("_Direction", horizontal);
			Graphics.Blit(source, rt1, Material);
			Material.SetVector("_Direction", vertical);
			Graphics.Blit(rt1, rt2, Material);

			for (int i = 1; i < Passes; i++)
			{
				Material.SetVector("_Direction", horizontal);
				Graphics.Blit(rt2, rt1, Material);
				Material.SetVector("_Direction", vertical);
				Graphics.Blit(rt1, rt2, Material);
			}

			Graphics.Blit(rt2, destination);

			RenderTexture.ReleaseTemporary(rt1);
			RenderTexture.ReleaseTemporary(rt2);
		}
	}
}
