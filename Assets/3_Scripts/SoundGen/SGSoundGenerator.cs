using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundGen
{
	public class SGSoundGenerator
	{
		#region Constructors

		public SGSoundGenerator(int _sampleCount = defaultSampleCount, float _sampleRate = defaultSampleRate, bool _isStereo = false)
		{
			sampleCount = Mathf.Max(_sampleCount, 1);
			sampleRate = Mathf.Max(_sampleRate, 1);
			samplePeriod = 1.0f / sampleRate;
			duration = sampleCount / sampleRate;
			isStereo = _isStereo;

			timestamp = 0.0f;

			samples = new float[sampleCount];

			Clear();
		}

		#endregion
		#region Fields

		public readonly int sampleCount = 1024;
		public readonly float sampleRate = 44100.0f;
		public readonly float samplePeriod = 1.0f / 44100.0f;
		public readonly float duration = 1024 / 44100.0f;
		public readonly bool isStereo = false;

		private float timestamp = 0.0f;

		private float[] samples = null;
		private AudioClip clip = null;

		private SGGeneratorGraph graph = null;

		public const int defaultSampleCount = 1024;
		public const float defaultSampleRate = 44100.0f;
		public const float defaultSamplePeriod = 1.0f / defaultSampleRate;
		public const float defaultBufferDuration = defaultSampleCount / defaultSampleRate;
		public const string defaultClipName = "SoundGenClip";

		#endregion
		#region Properties

		public float Timestamp => timestamp;

		public float[] Samples => samples;
		public AudioClip Clip => clip;

		public SGGeneratorGraph GeneratorGraph => graph;

		#endregion
		#region Methods

		public void Clear()
		{
			for (int i = 0; i < sampleCount; ++i)
			{
				samples[i] = 0.0f;
			}
		}

		public void SetTimestamp(float newTimestamp)
		{
			timestamp = newTimestamp;
		}

		public AudioClip CreateClip(string clipName = defaultClipName)
		{
			clip = AudioClip.Create(clipName, sampleCount, 1, (int)sampleRate, true, callbackAudioReader);

			Apply(clip);

			return clip;
		}

		public bool SetGeneratorGraph(SGGeneratorGraph newGraph)
		{
			if(newGraph == null)
			{
				graph = null;
				return true;
			}
			else
			{
				graph = newGraph;
				return graph.VerifyGraph(sampleCount);
			}
		}

		public bool Apply(AudioClip clip)
		{
			if (clip == null) return false;

			return clip.SetData(samples, 0);
		}

		public void GenerateAudio()
		{
			GenerateAudio(samples);
		}
		private void GenerateAudio(float[] sampleBuffer)
		{
			if(graph != null)
			{
				graph.GenerateAudio(this, sampleBuffer, ref timestamp);
			}
			else
			{
				timestamp += duration;
			}
		}

		private void callbackAudioReader(float[] data)
		{
			GenerateAudio(data);
		}

		#endregion
	}
}
