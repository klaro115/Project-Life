using System;
using UnityEngine;

namespace ProceduralAnim.IK
{
	public static class PASolver
	{
		#region Types

		public enum IntersectType
		{
			Circle,
			TwoPoints,
			Point
		}
		public struct Intersect
		{
			public IntersectType type;
			public Vector3 center;
			public Vector3 pointA;
			public Vector3 pointB;
			public float radius;
			public float spacingRatio;
		}

		#endregion
		#region Fields

		private static int jointBallCount = 0;
		private static int[] jointBallList = new int[PAConstants.maxSolverJointListLength];
		private static Intersect[] intersectBuffer = new Intersect[PAConstants.maxSolverJointListLength];

		#endregion
		#region Methods

		public static PASolverState SolveChain(PAChain chain, PASolverPolicy policy, float urgency = PAConstants.jointDefaultUrgency)
		{
			if(chain == null)
			{
				Debug.LogError("[PASolver] Error! Unable to resolve IK for null chain!");
				return PASolverState.Failed;
			}

			PAEndpoint end = chain.end;
			PAChainJoint[] jointList = chain.joints;

			Vector3 targetPoint = end.targetPoint;
			Quaternion targetRot = end.targetRotation;

			float reachDefer = chain.endpointReachDefering;
			float reachModifier = 1.0f + reachDefer;

			PASolverReachResult reachResult = GetChainReach(jointList, targetPoint, targetRot, reachModifier, chain.maxReachSq);

			if(policy == PASolverPolicy.AbortWhenOutOfReach && reachResult.outOfReach)
			{
				return PASolverState.OutOfReach;
			}

			// 1. Determine outer-most joint pose from endpoint targets:
			PAChainJoint endChainJoint = jointList[0];
			PAJoint endJoint = endChainJoint.joint;
			Vector3 endJointPos = targetPoint - end.ContactOffset + endJoint.transform.InverseTransformDirection(endChainJoint.hierarchyOffset);
			Quaternion endJointRot = targetRot * Quaternion.Inverse(end.ContactRotationOffset) * Quaternion.Inverse(endChainJoint.hierarchyRotation);
			endJoint.SetTargets(endJointPos, endJointRot, urgency);

			// 2. Draw line from inner-most to endpoint joint:
			PAChainJoint curChainJoint = jointList[reachResult.maxIndex];
			PAJoint curJoint = curChainJoint.joint;
			Transform curTrans = curJoint.transform;
			Vector3 curEndOffset = endJointPos - curTrans.position;
			float curEndDist = curEndOffset.magnitude;
			float endDistance = Mathf.Sqrt(reachResult.distanceSq);
			Vector3 curEndDir = curEndOffset / curEndDist;

			// 3. For each joint, determine target spacing and reference points:
			for(int i = 0; i < reachResult.maxIndex; ++i)
			{
				PAChainJoint spacingJoint = jointList[i];
				float spacingRatio = spacingJoint.maxEndpointReach / endDistance;
				float spacingDistance = spacingRatio * endDistance;
				Vector3 intersectCenter = endJointPos - curEndDir * spacingDistance;
				float intersectRadius = Mathf.Sqrt(spacingJoint.segmentLength * spacingJoint.segmentLength - spacingDistance * spacingDistance);
				IntersectType intersectType = spacingJoint.joint.type == PAJointType.Ball ? IntersectType.Circle : IntersectType.TwoPoints;

				intersectBuffer[i] = new Intersect()
				{
					type = intersectType,
					center = intersectCenter,
					radius = intersectRadius,
					spacingRatio = spacingRatio,
				};
			}

			// TODO (see right below)

			/* SOLVER ALGORITHM BREAKDOWN:

			1. Determine endpoint joint pose.
			2. Draw line from inner-most to endpoint joint.
			3. For each joint, determine target spacing, such that:
				3.1 Spacing distances match joint length to line length ratio.
			4. From inner-most joint, derive intersections to next outward joint.
				4.1 Intersections will be either:
					- Rings		: sphere shells intersecting
					- 2 Points	: rings intersecting
					- 2 Points	: sphere shell & plane intersect
					- 1 Point	: plane & line intersect
					- 1 Point	: ring & line intersect
				4.2 Iterate outward until either:
					- Only 1 point remains from consecutive intersects.
					- The enpoint joint has been reached.
				4.3 Going inward, fix positions based on constraints given by that single-point intersect.
				4.4 Return to single-point joint to continue stepping.
			5. Continue stepping outwards until positions have been fixed.
			6. Iterate from outer-most again, calculate rotations.
			7. Return resulting pose.

			*/
			/* ALTERNATIVE ALGORITHM: (simpler, shorter, more predictable)
			
			1. Determine endpoint joint pose.
			2. For N joints involved, starting from the outer-most one:
				2.1 Treat previous (outer) joints as 1 joint segment, nicknamed lower segment SL.
				2.1 Solve single-joint IK for current joint's upper segment SU and SL.
			3. Iterate over chain until joint N, producing increasingly weaker poses.
			4. Return resulting pose.

			*/

			// Iterate over all active ball joints in chain:
			int prevBallIndex = 0;
			for(int b = 0; b < jointBallCount; ++b)
			{
				// Get references to current ball joint:
				int curBallIndex = jointBallList[b];
				PAChainJoint bcj = chain.joints[curBallIndex];
				PAJoint bj = bcj.joint;

				// Now, for all hinge joints further down the chain and after the previous ball joint:
				for (int i = prevBallIndex; i < curBallIndex; ++i)
				{
					PAChainJoint cj = chain.joints[i];
					PAJoint j = cj.joint;

					// Determine rotational planes availale
				}

				prevBallIndex = curBallIndex;
			}
			
			// TODO

			return PASolverState.Solved;
		}

		private static PASolverReachResult GetChainReach(PAChainJoint[] jointList, Vector3 targetPoint, Quaternion targetRot, float reachModifier, float maxReachSq)
		{
			if(jointList == null) return PASolverReachResult.None;

			jointBallCount = 0;

			int lastIndex = jointList.Length - 1;
			PAJoint lastJoint = jointList[lastIndex].joint;
			Vector3 lastJointOffset = targetPoint - lastJoint.transform.position;
			float lastJointDistSq = Vector3.SqrMagnitude(lastJointOffset);

			if (lastJointDistSq < maxReachSq)
			{
				for (int i = 0; i < jointList.Length; ++i)
				{
					PAChainJoint cj = jointList[i];
					Transform cjTrans = cj.joint.transform;

					Vector3 cjOffset = targetPoint - cjTrans.position;
					Quaternion cjRotOffset = targetRot * Quaternion.Inverse(cjTrans.rotation);
					float cjDistSq = Vector3.SqrMagnitude(cjOffset);

					if (cj.joint.type == PAJointType.Ball) jointBallList[jointBallCount++] = i;

					if (cjDistSq < cj.maxEndpointReachSq * reachModifier)
					{
						return new PASolverReachResult()
						{
							outOfReach = false,
							maxIndex = i,
							offset = cjOffset,
							rotation = cjRotOffset,
							distanceSq = lastJointDistSq
						};
					}
				}
			}

			return new PASolverReachResult() { outOfReach = true, maxIndex = lastIndex, offset = lastJointOffset, distanceSq = lastJointDistSq };
		}

		#endregion
	}
}
