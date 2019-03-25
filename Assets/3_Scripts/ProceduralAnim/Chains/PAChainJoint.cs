using System;
using UnityEngine;

namespace ProceduralAnim
{
	[System.Serializable]
	public struct PAChainJoint
	{
		#region Constructors

		public PAChainJoint(PAJoint _joint, Vector3 _hierarchyOffset, Quaternion _hierarchyRotation)
		{
			joint = _joint;
			hierarchyOffset = _hierarchyOffset;
			hierarchyRotation = _hierarchyRotation;
		}

		#endregion
		#region Fields

		public readonly PAJoint joint;
		public readonly Vector3 hierarchyOffset;
		public readonly Quaternion hierarchyRotation;

		#endregion
		#region Properties

		public static PAChainJoint None => new PAChainJoint(null, Vector3.zero, Quaternion.identity);

		#endregion
	}
}
