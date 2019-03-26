using System;
using UnityEngine;

namespace ProceduralAnim.IK
{
	public static class PASolver
	{
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

			// TODO

			return PASolverState.Solved;
		}

		private static PASolverReachResult GetChainReach(PAChainJoint[] jointList, Vector3 targetPoint, Quaternion targetRot, float reachModifier, float maxReachSq)
		{
			if(jointList == null) return PASolverReachResult.None;

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
