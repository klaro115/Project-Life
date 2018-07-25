using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
}
