using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[AddComponentMenu("Scripts/Misc/Cubemapper")]
[RequireComponent(typeof(Camera))]
public class Cubemapper : MonoBehaviour
{
	public int imageSize = 4096;
	public string fileName = "cubeFace";

	[ContextMenu("Render Cubemap")]
	public void renderToCubemap()
	{
		Camera cam = GetComponent<Camera>();
		Cubemap cube = new Cubemap(imageSize, TextureFormat.RGBA32, false);

		cam.RenderToCubemap(cube);

		for(int i = 0; i < 6; ++i)
		{
			CubemapFace face = (CubemapFace)i;
			Color[] pixels = cube.GetPixels(face);

			Debug.Log("Writing face " + i + ": " + face.ToString());

			Texture2D tex = new Texture2D(imageSize, imageSize, TextureFormat.RGBA32, false);
			tex.SetPixels(pixels);
			tex.Apply();

			byte[] pngBytes = tex.EncodeToPNG();

			using (BinaryWriter writer = new BinaryWriter(File.Open(fileName + "_" + face.ToString() + ".png", FileMode.OpenOrCreate)))
			{
				writer.Write(pngBytes);
			}
		}
	}

	[ContextMenu("Render View")]
	public void renderToTexture()
	{
		Camera cam = GetComponent<Camera>();
		RenderTexture renderTex = new RenderTexture(imageSize, imageSize, 24, RenderTextureFormat.ARGB32);

		cam.targetTexture = renderTex;
		cam.Render();
		RenderTexture.active = renderTex;

		Texture2D tex = new Texture2D(imageSize, imageSize, TextureFormat.RGBA32, false);
		tex.ReadPixels(new Rect(0, 0, imageSize, imageSize), 0, 0);
		tex.Apply();

		cam.targetTexture = null;

		byte[] pngBytes = tex.EncodeToPNG();

		using (BinaryWriter writer = new BinaryWriter(File.Open(fileName + "_render.png", FileMode.OpenOrCreate)))
		{
			writer.Write(pngBytes);
		}
	}
}
