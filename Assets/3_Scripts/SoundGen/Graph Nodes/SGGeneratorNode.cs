using UnityEngine;

namespace SoundGen
{
	[System.Serializable]
	public abstract class SGGeneratorNode
	{
		#region Fields

		public int order = 0;

		protected float[] outputSamples = null;
		protected int outputSampleCount = 0;

		#endregion
		#region Methods

		public virtual bool VerifyNode(int minSampleCount)
		{
			// Make sure the node has at least a minimum sample count ready to hold output data:
			if(outputSamples.Length < minSampleCount)
			{
				outputSamples = new float[minSampleCount];
				ClearOutputBuffer();
			}

			return true;
		}

		public void SetOutputBuffer(float[] newOutputSamples)
		{
			// If no sample array was given, create new one with minimum buffer size:
			outputSamples = newOutputSamples != null ? newOutputSamples  : new float[SGGeneratorGraph.minAllowedSampleCount];

			// Clear output buffer contents:
			ClearOutputBuffer();
		}
		public void ClearOutputBuffer()
		{
			// Reset all content in output sample buffer:
			if(outputSamples != null)
			{
				for(int i = 0; i < outputSamples.Length; ++i)
				{
					outputSamples[i] = 0.0f;
				}
			}
			outputSampleCount = 0;
		}

		public abstract bool SetInputNode();
		public abstract void ClearInputNodes();

		public abstract void GenerateAudio(SGSoundGenerator gen, float timestamp);

		#endregion
	}
}
