using System.Collections;
using UnityEngine;

namespace UFaceAnim.Audio
{
	[RequireComponent(typeof(AudioSource))]
	public class FAAudioReaderEnergy : FAAudioReader
	{
		#region Fields

		//TEST
		public Transform testCube = null;

		#endregion
		#region Methods

		protected override void Update()
		{
			switch (signalOrigin)
			{
				case FAAudioOrigin.Microphone:
					if (!Microphone.IsRecording(null))
					{
						startMicrophone();
					}
					break;
				case FAAudioOrigin.Stream:
					break;
				default:
					break;
			}

			audioSrc.GetOutputData(samples, 0);

			energy = 0.0f;
			for(int i = 0; i < samples.Length; ++i)
			{
				energy += Mathf.Abs(samples[i]);
			}
			energy *= amplification;

			//TEST
			if (testCube != null)
			{
				testCube.localScale = Vector3.one * (0.1f + energy * 0.4f);
			}
		}

		#endregion
	}
}
