// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

namespace Colorful
{
	using UnityEngine;

	[HelpURL("http://www.thomashourdel.com/colorful/doc/color-correction/lookup-filter-3d.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Color Correction/Lookup Filter 3D (Color Grading)")]
	public class LookupFilter3D : BaseEffect
	{
		[Tooltip("The lookup texture to apply. Read the documentation to learn how to create one.")]
		public Texture2D LookupTexture;

		[Range(0f, 1f), Tooltip("Blending factor.")]
		public float Amount = 1f;

		protected Texture3D m_Lut3D;
		protected string m_BaseTextureName;

		protected override void Start()
		{
			base.Start();

			// We need 3D textures support
			if (!SystemInfo.supports3DTextures)
			{
				Debug.LogWarning("3D textures aren't supported on this device");
				enabled = false;
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			if (m_Lut3D)
				DestroyImmediate(m_Lut3D);
		}

		protected virtual void Reset()
		{
			m_BaseTextureName = null;
		}

		protected void SetIdentityLut()
		{
			int dim = 16;
			Color[] newC = new Color[dim * dim * dim];
			float oneOverDim = 1.0f / (1.0f * dim - 1.0f);

			for (int i = 0; i < dim; i++)
			{
				for (int j = 0; j < dim; j++)
				{
					for (int k = 0; k < dim; k++)
					{
						newC[i + (j * dim) + (k * dim * dim)] = new Color((i * 1.0f) * oneOverDim, (j * 1.0f) * oneOverDim, (k * 1.0f) * oneOverDim, 1.0f);
					}
				}
			}

			if (m_Lut3D)
				DestroyImmediate(m_Lut3D);

			m_Lut3D = new Texture3D(dim, dim, dim, TextureFormat.ARGB32, false);
			m_Lut3D.hideFlags = HideFlags.HideAndDontSave;
			m_Lut3D.SetPixels(newC);
			m_Lut3D.Apply();
			m_BaseTextureName = "";
		}

		public bool ValidDimensions(Texture2D tex2D)
		{
			if (tex2D == null || tex2D.height != Mathf.FloorToInt(Mathf.Sqrt(tex2D.width)))
				return false;

			return true;
		}

		protected void ConvertBaseTexture()
		{
			if (!ValidDimensions(LookupTexture))
			{
				Debug.LogWarning("The given 2D texture " + LookupTexture.name + " cannot be used as a 3D LUT. Pick another texture or adjust dimension to e.g. 256x16.");
				return;
			}

			m_BaseTextureName = LookupTexture.name;

			int dim = LookupTexture.height;

			Color[] c = LookupTexture.GetPixels();
			Color[] newC = new Color[c.Length];

			for (int i = 0; i < dim; i++)
			{
				for (int j = 0; j < dim; j++)
				{
					for (int k = 0; k < dim; k++)
					{
						int j_ = dim - j - 1;
						newC[i + (j * dim) + (k * dim * dim)] = c[k * dim + i + j_ * dim * dim];
					}
				}
			}

			if (m_Lut3D)
				DestroyImmediate(m_Lut3D);

			m_Lut3D = new Texture3D(dim, dim, dim, TextureFormat.ARGB32, false);
			m_Lut3D.hideFlags = HideFlags.HideAndDontSave;
			m_Lut3D.wrapMode = TextureWrapMode.Clamp;
			m_Lut3D.SetPixels(newC);
			m_Lut3D.Apply();
		}

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (LookupTexture == null || Amount <= 0f)
			{
				Graphics.Blit(source, destination);
				return;
			}

			if (LookupTexture.name != m_BaseTextureName)
				ConvertBaseTexture();

			if (m_Lut3D == null)
				SetIdentityLut();

			Material.SetTexture("_LookupTex", m_Lut3D);
			float lutSize = (float)m_Lut3D.width;
			Material.SetVector("_Params", new Vector3(
					(lutSize - 1f) / (1f * lutSize),
					1f / (2f * lutSize),
					Amount
				));

			Graphics.Blit(source, destination, Material, CLib.IsLinearColorSpace() ? 1 : 0);
		}
	}
}
