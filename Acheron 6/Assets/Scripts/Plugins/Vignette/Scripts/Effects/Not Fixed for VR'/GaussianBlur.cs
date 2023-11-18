// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

namespace Colorful
{
	using UnityEngine;

	[HelpURL("http://www.thomashourdel.com/colorful/doc/blur-effects/gaussian-blur.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Blur Effects/Gaussian Blur")]
	public class GaussianBlur : BaseEffect
	{
		[Range(0, 10), Tooltip("Amount of blurring pass to apply.")]
		public int Passes = 1;

		[Range(1f, 16f), Tooltip("Downscales the result for faster processing or heavier blur.")]
		public float Downscaling = 1;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (Passes == 0)
				Graphics.Blit(source, destination);
			else if (Passes == 1)
				OnePassBlur(source, destination);
			else
				MultiPassBlur(source, destination);
		}

		protected virtual void OnePassBlur(RenderTexture source, RenderTexture destination)
		{
			int w = Mathf.FloorToInt((float)source.width / Downscaling);
			int h = Mathf.FloorToInt((float)source.height / Downscaling);
			RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, source.format);

			Material.SetVector("_Direction", new Vector2(1f / w, 0f));
			Graphics.Blit(source, rt, Material);
			Material.SetVector("_Direction", new Vector2(0f, 1f / h));
			Graphics.Blit(rt, destination, Material);

			RenderTexture.ReleaseTemporary(rt);
		}

		protected virtual void MultiPassBlur(RenderTexture source, RenderTexture destination)
		{
			int w = Mathf.FloorToInt((float)source.width / Downscaling);
			int h = Mathf.FloorToInt((float)source.height / Downscaling);
			Vector2 horizontal = new Vector2(1f / w, 0f);
			Vector2 vertical = new Vector2(0f, 1f / h);
			RenderTexture rt1 = RenderTexture.GetTemporary(w, h, 0, source.format);
			RenderTexture rt2 = RenderTexture.GetTemporary(w, h, 0, source.format);

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
