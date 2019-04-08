using System;
using UnityEngine;

namespace MathExt
{
	[System.Serializable]
	public struct Complex
	{
		#region Constructors

		public Complex(float _a, float _b)
		{
			a = _a;
			b = _b;
		}
		public Complex(float cos, float iSin, float magnitude)
		{
			a = cos * magnitude;
			b = iSin * magnitude;
		}

		#endregion
		#region Fields

		public float a;		// Real part.
		public float b;		// Complex part.

		#endregion
		#region Properties

		public float Magnitude => Mathf.Sqrt(a * a + b * b);
		public float MagnitudeSquared => a * a + b * b;

		public float Argument => Mathf.Atan2(b, a);

		#endregion
		#region Methods

		/// <summary>
		/// Outputs the exponential/phasor form of this complex number.
		/// Transforms 'z = a + ib' to 'z = |z| * e^(iT)'
		/// </summary>
		/// <param name="outExponentTheta">Output exponent value 'T'.</param>
		/// <param name="outMultiplierMagnitude">Output multiplier value '|z|'.</param>
		public void GetExponentialForm(out float outExponentArgument, out float outMultiplierMagnitude)
		{
			outExponentArgument = Argument;
			outMultiplierMagnitude = Magnitude;
		}
		/// <summary>
		/// Outputs the trigonometry form of this complex number.
		/// Transforms 'z = a + bi' to 'z = |z| * (cos T + i * sin T)'
		/// </summary>
		/// <param name="outCos">Output cosinus part 'cos T'.</param>
		/// <param name="outISin">Output sinus part 'sin T'.</param>
		/// <param name="outMagnitude">Output magnitude '|z|'.</param>
		public void GetTrigoForm(out float outCos, out float outISin, out float outMagnitude)
		{
			float argument = Argument;

			outCos = Mathf.Cos(argument);
			outISin = Mathf.Sin(argument);
			outMagnitude = Magnitude;
		}

		public static Complex Pow(Complex c, float k)
		{
			float mag = Mathf.Pow(c.Magnitude, k);
			float theta = k * c.Argument;
			float cos = Mathf.Cos(theta);
			float iSin = Mathf.Sin(theta);

			return new Complex(cos, iSin, mag);
		}

		#endregion
		#region Methods Operators

		public static Complex operator +(Complex c0, Complex c1)
		{
			return new Complex(c0.a + c1.a, c0.b + c1.b);
		}
		public static Complex operator -(Complex c0, Complex c1)
		{
			return new Complex(c0.a - c1.a, c0.b - c1.b);
		}
		public static Complex operator *(Complex c0, Complex c1)
		{
			float a = c0.a * c1.a - c0.b * c1.b;
			float b = c0.a * c1.b + c0.b * c1.a;
			return new Complex(a, b);
		}
		public static Complex operator /(Complex c0, Complex c1)
		{
			float c = 1.0f / (c1.a * c1.a + c1.b * c1.b);
			float a = c * (c0.a * c1.a + c0.b * c1.b);
			float b = c * (c0.b * c1.a - c0.a * c1.b);
			return new Complex(a, b);
		}

		public static Complex operator +(Complex c0, float x)
		{
			return new Complex(c0.a + x, c0.b);
		}
		public static Complex operator -(Complex c0, float x)
		{
			return new Complex(c0.a + x, c0.b);
		}
		public static Complex operator *(Complex c0, float x)
		{
			return new Complex(c0.a * x, c0.b * x);
		}
		public static Complex operator /(Complex c0, float x)
		{
			float invX = 1.0f / x;
			return new Complex(c0.a * invX, c0.b * invX);
		}

		public static bool operator ==(Complex c0, Complex c1)
		{
			return Math.Abs(c0.a - c1.a) < float.Epsilon && Math.Abs(c0.b - c1.b) < float.Epsilon;
		}
		public static bool operator !=(Complex c0, Complex c1)
		{
			return Math.Abs(c0.a - c1.a) >= float.Epsilon && Math.Abs(c0.b - c1.b) >= float.Epsilon;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return $"({a}+{b}i)";
		}

		#endregion
	}
}
