using System;
using System.Collections;
using UnityEngine;

namespace UHairGen
{
	public static class HGHairBuilder
	{
		#region Fields

		private static int vCounter = 0;
		private static int iCounter = 0;
		private static Vector3[] verts = null;
		private static int[] indices = null;
		private static Vector2[] uvs = null;
		private static Color[] colors = null;

		#endregion
		#region Methods

		public static Mesh build(HGHair hair)
		{
			// Verify input settings:
			if(hair == null || hair.regions == null || hair.regions.Length == 0)
			{
				Debug.LogError("[HGHairGenerator] Error! Unable to create mesh from null hair style!");
				return null;
			}

			// Determine the lowest 
			float maxY = 0.0f;
			for (int i = 0; i < hair.regions.Length; ++i)
				maxY = Mathf.Max(hair.regions[i].y, maxY);

			// Determine over how many rings and segments hair strands may be generated:
			int ringCount = Mathf.Clamp(Mathf.RoundToInt(maxY / hair.ringSpacing), 1, 100);
			int segmentCount = Mathf.Clamp(Mathf.RoundToInt(1.0f / hair.segmentWidth), 1, 200);
			float ringAngle = Mathf.PI / ringCount;
			float segAngle = 2.0f * Mathf.PI / segmentCount;

			// Make sure the static mesh buffer is sufficiently large:
			verifyStaticMeshBuffers(segmentCount, ringCount, hair);
			vCounter = 0;
			iCounter = 0;

			// Build the actual mesh:
			for (int r = 0; r < ringCount; ++r)
			{
				float rAngle = r * ringAngle;
				buildHairRing(rAngle, segmentCount, segAngle, hair);
			}


			// Create a new mesh object:
			Mesh mesh = new Mesh();

			// TODO: Write hair stand mesh structures to mesh:

			return mesh;
		}

		private static void verifyStaticMeshBuffers(int segCount, int ringCount, HGHair hair)
		{
			if (hair == null) return;


		}

		private static void buildHairRing(float rAngle, int segCount, float segAngle, HGHair hair)
		{
			if (hair == null || segCount <= 0) return;

			// TODO: Build mesh data according to the given hair object's settings and this head's dimensions.

			// TODO: Write generated mesh data to static mesh buffers. Keep track of entry counters.
		}

		//...

		#endregion
	}
}
