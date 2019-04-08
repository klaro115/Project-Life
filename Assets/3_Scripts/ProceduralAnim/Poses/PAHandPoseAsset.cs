using System;
using UnityEngine;

namespace ProceduralAnim.Poses
{
	[System.Serializable]
	[CreateAssetMenu(menuName = "ProcAnim/Hand Pose", fileName = "new HandPose")]
	public class PAHandPoseAsset : ScriptableObject
	{
		#region Fields

		public PAHandPose pose = PAHandPose.Default;

		#endregion
	}
}
