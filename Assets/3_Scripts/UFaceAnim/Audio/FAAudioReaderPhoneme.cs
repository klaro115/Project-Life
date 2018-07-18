using System.Collections;
using UnityEngine;

namespace UFaceAnim.Audio
{
	[RequireComponent(typeof(AudioSource))]
	public class FAAudioReaderPhoneme : FAAudioReader
	{
		#region Fields

		public int sampleRate = 44100;
		public int frameCount = 3;

		public float silenceRange = 0.05f;			// [%]?

		public float vowelFrequencyRange = 4000.0f;	// [Hz]
		public float vowelBandThreshold = 0.1f;		// [%]
		public float vowelBandWidth = 750.0f;		// [Hz]
		private int vowelBandCount = 1;

		private float[,] bands = null;
		private float[] vowelBandMatrix = null;

		private float minEnergy = 0.1f;
		private float maxEnergy = 0.0f;

		private FABasePhonemes phoneme = FABasePhonemes.None;

		private float[] vowelF1 = new float[5]
		{
			0,		// i
			0,		// u
			200,	// e, é
			0,		// o
			550,	// a, ae
		};
		private float[] vowelF2 = new float[5]
		{
			1950,	// i
			600,	// u
			1600,	// e, é
			450,	// o
			700,	// a, ae
		};
		// NOTE: The above bands F1 and F2 have been set with an approximate vocal range 4000 Hz, they are adjusted on start.

		#endregion
		#region Properties
		
		public FABasePhonemes Phoneme
		{
			get { return phoneme; }
		}

		#endregion
		#region Methods

		protected override void Start()
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

			bands = new float[frameCount, sampleCount];
			for (int f = 0; f < frameCount; ++f)
			{
				for (int b = 0; b < sampleCount; ++b)
					bands[f, b] = 0.0f;
			}

			// Prepare vowel detection bands and matrices:
			{
				vowelBandCount = Mathf.CeilToInt(getFFTBandFromFrequency(vowelBandWidth));
				vowelBandMatrix = new float[vowelBandCount];

				float halfBandWidth = vowelBandCount * 0.5f;
				for (int i = 0; i < vowelBandMatrix.Length; ++i)
				{
					vowelBandMatrix[i] = i < halfBandWidth ? i / halfBandWidth : 1 - (i - halfBandWidth) / halfBandWidth;
				}

				vowelBandThreshold /= frameCount;

				float f1f2Modifier = vowelFrequencyRange / 4000.0f;
				for(int i = 0; i < vowelF1.Length; ++i)
				{
					vowelF1[i] *= f1f2Modifier;
					vowelF2[i] *= f1f2Modifier;
				}
			}
		}

		protected override void Update()
		{
			// Push back band frame buffers:
			for (int f = 0; f < frameCount - 1; ++f)
			{
				for (int i = 0; i < sampleCount; ++i)
				{
					bands[f + 1, i] = bands[f, i];
				}
			}

			// Do FFT:
			audioSrc.GetSpectrumData(samples, 0, FFTWindow.Hanning);

			// Calculate total energy across the entire spectrum:
			energy = 0.0f;
			for (int i = 0; i < samples.Length; ++i)
			{
				float sample = samples[i];
				bands[0, i] = sample;

				energy += sample;
			}
			maxEnergy = Mathf.Max(maxEnergy, energy);
			minEnergy = Mathf.Min(minEnergy, energy);
			float silenceLevel = Mathf.Lerp(minEnergy, maxEnergy, silenceRange);

			// Only do further analysis if a minimum energy/loudness threshold was surpassed:
			if (energy > silenceLevel)
			{
				// Just ignore any bands beyond roughly 6 KHz:
				int maxBand = getFFTBandFromFrequency(6000);

				float invFrameCount = 1.0f / frameCount;

				for (int i = 0; i < maxBand; ++i)
				{
					// Take the average energy across multiple frames for noise reduction and signal smoothing:
					float band = 0.0f;
					for(int f = 0; f < frameCount - 1; ++f)
					{
						band += bands[f, i];
					}
					band *= invFrameCount;

					// Since we conveniently have a sample buffer the right size lying around, we may as well store values there for now:
					samples[i] = band;
				}

				findPhonemes(maxBand);
			}
			else
			{
				phoneme = FABasePhonemes.None;
			}
		}

		private void findPhonemes(int bufferSize)
		{
			float vi = findVowel(vowelF1[0], vowelF2[0]);
			float vu = findVowel(vowelF1[1], vowelF2[1]);
			float ve = findVowel(vowelF1[2], vowelF2[2]);
			float vo = findVowel(vowelF1[3], vowelF2[3]);
			float va = findVowel(vowelF1[4], vowelF2[4]);

			phoneme = FABasePhonemes.Group9_Rest;
			float vMax = 0.0001f;
			if(vi > vMax || va > vMax)
			{
				vMax = Mathf.Max(vi, va);
				phoneme = FABasePhonemes.Group0_AI;
			}
			if (vu > vMax)
			{
				vMax = vu;
				phoneme = FABasePhonemes.Group2_U;
			}
			if (ve > vMax)
			{
				vMax = ve;
				phoneme = FABasePhonemes.Group1_E;
			}
			if (vo > vMax)
			{
				vMax = vo;
				phoneme = FABasePhonemes.Group3_O;
			}

			// TODO: Check for each fricative, plosive and nasal sound one after the other as well.
		}

		private float findVowel(float f1, float f2)
		{
			int bandF1 = getFFTBandFromFrequency(f1);
			int bandF2 = getFFTBandFromFrequency(f2);

			float resultF1 = findFBand(bandF1, vowelBandCount);
			float resultF2 = findFBand(bandF2, vowelBandCount);

			if (resultF1 < vowelBandThreshold) resultF1 = 0;
			if (resultF2 < vowelBandThreshold) resultF2 = 0;

			return Mathf.Min(resultF1, resultF2);
		}

		private float findFBand(int bandIndex, int fBandWidth)
		{
			// NOTE: This checks if there is a clearly defined frequency band in a specific frequency range: (useful for detecting vowels)

			// Calculate correlation between the band values and the matrix:
			float sum = 0.0f;
			for(int i = bandIndex; i < fBandWidth; ++i)
			{
				sum += samples[i] * vowelBandMatrix[i];
			}

			return sum / fBandWidth;
			// NOTE: Complete formula: rho = 1/N * sum(0,N)( x(n) * y(n) )
		}

		private float getFrequencyFromFFTBand(int bandIndex)
		{
			return sampleRate * bandIndex / sampleCount;
		}
		private int getFFTBandFromFrequency(float freq)
		{
			return (int)Mathf.Clamp(freq * sampleCount / sampleRate, 0, sampleCount);
		}

		#endregion
	}
}
