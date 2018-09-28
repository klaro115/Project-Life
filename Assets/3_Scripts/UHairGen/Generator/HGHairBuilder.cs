using System.Collections;
using UnityEngine;

namespace UHairGen
{
	public static class HGHairBuilder
	{
		#region Fields

		private static int sCounter = 0;
		private static int vCounter = 0;
		private static int tCounter = 0;
		private static HGStrand[] strands = null;
		private static Vector3[] verts = null;
		private static int[] indices = null;
		private static Vector2[] uvs = null;
		private static Color[] colors = null;

		#endregion
		#region Methods

		public static void clear()
		{
			// Drop all references to our mesh buffer arrays, so the GC can dispose of them:
			strands = null;
			verts = null;
			indices = null;
			uvs = null;
			colors = null;

			// Reset all counters, just in case:
			sCounter = 0;
			vCounter = 0;
			tCounter = 0;
		}

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
			tCounter = 0;

			// Build the actual mesh:
			for (int r = 0; r < ringCount; ++r)
			{
				float rAngle = r * ringAngle;
				buildRingStrandData(rAngle, segmentCount, segAngle, hair);
			}


			// TODO: Build mesh data following to the previously generated strands and this head's dimensions.

			// TODO: Generate splines, structures and anchor points along which to construct strand mesh data.

			// TODO: Write generated mesh data to static mesh buffers. Keep track of entry counters.


			// Create a new mesh object:
			Mesh mesh = new Mesh();


			// TODO: Write hair stand mesh structures to mesh:


			return mesh;
		}

		private static void buildRingStrandData(float rAngle, int maxStrandCount, float segAngle, HGHair hair)
		{
			if (hair == null || hair.regions == null || hair.regions.Length == 0 || maxStrandCount <= 0) return;

			// Calculate the actual optimal number of strands to generate for this ring:
			float angularStrandAmount = Mathf.Sin(rAngle);
			int strandCount = Mathf.Clamp(Mathf.RoundToInt(maxStrandCount * angularStrandAmount), 1, maxStrandCount);
			float invStrandCount = 1.0f / (float)strandCount;
			// NOTE: Ring radius changes with inclination angle, so adjusting numbers will keep hair density constant across the mesh.

			// Process some random attributes pertaining to all hair strands:
			float lengthModifier = 1.0f + Random.Range(-hair.lengthRandom, hair.lengthRandom);

			// Generate placeholder objects containing the essential data of each strand first:
			int regionIndex = 0;
			for (int x = 0; x < strandCount; ++x)
			{
				// Interpolate base hair data from hair regions, based on current radial coordinate:
				float posX = x * invStrandCount;
				HGRegion region = hair.lerpRegions(posX, ref regionIndex);

				// Check angular minimum elevation prior to placing any hair strands:
				if (rAngle > region.y * Mathf.PI) continue;

				// Calculate strand length and skip any strands that are below the minimum length threshold:
				float length = region.length.getRandom() * lengthModifier;
				if (length < hair.minLengthThreshold) continue;


				// TODO: Build strand placeholder objects for each hair strand, so we can later construct the mesh more directly.
			}
		}

		/// <summary>
		/// Verify wether the currently allocated static buffers for mesh generation are sufficiently large.
		/// This will then reallocate larger buffer arrays if length is insufficient.
		/// </summary>
		/// <param name="segCount">Number of hair segments to generate on the hair body's broadest circumference ring.</param>
		/// <param name="ringCount">Number of rings along which to generate hair strands.</param>
		/// <param name="hair">The target hair style to prepare buffers for.</param>
		private static void verifyStaticMeshBuffers(int segCount, int ringCount, HGHair hair)
		{
			if (hair == null) return;

			// Get the maximum possible length of a singular hair strand for this hair style:
			float maxStrandLength = 0.0f;
			if(hair.regions != null && hair.regions.Length > 0)
			{
				for (int i = 0; i < hair.regions.Length; ++i)
					maxStrandLength = Mathf.Max(hair.regions[i].length.max, maxStrandLength);
			}
			maxStrandLength *= hair.lengthRandom;
			if (maxStrandLength < hair.minLengthThreshold)
			{
				maxStrandLength = 0.0f;
			}

			// Calculate the number of strands, verts, triangles, etc. required for constructing the given hair style:
			int maxQuadsPerStrand = Mathf.CeilToInt(maxStrandLength / hair.segmentLength);
			int maxStrandCount = ringCount * segCount;
			int maxQuadCount = maxStrandCount * maxQuadsPerStrand;
			int maxVertCount = maxQuadCount * 2 + 2 * maxStrandCount;
			int maxTrisCount = maxQuadCount * 2;
			int maxIndexCount = maxTrisCount * 3;

			// Check if the existing static buffers are sufficiently large to accomodate this maximum amount of data:
			if (strands == null || strands.Length < maxStrandCount) strands = new HGStrand[maxStrandCount];
			if (verts == null || verts.Length < maxVertCount) verts = new Vector3[maxVertCount];
			if (uvs == null || uvs.Length < maxVertCount) uvs = new Vector2[maxVertCount];
			if (colors == null || colors.Length < maxVertCount) colors = new Color[maxVertCount];
			if (indices == null || indices.Length < maxIndexCount) indices = new int[maxIndexCount];

			// Reset contents on all static object and mesh buffers:
			HGStrand defaultStrand = HGStrand.Default;
			for(int i = 0; i < maxStrandCount; ++i)
			{
				strands[i] = defaultStrand;
			}
			Color defaultColor = new Color(1, 1, 1, 1);
			for (int i = 0; i < maxVertCount; ++i)
			{
				verts[i] = new Vector3();
				uvs[i] = new Vector2();
				colors[i] = defaultColor;
			}
			for(int i = 0; i < maxIndexCount; ++i)
			{
				indices[i] = 0;
			}
		}

		//...

		#endregion
	}
}
