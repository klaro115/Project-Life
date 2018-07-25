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
			public float modifier;

			public static Target None { get { return new Target() { target = FABlendTarget.None, negativeRange = false, modifier = 0.0f }; } }
		}

		#endregion
		#region Fields

		public Target[] targets = new Target[1] { Target.None };
		private int blendShapeCount = 1;

		private SkinnedMeshRenderer rend = null;

		#endregion
		#region Methods

		public void initialize()
		{
			// Retrieve the skinned mesh renderer used for controlling the blend shapes:
			rend = GetComponent<SkinnedMeshRenderer>();
			blendShapeCount = rend.sharedMesh.blendShapeCount;

			// Make sure the blend shape count matches the target array size:
			int shapeCount = Mathf.Max(blendShapeCount, targets.Length);
			if (targets.Length < shapeCount)
			{
				Target[] newTars = new Target[shapeCount];
				for(int i = 0; i < newTars.Length; ++i)
				{
					newTars[i] = i < targets.Length ? targets[i] : Target.None;
				}
			}

			// Reset all blend shape weights in renderer:
			for(int i = 0; i < rend.sharedMesh.blendShapeCount; ++i)
			{
				rend.SetBlendShapeWeight(i, 0.0f);
			}
		}

		public void update(ref FABlendState blendState)
		{
			if (targets == null) return;

			for(int i = 0; i < blendShapeCount; ++i)
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
			// Retrieve the appropriate weighting value for the given target:
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
				case FABlendTarget.BrowsL:
					value = blendState.browsL;
					break;
				case FABlendTarget.BrowsR:
					value = blendState.browsR;
					break;
				case FABlendTarget.BrowsInOut:
					value = blendState.browsInOut;
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
				case FABlendTarget.EyesDirX:
					value = blendState.eyesDir.x;
					break;
				case FABlendTarget.EyesDirY:
					value = blendState.eyesDir.y;
					break;
				default:
					break;
			}

			// Process blend weight:
			value *= 1.0f + target.modifier;
			if (target.negativeRange) value *= -1;
			float weight = 100 * Mathf.Clamp01(value);

			// Write blend shape weight to skinned mesh renderer:
			rend.SetBlendShapeWeight(index, weight);
		}

		#endregion
	}
}
