using System.Collections;
using UnityEngine;

namespace UHairGen
{
	public static class HGHairBuilder
	{
		#region Types

		public struct AnchorState
		{
			public int prevIndex;
			public int curIndex;
			public int splineIndex;
			public HGAnchor anchor;
		}
		public struct AnchorSettings
		{
			public float bodyRadius;
			public bool isWeighted;
			public bool singleAnchor;
		}

		#endregion
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
			// Drop all strands' node lists:
			if(strands != null)
			{
				for (int i = 0; i < strands.Length; ++i)
					strands[i].nodes = null;
			}

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

			// Build hair strand data on a per-ring basis:
			for (int r = 0; r < ringCount; ++r)
			{
				float ringY = maxY * ((float)r / ringCount);
				buildRingStrandData(ringY, segmentCount, segAngle, lengthModifier, bodyRadius, hair);
			}

			// Apply splines and anchor points to deform strand nodes:
			for(int i = 0; i < sCounter; ++i)
			{
				applyStrandModifiers(ref strands[i], hair, bodyRadius);
			}

			// Construct all-linear base geometry for each hair strand:
			for (int i = 0; i < sCounter; ++i)
			{
				buildStrandGeometry(ref strands[i], hair, bodyRadius);	// >>> TEMP / TODO: Change method to build geometry along the strand's nodes instead!!! <<<
			}


			// TODO: Properly generate mesh data using static mesh buffers from the strands' nodes. Keep track of entry counters.


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

		private static void buildRingStrandData(float posY, int maxStrandCount, float segAngle, float lengthModifier, float bodyRadius, HGHair hair)
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
				Quaternion originRot = Quaternion.Euler(0, angX, 0) * Quaternion.Euler(angY, 0, 0);
				Vector3 originDir = (originRot * Vector3.up).normalized;
				Vector3 originNorm = (originRot * Vector3.forward).normalized;
				Vector3 originUp = Vector3.Cross(originDir, Vector3.Cross(originDir, originNorm).normalized).normalized;
				Vector3 originPos = hair.meshOffset + originDir * bodyRadius;

				// Finally, assemble the strand data:
				int strandIndex = sCounter;
				HGStrand strand = strands[strandIndex];
				strand.x = posX;
				strand.y = posY;
				strand.length = length;
				strand.width = hair.segmentWidth;
				strand.segments = quadCount;

				// Set all strand nodes to simple follow a straight line orthogonal along the strand's growth direction:
				HGNode[] nodes = strand.nodes;
				float nodeTangent = 0.333f * hair.segmentLength; // note: only fill in a placeholder value.
				for (int i = 0; i < nodes.Length; ++i)
				{
					Vector3 nodePos = originPos + originDir * i * hair.segmentLength;
					nodes[i] = new HGNode(nodePos, originRot, nodeTangent);
				}
				strand.nodeCount = quadCount + 1;

