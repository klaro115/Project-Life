﻿using System.Collections;
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

		public static Mesh build(HGHair hair, float bodyRadius = 1.0f)
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
			float circumference = 2.0f * Mathf.PI * bodyRadius;
			int ringCount = Mathf.Clamp(Mathf.RoundToInt(maxY / hair.ringSpacing), 1, 100);
			int segmentCount = Mathf.Clamp(Mathf.RoundToInt(circumference / hair.segmentWidth), 1, 200);
			float segAngle = 2.0f * Mathf.PI / segmentCount;

			// Process some random attributes pertaining to all hair strands:
			float lengthModifier = 1.0f + Random.Range(-hair.lengthRandom, hair.lengthRandom);

			// Make sure the static mesh buffer is sufficiently large:
			verifyStaticMeshBuffers(segmentCount, ringCount, lengthModifier, hair);
			sCounter = 0;
			vCounter = 0;
			tCounter = 0;

			// Build the actual mesh:
			for (int r = 0; r < ringCount; ++r)
			{
				float ringY = maxY * ((float)r / ringCount);
				buildRingStrandData(ringY, segmentCount, segAngle, lengthModifier, hair);
			}

			// Construct all-linear base geometry for each hair strand:
			for(int i = 0; i < sCounter; ++i)
			{
				buildStrandGeometry(ref strands[i], hair, bodyRadius);
			}


			// TODO: Generate splines, structures and anchor points along which to deform strand geometry.

			// TODO: Write generated mesh data to static mesh buffers. Keep track of entry counters.


			// Create a new mesh object:
			Mesh mesh = new Mesh();

			// Assemble mesh from static buffer contents:
			Vector3[] meshVerts = new Vector3[vCounter];
			Vector2[] meshUVs = new Vector2[vCounter];
			Color[] meshColors = new Color[vCounter];
			for (int i = 0; i < meshVerts.Length; ++i)
			{
				meshVerts[i] = verts[i];
				meshUVs[i] = uvs[i];
				meshColors[i] = colors[i];
			}
			int[] meshIndices = new int[tCounter];
			for (int i = 0; i < meshIndices.Length; ++i)
			{
				meshIndices[i] = indices[i];
			}

			mesh.vertices = meshVerts;
			mesh.triangles = meshIndices;
			mesh.uv = meshUVs;
			mesh.colors = meshColors;
			
			// Recalculate mesh normals and the likes:
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();

			return mesh;
		}

		private static void buildRingStrandData(float posY, int maxStrandCount, float segAngle, float lengthModifier, HGHair hair)
		{
			// Don't create strands if there is no hair on this ring:
			if (hair == null || hair.regions == null || hair.regions.Length == 0 || maxStrandCount <= 0) return;

			// Calculate the actual optimal number of strands to generate for this ring:
			float angularStrandAmount = Mathf.Sin(posY * Mathf.PI);
			int strandCount = Mathf.Clamp(Mathf.RoundToInt(maxStrandCount * angularStrandAmount), 1, maxStrandCount);
			float invStrandCount = 1.0f / (float)strandCount;
			// NOTE: Ring radius changes with inclination angle, so adjusting numbers will keep hair density constant across the mesh.

			// Generate placeholder objects containing the essential data of each strand first:
			int regionIndex = 0;
			for (int x = 0; x < strandCount; ++x)
			{
				// Interpolate base hair data from hair regions, based on current radial coordinate:
				float posX = x * invStrandCount;
				HGRegion region = hair.lerpRegions(posX, posY, ref regionIndex);

				// Check angular minimum elevation prior to placing any hair strands:
				if (posY > region.y) continue;

				// Calculate strand length and skip any strands that are below the minimum length threshold:
				float length = region.length.getRandom() * lengthModifier;
				if (length < hair.minLengthThreshold) continue;

				// Calculate the number of quads making up this strand:
				int quadCount = Mathf.CeilToInt(length / hair.segmentLength);

				// Determine the growth direction and normal vector of the hair strand's mesh blade:
				float angX = posX * 360.0f;
				float angY = posY * 180.0f;
				Quaternion rotation = Quaternion.Euler(0, angX, 0) * Quaternion.Euler(angY, 0, 0);
				Vector3 originDir = rotation * Vector3.up;
				Vector3 originNorm = rotation * Vector3.forward;

				// Finally, assemble the strand data:
				HGStrand strand = HGStrand.Default;
				strand.x = posX;
				strand.y = posY;
				strand.length = length;
				strand.width = hair.segmentWidth;
				strand.segments = quadCount;
				strand.forward = originDir.normalized;
				strand.normal = originNorm.normalized;

				// Write strand to the buffer and increment strand counter:
				strands[sCounter++] = strand;
			}
		}

		private static void buildStrandGeometry(ref HGStrand strand, HGHair hair, float bodyRadius)
		{
			// Make sure building this strand is even worth the cpu time:
			if (hair == null || strand.length < hair.minLengthThreshold || strand.width <= 0 || strand.segments < 1) return;

			// Get us some reference values for easier access:
			float w = strand.width;
			float l = strand.length;
			float sl0 = hair.segmentLength;
			Vector3 forward = strand.forward;
			Vector3 up = strand.normal;
			Vector3 right = -Vector3.Cross(forward, up); // todo: test if this needs flipping first.
			Vector3 origin = forward * bodyRadius;
			Vector3 segWidth = right * w * 0.5f;
			Color defaultVertexColor = Color.white;

			int vBaseIndex = vCounter;
			int tBaseIndex = tCounter;

			// Create the base vertices:
			verts[vCounter++] = origin - segWidth;
			verts[vCounter++] = origin + segWidth;
			uvs[vBaseIndex] = new Vector2(0, 0);
			uvs[vBaseIndex + 1] = new Vector2(1, 0);
			colors[vBaseIndex] = defaultVertexColor;
			colors[vBaseIndex + 1] = defaultVertexColor;

			// Iterate over the strand's segments/quads and generate geometry:
			for (int i = 0; i < strand.segments; ++i)
			{
				// Adjust length of segments to properly match total strand length:
				l -= sl0;
				float sl = l > 0 ? sl0 : Mathf.Abs(l);
				float curLength = i * sl0 + sl;
				int curIndex = vCounter;

				// Create vertices:
				Vector3 segPos = origin + forward * curLength;
				verts[vCounter++] = segPos - segWidth;
				verts[vCounter++] = segPos + segWidth;

				// Calculate UV coordinates:
				float uvY = Mathf.Clamp01(curLength / l);
				uvs[curIndex] = new Vector2(0.0f, uvY);
				uvs[curIndex + 1] = new Vector2(1.0f, uvY);

				// Set placeholder vertex colors:
				colors[curIndex] = defaultVertexColor;
				colors[curIndex + 1] = defaultVertexColor;

				// Create triangles:
				indices[tCounter++] = curIndex;
				indices[tCounter++] = curIndex - 1;
				indices[tCounter++] = curIndex + 1;
				indices[tCounter++] = curIndex - 2;
				indices[tCounter++] = curIndex - 1;
				indices[tCounter++] = curIndex;
			}

			// Memorize geometry indices in strand for later modulation and deformation:
			strand.vIndexStart = vBaseIndex;
			strand.vIndexCount = vCounter - vBaseIndex;
			strand.tIndexStart = tBaseIndex;
			strand.tIndexCount = tCounter - tBaseIndex;
		}

		/// <summary>
		/// Verify wether the currently allocated static buffers for mesh generation are sufficiently large.
		/// This will then reallocate larger buffer arrays if length is insufficient.
		/// </summary>
		/// <param name="segCount">Number of hair segments to generate on the hair body's broadest circumference ring.</param>
		/// <param name="ringCount">Number of rings along which to generate hair strands.</param>
		/// <param name="hair">The target hair style to prepare buffers for.</param>
		private static void verifyStaticMeshBuffers(int segCount, int ringCount, float lengthModifier, HGHair hair)
		{
			if (hair == null) return;

			// Get the maximum possible length of a singular hair strand for this hair style:
			float maxStrandLength = 0.0f;
			if(hair.regions != null)
			{
				for (int i = 0; i < hair.regions.Length; ++i)
					maxStrandLength = Mathf.Max(hair.regions[i].length.max, maxStrandLength);
			}
			maxStrandLength *= lengthModifier;
			if (maxStrandLength < hair.minLengthThreshold)
			{
				Debug.Log("TEST: Length below threshold. Nope.");
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

		#endregion
	}
}
