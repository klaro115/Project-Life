using UnityEngine;

namespace ProceduralAnim.Poses
{
	[System.Serializable]
	public struct PAFinger
	{
		#region Fields

		public Transform phalanx0;
		public Transform phalanx1;
		public Transform phalanx2;

		[HideInInspector]
		public Quaternion defaultRotation0;
		[HideInInspector]
		public Quaternion defaultRotation1;
		[HideInInspector]
		public Quaternion defaultRotation2;

		#endregion
		#region Methods

		public void Initialize()
		{
			defaultRotation0 = phalanx0 != null ? phalanx0.localRotation : Quaternion.identity;
			defaultRotation1 = phalanx1 != null ? phalanx1.localRotation : Quaternion.identity;
			defaultRotation2 = phalanx2 != null ? phalanx2.localRotation : Quaternion.identity;
		}

		#endregion
	}

	[System.Serializable]
	public struct PAHandAxisSet
	{
		#region Fields

		public Vector3 openCloseAxis;
		public Vector3 spreadAxis;
		public Vector3 rollAxis;

		public float rollMaxAngle;
		public float spreadMaxAngle;
		public float openCloseMaxAngle;

		#endregion
		#region Properties

		public static PAHandAxisSet Default => new PAHandAxisSet()
		{
			openCloseAxis = Vector3.right,
			spreadAxis = Vector3.forward,
			rollAxis = Vector3.up,
			rollMaxAngle = 80.0f,
			spreadMaxAngle = 20.0f,
			openCloseMaxAngle = 165.0f
		};

		#endregion
		#region Methods

		public void Initialize()
		{
			openCloseAxis = openCloseAxis.normalized;
			spreadAxis = spreadAxis.normalized;
			rollAxis = rollAxis.normalized;
		}
		
		#endregion
	}
}
