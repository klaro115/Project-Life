using System;
using UnityEngine;

namespace ProceduralAnim
{
	[System.Serializable]
	public struct PAChainJoint
	{
		#region Constructors

		public PAChainJoint(PAJoint _joint, Vector3 _hierarchyOffset, Quaternion _hierarchyRotation, float _maxEndpointReachSq)
		{
			joint = _joint;
			hierarchyOffset = _hierarchyOffset;
			hierarchyRotation = _hierarchyRotation;
			maxEndpointReachSq = _maxEndpointReachSq;
		}

		#endregion
		#region Fields

		public PAJoint joint;
		public Vector3 hierarchyOffset;
		public Quaternion hierarchyRotation;
		public float maxEndpointReachSq;

		#endregion
		#region Properties

		public static PAChainJoint None => new PAChainJoint(null, Vector3.zero, Quaternion.identity, 0.0f);

		#endregion
	}
}
