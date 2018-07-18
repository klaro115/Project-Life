using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFaceAnim.Audio
{
	public abstract class FAAudioReader : MonoBehaviour
	{
		#region Fields

		public FAAudioOrigin signalOrigin = FAAudioOrigin.AudioSource;

		public int sampleCount = 256;

		protected AudioSource audioSrc = null;
		protected int audioDeviceCap = 44100;

		protected float[] samples = null;
		protected float energy = 0.0f;

		#endregion
		#region Properties

		public AudioSource Source
		{
			get { return audioSrc; }
		}
		public float Energy
		{
			get { return energy; }
		}

		#endregion
		#region Methods

		protected virtual void Start()
		{
			audioSrc = GetComponent<AudioSource>();

			sampleCount = Mathf.NextPowerOfTwo(Mathf.Max(sampleCount, 32));
			if (samples == null || samples.Length != sampleCount)
			{
				samples = new float[sampleCount];
			}

			if (signalOrigin == FAAudioOrigin.Microphone)
			{
				if (Microphone.devices == null)
				{
					signalOrigin = FAAudioOrigin.None;
					return;
				}

				int audioDeviceCapMin;
				Microphone.GetDeviceCaps(null, out audioDeviceCapMin, out audioDeviceCap);

				startMicrophone();
			}
			else if (signalOrigin == FAAudioOrigin.Stream)
			{
				// TODO: Open web audio stream.
			}
			else
			{
				audioSrc.Play();
			}
		}

		public bool setAudioClip(AudioClip newClip, bool playNow = true)
		{
			if (newClip == null || signalOrigin != FAAudioOrigin.AudioSource) return false;

			audioSrc.clip = newClip;
			if (playNow)
			{
				audioSrc.Play();
			}
			return true;
		}

		protected void startMicrophone()
		{
			audioSrc.clip = Microphone.Start(null, true, 1, audioDeviceCap);//audioDevice
			audioSrc.loop = true;
			//audioSrc.mute = true;	// < Really, who likes to hear their own voice...
			//audioSrc.volume = 0;
			audioSrc.Play();
		}

		[ContextMenu("Log microphones")]
		public void logMicrophoneDevices()
		{
			string[] devices = Microphone.devices;
			if (devices != null)
			{
				foreach (string d in devices)
					Debug.Log("Microphone device found: '" + d + "'");
			}
		}

		protected abstract void Update();

		#endregion
	}
}