				// Write strand to the buffer and increment strand counter:
				strands[sCounter++] = strand;
			}
		}

		private static void applyStrandModifiers(ref HGStrand strand, HGHair hair, float inBodyRadius)
		{
			// Verify some parameters and stuff:
			if (hair == null || strand.length < hair.minLengthThreshold || strand.width <= 0 || strand.segments < 1) return;

			AnchorState state = new AnchorState() { prevIndex = -1, curIndex = -1, splineIndex = -1 };
			AnchorSettings settings = new AnchorSettings() { bodyRadius = inBodyRadius, isWeighted = false, singleAnchor = true };
			for (int i = 0; i < strand.nodeCount; ++i)
			{
				//HGNode prevNode = strand.nodes[Mathf.Max(i - 1, 0)];
				applyNodeModifiers(ref strand, ref strand.nodes[i], i, ref state, hair, settings);
			}

			// Apply gravity to lower, potentially freely dangling section of the strand:
			int freeDanglingIndex = Mathf.Max(state.prevIndex + 1, 1);
			if(freeDanglingIndex < strand.nodeCount - 1)
			{
				//Debug.Log("Applying gravity to strand at " + strand.x + "," + strand.y + " => Loose node index: " + freeDanglingIndex);
				applyStrandEndGravity(ref strand, freeDanglingIndex, hair, inBodyRadius);
			}
		}

		private static void applyNodeModifiers(ref HGStrand strand, ref HGNode node, int nodeIndex, ref AnchorState state, HGHair hair, AnchorSettings settings)
		{
			HGAnchor[] anchors = hair.anchors;

			// If no anchor is currently acting upon this node, try finding one that does:			//TEMP: disabled for testing.
			if(state.curIndex < 0 && !(settings.singleAnchor && strand.isAnchored))
			{
				// Figure out which anchor - if any - this strand is attached to:
				int targetAnchorIndex = -1;
				float targetAnchorWeight = 0.0f;
				if (anchors != null)
				{
					// Chose anchor by calculating a weighting for each one and taking the highest weighted anchor:
					for (int i = 0; i < anchors.Length; ++i)
					{
						// Don't allow strands to hook onto the same anchor two times in a row in immediate succession:
						if (i == state.prevIndex) continue;
	
						// Calculate weighting based on distance between the node and the anchor:
						HGAnchor anchor = anchors[i];
						Vector3 anchorPos = HGMath.getRadialDirection(anchor.x, anchor.y) * settings.bodyRadius;
						float anchorRadiusSq = anchor.pullRadius * anchor.pullRadius;
						float distSq = Vector3.SqrMagnitude(anchorPos - node.position);

						// If the node is right within the active region of an anchor, use that right away:
						if (distSq < anchor.centerRadius * anchor.centerRadius)
						{
							targetAnchorIndex = i;
							break;
						}

						// Otherwise select based on weighting:
						float weight = distSq < anchorRadiusSq ? 1.0f / distSq : 0;
						if (weight > targetAnchorWeight)
						{
							targetAnchorIndex = i;
							targetAnchorWeight = weight;

							// When not in weighted mode, drop the search after finding a first match:
							if (!settings.isWeighted) break;
						}
					}
				}
				// Set the most fitting anchor we found as active:
				state.curIndex = targetAnchorIndex;
				if(targetAnchorIndex >= 0) state.anchor = anchors[targetAnchorIndex];
			}

			// If the node is affected by or hooked onto any anchors, apply the anchor's effects to the node.
			if (state.curIndex >= 0)
			{
				HGAnchor anchor = anchors[state.curIndex];
				
				// Mark anchored strands for later vertex-coloring and modulation:
				strand.isAnchored = true;



				// TODO: Reorient/Redirect nodes to flow directly through the anchor.



				// Disconnect strand if the node has passed the anchor behaviour's target space:
				if(nodeIndex == 0)
				{
					HGNode prevNode = strand.nodes[Mathf.Max(nodeIndex - 1, 0)];
					bool disconnectAnchor = false;

					// For point or direction type anchors, check if this node has passed the anchor's target range, and thus exited it:
					if (anchor.type != HGAnchorType.Spline && anchor.checkExitPoint(node, prevNode)) disconnectAnchor = true;
					// For spline type anchors, check if the node has exited the spline's end point:
					else if (anchor.type == HGAnchorType.Spline && anchor.checkExitSpline(node, prevNode)) disconnectAnchor = true;

					if(disconnectAnchor)
					{
						state.prevIndex = state.curIndex;
						state.curIndex = -1;

						// Reorient node flow direction upon exiting a directional type anchor:
						if(anchor.type == HGAnchorType.Directional)
						{
							node.rotation = Quaternion.LookRotation(anchor.exitDirection.normalized, node.rotation * Vector3.forward);
						}
					}
				}
			}

			// Apply this node's pose to all subsequent nodes:
			if(nodeIndex < strand.nodeCount - 1)
			{
				// Calculate the rotation 'difference' between the parent node and the current one:
				HGNode parentNode = strand.nodes[Mathf.Max(nodeIndex - 1, 0)];
				Quaternion parentRotation = parentNode.rotation;
				Quaternion nodeRotation = node.rotation;
				Quaternion deltaRotation = nodeRotation * Quaternion.Inverse(parentRotation);
				Vector3 nodePosition = node.position;
				
				// Rotate all subsequent nodes around the current one:
				for(int i = nodeIndex; i < strand.nodeCount; ++i)
				{
					HGNode childNode = strand.nodes[i];
					Vector3 offset = childNode.position - nodePosition;
					childNode.position = deltaRotation * offset + nodePosition;
					childNode.rotation = nodeRotation;
					strand.nodes[i] = childNode;
				}
			}
		}

		private static void applyStrandEndGravity(ref HGStrand strand, int startIndex, HGHair hair, float bodyRadius)
		{
			// Use first unanchored node to start tracing a bezier spline and use something akin to a ballistic trajectory to determine the curve's end node:
			HGNode startNode = strand.nodes[startIndex];
			Vector3 v0 = startNode.rotation * Vector3.up * startNode.tangent * (0.1f + hair.stiffness);
			float totalDeltaTime = 100;// (strand.length - (startIndex + 1) * strand.length) / Mathf.Max(v0.magnitude, 0.0001f) * 10;

			int stepCount = strand.nodeCount - startIndex;
			float totalLength = strand.length;
			float stepLength = totalLength / strand.nodeCount;
			float deltaTime = totalDeltaTime / Mathf.Max(stepCount, 1.0f);
			float scaleRatio = 1.0f / bodyRadius;   // note: all measurements must be adjusted to match mesh rescaling on the target hair body.
			float gravityAccel = 9.81f * scaleRatio;

			Vector3 prevVel = startNode.rotation * (Vector3.up * startNode.tangent * (0.1f + hair.stiffness));    // TODO: Needs testing, tangent/speed may be excessive!!!
			Vector3 prevPos = startNode.position;

			for (int i = 0; i < stepCount; ++i)
			{
				int index = i + startIndex;
				float segmentLength = stepLength < totalLength ? stepLength : Mathf.Abs(totalLength - stepLength);
				float curLength = segmentLength * scaleRatio;
				totalLength -= stepLength;
				HGNode curNode = strand.nodes[index];

				// Iteratively calculate next nodes' 'velocity' using a ballistic trajectory:
				Vector3 curVel = prevVel - Vector3.up * gravityAccel * deltaTime;

				// Apply upwards curling of loose ends of hair by simply rotating the flow direction, pitching it upwards:
				Vector3 curlAxis = -Vector3.Cross(Vector3.up, Vector3.Scale(prevPos, new Vector3(1, 0, 1))).normalized;
				float curlAmount = hair.curlUp * deltaTime;
				Quaternion curlRotation = Quaternion.AngleAxis(curlAmount, curlAxis);
				curVel = curlRotation * curVel;

				// Calculate position change based on the velocity that was calculated above:
				Vector3 curOffset = curVel * deltaTime;

				// Force-adjust distance between nodes to never differ from the corresponding segment's length:
				Vector3 curOffsetDir = curOffset.normalized;
				Vector3 curPos = prevPos + curOffsetDir * curLength * 4;

				// Apply position and orientation to node:
				curNode.position = curPos;
				curNode.rotation = Quaternion.LookRotation(curVel, curNode.rotation * Vector3.forward);
				strand.nodes[index] = curNode;

				// Update trajectory data for next node:
				prevPos = curPos;
				prevVel = curVel;
			}
			// NOTE: I'm iterating along the function rather then parametrizing along its length because them maths are kinda hard and I don't trust my results.
			// There is a chance that this negatively affects runtime performance, but right now, the main priority is to just get the system running reliably.


			// TODO: Check for intersections/collisions with hair body!!!
		}

		private static void buildStrandGeometry(ref HGStrand strand, HGHair hair, float bodyRadius)
		{
			// Make sure building this strand is even worth the cpu time:
			if (hair == null || strand.length < hair.minLengthThreshold || strand.width <= 0 || strand.segments < 1) return;

			// Get us some reference values for easier access:
			float w = strand.width;
			float l = strand.length;
			float sl0 = hair.segmentLength;
			Color defaultVertexColor = Color.white;

			HGNode originNode = strand.nodes[0];
			Vector3 originPosition = originNode.position;
			Quaternion originRotation = originNode.rotation;

			Vector3 forward = originRotation * Vector3.up;
			Vector3 up = originRotation * Vector3.forward;
			Vector3 right = Vector3.Cross(forward, up);
			Vector3 segWidth = right * w * 0.5f;

			int vBaseIndex = vCounter;
			int tBaseIndex = tCounter;

			// Create the base vertices:
			verts[vCounter++] = originPosition + segWidth;
			verts[vCounter++] = originPosition - segWidth;
			uvs[vBaseIndex] = new Vector2(0, 0);
			uvs[vBaseIndex + 1] = new Vector2(1, 0);
			colors[vBaseIndex] = defaultVertexColor;
			colors[vBaseIndex + 1] = defaultVertexColor;

			// Iterate over the strand's segments/quads and generate geometry:
			for (int i = 0; i < strand.nodeCount; ++i)
			{
				// Get the node we're currently working with:
				HGNode node = strand.nodes[i];
				Quaternion nodeRotation = node.rotation;

				// Apply the node and its parent's transformation:
				forward = nodeRotation * Vector3.up;
				up = nodeRotation * Vector3.forward;
				right = Vector3.Cross(forward, up);
				segWidth = right * w * 0.5f;

				// Adjust length of segments to properly match total strand length:
				l -= sl0;
				float sl = l > 0 ? sl0 : Mathf.Abs(l);
				float curLength = i * sl0 + sl;
				int curIndex = vCounter;

				// Create vertices:
				Vector3 segPos = node.position;
				verts[vCounter++] = segPos - segWidth;
				verts[vCounter++] = segPos + segWidth;

				// Calculate UV coordinates:
				float uvY = Mathf.Clamp01(curLength / l);
				uvs[curIndex] = new Vector2(0.0f, uvY);
				uvs[curIndex + 1] = new Vector2(1.0f, uvY);

				// Set placeholder vertex colors:
				Color col = strand.isAnchored ? Color.red : defaultVertexColor;	//TEST
				colors[curIndex] = col;
				colors[curIndex + 1] = col;

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
			int maxNodeCount = maxQuadsPerStrand + 1;
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
			HGNode defaultNode = new HGNode(Vector3.zero, Quaternion.identity);
			for (int i = 0; i < maxStrandCount; ++i)
			{
				HGNode[] nodes = strands[i].nodes;
				if (nodes == null || nodes.Length < maxNodeCount) nodes = new HGNode[maxNodeCount];
				for(int j = 0; j < nodes.Length; ++j)
				{
					nodes[j] = defaultNode;
				}

				HGStrand blankStrand = HGStrand.Default;
				blankStrand.nodes = nodes;
				strands[i] = blankStrand;
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
