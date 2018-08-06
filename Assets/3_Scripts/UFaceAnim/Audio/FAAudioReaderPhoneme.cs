using System.Collections;
using UnityEngine;

namespace UFaceAnim.Audio
{
	[AddComponentMenu("Scripts/UFaceAnim/Audio/Phoneme Reader")]
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
		public float plosiveBandWidth = 300.0f;
		private int plosiveBandCount = 1;
		public float fricativeBandThreshold = 0.01f; // [%]
		public float fricativeMinBandAmount = 0.55f;	// [%]

		private float[,] bands = null;
		private float[] vowelBandMatrix = null;
		private float[] plosiveBandMatrix = null;

		private float minEnergy = 0.1f;
		private float maxEnergy = 0.0f;

		private FABasePhonemes prevPhoneme = FABasePhonemes.None;
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
		private float[] plosiveF1 = new float[5]	// >>> TODO: Extract frequency ranges from paper! <<<
{
			0,		// i
			0,		// u
			200,	// e, é
			0,		// o
			550,    // a, ae
};
		private float[] plosiveF2 = new float[5]
		{
			1950,	// i
			600,	// u
			1600,	// e, é
			450,	// o
			700,	// a, ae
		};
		// NOTE: The above bands F1 and F2 have been set with an approximate vocal range 4000 Hz, they are adjusted on start.

		private static readonly FABasePhonemes[] phonemeCheckOrder = new FABasePhonemes[10]
		{
			FABasePhonemes.Group0_AI, FABasePhonemes.Group2_U, FABasePhonemes.Group1_E, FABasePhonemes.Group3_O, FABasePhonemes.Group0_AI,
			FABasePhonemes.Group4_CDGK, FABasePhonemes.Group5_FV, FABasePhonemes.Group6_LTh, FABasePhonemes.Group7_MBP, FABasePhonemes.Group8_WQ
		};

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

			// Prepare vowel and plosive detection bands and matrices:
			initializePhonemeBands(ref vowelBandCount, vowelBandWidth, ref vowelBandMatrix, ref vowelBandThreshold, vowelF1, vowelF2);
			initializePhonemeBands(ref plosiveBandCount, plosiveBandWidth, ref plosiveBandMatrix, ref vowelBandThreshold, plosiveF1, plosiveF2);
		}

		private void initializePhonemeBands(ref int bandCount, float bandWidth, ref float[] bandMatrix, ref float bandThreshold, float[] f1, float[] f2)
		{
			bandCount = Mathf.CeilToInt(getFFTBandFromFrequency(bandWidth));
			bandMatrix = new float[bandCount];

			float halfBandWidth = bandCount * 0.5f;
			for (int i = 0; i < bandMatrix.Length; ++i)
			{
				bandMatrix[i] = i < halfBandWidth ? i / halfBandWidth : 1 - (i - halfBandWidth) / halfBandWidth;
			}

			bandThreshold /= frameCount;

			float f1f2Modifier = vowelFrequencyRange / 4000.0f;
			for (int i = 0; i < f1.Length; ++i)
			{
				f1[i] *= f1f2Modifier;
				f2[i] *= f1f2Modifier;
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
			energy *= amplification;

			maxEnergy = Mathf.Max(maxEnergy, energy);
			minEnergy = Mathf.Min(minEnergy, energy);
			float silenceLevel = Mathf.Lerp(minEnergy, maxEnergy, silenceRange);

			prevPhoneme = phoneme;

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

				findPhonemes();
			}
			else
			{
				phoneme = FABasePhonemes.None;
			}
		}

		private void findPhonemes()
		{
			float maxWeight = 0.0f;
			FABasePhonemes maxWeightPhoneme = FABasePhonemes.None;

			// Check vowels first, using their respective F bands:
			for(int i = 0; i < 5; ++i)
			{
				float weight = findVowel(vowelF1[i], vowelF2[i]);
				if(weight > maxWeight)
				{
					maxWeight = weight;
					maxWeightPhoneme = phonemeCheckOrder[i];
				}
			}

			// Check plosives next, as they are recognizable by the preceeding silence:
			if(prevPhoneme == FABasePhonemes.None)
			{
				// >>> TODO: actually check the bands for each of the plosives just like we did with the vowels! <<<
			}

			// Check for any fricatives. In the spectrum diagram, these are similar white noise, aka non-negligeable energy across many bands:
			{
				int fricativeMaxBand = getFFTBandFromFrequency(vowelFrequencyRange);
				int fricativeBandsCovered = 0;
				for (int i = 0; i < fricativeMaxBand; ++i)
				{
					if (samples[i] > fricativeBandThreshold) fricativeBandsCovered++;
				}
				float fricativeAmount = (float)fricativeBandsCovered / fricativeMaxBand;
				float fricativeWeight = fricativeAmount > fricativeMinBandAmount ? fricativeAmount : 0;
				if (fricativeWeight > maxWeight)
				{
					maxWeight = fricativeWeight;
					maxWeightPhoneme = FABasePhonemes.Group5_FV;
				}
			}

			// TODO: Check for each plosive and nasal sound one after the other as well.

			phoneme = maxWeightPhoneme;
		}

		private float findVowel(float f1, float f2)
		{
			int bandF1 = getFFTBandFromFrequency(f1);
			int bandF2 = getFFTBandFromFrequency(f2);

			float resultF1 = findBand(bandF1, vowelBandCount);
			float resultF2 = findBand(bandF2, vowelBandCount);

			if (resultF1 < vowelBandThreshold) resultF1 = 0;
			if (resultF2 < vowelBandThreshold) resultF2 = 0;

			return Mathf.Min(resultF1, resultF2);
		}

		private float findPlosive(float f1, float f2)
		{
			int bandF1 = getFFTBandFromFrequency(f1);
			int bandF2 = getFFTBandFromFrequency(f2);

			float resultF1 = findBand(bandF1, plosiveBandCount);
			float resultF2 = findBand(bandF2, plosiveBandCount);

			if (resultF1 < vowelBandThreshold) resultF1 = 0;
			if (resultF2 < vowelBandThreshold) resultF2 = 0;

			return Mathf.Min(resultF1, resultF2);
		}

		private float findBand(int bandIndex, int fBandWidth)
		{
			// NOTE: This checks if there is a clearly defined frequency band in a specific frequency range: (useful for detecting vowels)

			// Measure band values relative to a 'baseline' formed by the lowest and highest band values within the range:
			float invBandWidth = 1.0f / fBandWidth;
			float valueLow = samples[bandIndex];
			float valueHigh = samples[bandIndex + fBandWidth - 1];

			// Calculate correlation between the band values and the matrix:
			float sum = 0.0f;
			for(int i = 0; i < fBandWidth; ++i)
			{
				int j = bandIndex + i;

				float baseLine = Mathf.Lerp(valueLow, valueHigh, i * invBandWidth);
				float offset = samples[j] - baseLine;
				sum += offset * vowelBandMatrix[i];
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
