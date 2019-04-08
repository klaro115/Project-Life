using System;
using UnityEngine;

namespace ProceduralAnim.IK
{
	public static class PASolver
	{
		#region Fields

		private static int jointBallCount = 0;
		private static int[] jointBallList = new int[PAConstants.maxSolverJointListLength];

		#endregion
		#region Methods

		public static PASolverState SolveChain(PAChain chain, PASolverPolicy policy)
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

			// TODO: Algorithm layout:
			//
			// 1. Determine target pose for endpoint.
			// 2. Derive outer-most joint's target pose from endpoint => joint is 'fixed'.
			// 3. Calculate offsets (rotation & position) from first (fixed) joint to last.
			//  3.1 Calculate fraction of offset to distribute to each ball joint.
			// 4. Move up chain, iterating over ball joints only:
			//  4.1 Draw offsets from previous fixed joint's pose to current ball joint.
			//  4.2 Equally divide offsets to distribute between ball and hinge joints after previous fixed joint.
			//  4.2 Iterate over .
			//   4.2.1 

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

			if (Vector3.SqrMagnitude(lastJointOffset) < maxReachSq)
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
						return new PASolverReachResult() { outOfReach = false, maxIndex = i, offset = cjOffset, rotation = cjRotOffset };
					}
				}
			}

			return new PASolverReachResult() { outOfReach = true, maxIndex = lastIndex, offset = lastJointOffset };
		}

		#endregion
	}
}
