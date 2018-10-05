using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UHairGen
{
	[System.Serializable]
	public struct HGNode
	{
		#region Constructors

		public HGNode(Vector3 pos, Quaternion rot, float tan = 0.1f)
		{
			position = pos;
			rotation = rot;
			tangent = tan;
		}

		#endregion
		#region Fields

		public Vector3 position;
		public Quaternion rotation;
		public float tangent;

		#endregion
	}
}
