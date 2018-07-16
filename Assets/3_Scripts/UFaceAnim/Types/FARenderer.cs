using System.Collections;
using UnityEngine;

namespace UFaceAnim
{
	[AddComponentMenu("Scripts/UFaceAnim/Renderer")]
	[RequireComponent(typeof(SkinnedMeshRenderer))]
	public class FARenderer : MonoBehaviour
	{
		#region Types

		[System.Serializable]
		public struct Target
		{
			public FABlendTarget target;
			public bool negativeRange;

			public static Target None { get { return new Target() { target = FABlendTarget.None, negativeRange = false }; } }
		}

		#endregion
		#region Fields

		public Target[] targets = new Target[1] { Target.None };

		private SkinnedMeshRenderer rend = null;

		#endregion
		#region Methods

		public void initialize()
		{
			// Retrieve the skinned mesh renderer used for controlling the blend shapes:
			rend = GetComponent<SkinnedMeshRenderer>();

			// Make sure the blend shape count matches the target array size:
			int shapeCount = Mathf.Max(rend.sharedMesh.blendShapeCount, targets.Length);
			if (targets.Length < shapeCount)
			{
				Target[] newTars = new Target[shapeCount];
				for(int i = 0; i < newTars.Length; ++i)
				{
					newTars[i] = i < targets.Length ? targets[i] : Target.None;
				}
			}

			//...
		}

		public void update(ref FABlendState blendState)
		{
			if (targets == null) return;

			for(int i = 0; i < targets.Length; ++i)
			{
				Target target = targets[i];
				if(target.target != FABlendTarget.None)
				{
					updateState(ref blendState, i, target);
				}
			}
		}

		private void updateState(ref FABlendState blendState, int index, Target target)
		{
			float value = 0.0f;
			switch (target.target)
			{
				case FABlendTarget.MouthOpenClose:
					value = blendState.mouthOpenClose;
					break;
				case FABlendTarget.MouthShowTeeth:
					value = blendState.mouthShowTeeth;
					break;
				case FABlendTarget.MouthInOut:
					value = blendState.mouthInOut;
					break;
				case FABlendTarget.MouthCornerL:
					value = blendState.mouthCornerL;
					break;
				case FABlendTarget.MouthCornerR:
					value = blendState.mouthCornerR;
					break;
				case FABlendTarget.BrowsInOut:
					value = blendState.browsInOut;
					break;
				case FABlendTarget.BrowsL:
					value = blendState.browsL;
					break;
				case FABlendTarget.BrowsR:
					value = blendState.browsR;
					break;
				case FABlendTarget.BrowsSharpFlat:
					value = blendState.browsSharpFlat;
					break;
				case FABlendTarget.EyesCloseL:
					value = blendState.eyesCloseL;
					break;
				case FABlendTarget.EyesCloseR:
					value = blendState.eyesCloseR;
					break;
				case FABlendTarget.EyesWander:
					value = blendState.eyesWander;
					break;
				default:
					break;
			}

			if (target.negativeRange) value *= -1;
			float weight = 100 * value;

			rend.SetBlendShapeWeight(index, weight);
		}

		#endregion
	}
}
