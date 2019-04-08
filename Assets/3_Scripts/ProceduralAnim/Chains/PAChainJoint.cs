using System;
using UnityEngine;

namespace ProceduralAnim
{
	[System.Serializable]
	public struct PAChainJoint
	{
		#region Constructors

		public PAChainJoint(PAJoint _joint, Vector3 _hierarchyOffset, Quaternion _hierarchyRotation, float _maxEndpointReachSq, int _order = 0)
		{
			joint = _joint;
			hierarchyOffset = _hierarchyOffset;
			hierarchyRotation = _hierarchyRotation;
			maxEndpointReachSq = _maxEndpointReachSq;
			order = _order;
		}
		public PAChainJoint(PAChainJoint other, int _order = 0)
		{
			joint = other.joint;
			hierarchyOffset = other.hierarchyOffset;
			hierarchyRotation = other.hierarchyRotation;
			maxEndpointReachSq = other.maxEndpointReachSq;
			order = _order;
		}

		#endregion
		#region Fields

		public PAJoint joint;
		public Vector3 hierarchyOffset;
		public Quaternion hierarchyRotation;
		public float maxEndpointReachSq;
		public int order;

		#endregion
		#region Properties

		public static PAChainJoint None => new PAChainJoint(null, Vector3.zero, Quaternion.identity, 0.0f);

		#endregion
	}
}
