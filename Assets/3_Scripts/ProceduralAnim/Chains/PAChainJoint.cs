using System;
using UnityEngine;

namespace ProceduralAnim
{
	[System.Serializable]
	public class PAChainJoint
	{
		#region Constructors

		public PAChainJoint(PAJoint _joint, Vector3 _hierarchyOffset, Quaternion _hierarchyRotation, float _maxEndpointReach, float _segmentLength, int _order = 0)
		{
			joint = _joint;
			hierarchyOffset = _hierarchyOffset;
			hierarchyRotation = _hierarchyRotation;
			maxEndpointReach = _maxEndpointReach;
			maxEndpointReachSq = _maxEndpointReach * _maxEndpointReach;
			segmentLength = _segmentLength;
			order = _order;
		}
		public PAChainJoint(PAChainJoint other, int _order = 0)
		{
			joint = other.joint;
			hierarchyOffset = other.hierarchyOffset;
			hierarchyRotation = other.hierarchyRotation;
			maxEndpointReach = other.maxEndpointReach;
			maxEndpointReachSq = other.maxEndpointReachSq;
			segmentLength = other.segmentLength;
			order = _order;
		}

		#endregion
		#region Fields

		public PAJoint joint = null;
		public Vector3 hierarchyOffset = Vector3.zero;
		public Quaternion hierarchyRotation = Quaternion.identity;
		public float maxEndpointReach = 0;
		public float maxEndpointReachSq = 0;
		public float segmentLength = 0;
		public int order = 0;

		#endregion
		#region Properties

		public static PAChainJoint None => new PAChainJoint(null, Vector3.zero, Quaternion.identity, 0.0f, 0.0f);

		#endregion
	}
}
