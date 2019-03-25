using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralAnim
{
	[System.Serializable]
	public class PAChain
	{
		#region Fields

		[Header("Behaviour")]
		public PAChainTargeting targetingMethod = PAChainTargeting.End;
		private PAEndpoint alternatingLastTarget = null;

		[Header("Endpoints")]
		public PAEndpoint end = null;
		public Transform root = null;

		//[Header("Chain ")]
		[HideInInspector]
		public PAChainJoint[] joints = null;

		#endregion
		#region Methods

		public bool GenerateChain()
		{
			if (end == null || root == null) return false;

			Transform[] transformChain = GetHierarchyToRoot(root, end.transform).ToArray();

			if (transformChain == null || transformChain.Length == 0)
			{
				joints = null;
				return false;
			}

			PAJoint[] jointList = new PAJoint[transformChain.Length];
			int jointCount = 0;
			int transformCount = transformChain.Length;
			for (int i = 0; i < transformChain.Length; ++i)
			{
				Transform trans = transformChain[i];
				PAJoint joint = trans.GetComponent<PAJoint>();
				jointList[i] = joint;
				if (joint != null) jointCount++;
			}

			if(jointCount == 0)
			{
				joints = null;
				return false;
			}

			Transform parentJoint = root;
			PAChainJoint[] chainJoints = new PAChainJoint[transformCount];
			for (int i = 0; i < transformCount; ++i)
			{
				PAJoint joint = jointList[i];
				if (joint != null)
				{
					Transform trans = joint.transform;
					Vector3 hierarchyOffset = parentJoint.InverseTransformPoint(trans.position);
					Quaternion hierarchyRotation = trans.rotation * Quaternion.Inverse(parentJoint.rotation);

					PAChainJoint chainJoint = new PAChainJoint(joint, hierarchyOffset, hierarchyRotation);
					chainJoints[i] = chainJoint;

					parentJoint = joint.transform;
				}
				else chainJoints[i] = PAChainJoint.None;
			}

			joints = chainJoints.Where(o => o.joint != null).ToArray();
			return true;
		}

		#endregion
		#region Methods Targeting

		/*
		private PAEndpoint GetClosestEndpoint(Vector3 targetPoint, float weightShift = 0.0f)
		{
			float distSqStart = Vector3.SqrMagnitude(start.ContactPoint - targetPoint) * (1 + weightShift);
			float distSqEnd = Vector3.SqrMagnitude(end.ContactPoint - targetPoint) * (1 - weightShift);
			return distSqStart < distSqEnd ? start : end;
		}
		private PAEndpoint GetAlternatingEndpoint()
		{
			PAEndpoint alternatingNewTarget = start;
			if (alternatingLastTarget == alternatingNewTarget) alternatingNewTarget = end;
			alternatingLastTarget = alternatingNewTarget;
			return alternatingNewTarget;
		}
		private PAEndpoint GetTargetEndpoint(Vector3 targetPoint)
		{
			switch (targetingMethod)
			{
				case PAChainTargeting.End:
					return end;
				case PAChainTargeting.Start:
					return start;
				case PAChainTargeting.Closest:
					return GetClosestEndpoint(targetPoint);
				case PAChainTargeting.Alternating:
					return GetAlternatingEndpoint();
				case PAChainTargeting.AlternatingClosest:
					{
						PAEndpoint altEP = GetAlternatingEndpoint();
						float weightShift = PAConstants.endpointAlternatingWeight * (altEP == start ? -1 : 1);
						return GetClosestEndpoint(targetPoint, weightShift);
					}
				case PAChainTargeting.Preferential:
					return GetClosestEndpoint(targetPoint, PAConstants.endpointPreferentialWeight);
				default:
					break;
			}
			return end;
		}
		*/

		public void ResetTargets()
		{
			if (end != null) end.ResetTarget();
		}

		public void SetTarget(Vector3 targetPoint, Quaternion targetRotation)
		{
			if (end != null) end.SetTarget(targetPoint, targetRotation);
		}
		public void SetTarget(Transform newTarget)
		{
			Vector3 targetPoint = newTarget != null ? newTarget.position : Vector3.zero;
			if (end != null) end.SetTarget(newTarget);
		}
		public void SetTarget(IPATarget newTarget)
		{
			Vector3 targetPoint = newTarget != null ? newTarget.TargetTransform.position : Vector3.zero;
			if (end != null) end.SetTarget(newTarget);
		}

		#endregion
		#region Methods Helper

		public static List<Transform> GetHierarchyToRoot(Transform root, Transform child)
		{
			List<Transform> hierarchy = new List<Transform>();
			hierarchy.Add(child);

			Transform parent = child.parent;
			int depth = 0;
			const int maxDepth = 20;
			while (parent != null && depth++ < maxDepth)
			{
				hierarchy.Add(parent);
				if (parent == root) break;
				child = parent;
				parent = child.parent;
			}
			if (depth >= maxDepth)
			{
				Debug.LogError("[PAChain] Error! Hierarchy root search failed after max. depth was reached.");
				return null;
			}

			return hierarchy;
		}

		public static Transform[] FindHierarchyChain(Transform root, Transform start, Transform end, out int intersectNodeIndex, bool discardIntersectNode = true)
		{
			intersectNodeIndex = -1;
			if (root == null || start == null || end == null || start == end)
			{
				Debug.LogError("[PAChain] Error! Cannot determine hierarchy line from invalid root, start or end transforms!");
				return null;
			}

			Transform[] startHierarchy = GetHierarchyToRoot(root, start).ToArray();
			Transform[] endHierarchy = GetHierarchyToRoot(root, end).ToArray();

			List<Transform> chain = new List<Transform>();

			bool found = false;
			int j = 0;
			for(int i = 0; i < startHierarchy.Length; ++i)
			{
				Transform startNode = startHierarchy[i];

				for (j = 0; j < endHierarchy.Length; ++j)
				{
					Transform endNode = endHierarchy[j];
					if (startNode == endNode)
					{
						found = true;
						intersectNodeIndex = i;
						break;
					}
				}
				if (!found || !discardIntersectNode) chain.Add(startNode);
				if (found) break;
			}
			if (!found)
			{
				Debug.LogError($"[PAChain] Error! Could not find hierarchial overlap for endpoints '{start.name}' and '{end.name}'");
				return null;
			}

			for(int i = j - 1; i >= 0; i--)
			{
				Transform downNode = endHierarchy[i];
				chain.Add(downNode);
			}

			return chain.ToArray();
		}

		public override string ToString()
		{
			int jointCount = joints != null ? joints.Length : 0;
			return $"{root?.name}-{end?.name} ({jointCount})";
		}

		#endregion
	}
}
