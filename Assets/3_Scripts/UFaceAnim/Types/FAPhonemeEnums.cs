
namespace UFaceAnim
{
	public enum FABasePhonemes
	{
		None,			// No phoneme or pause/silence.

		Group0_AI,		// Sounds: A, I
		Group1_E,		// Sounds: E
		Group2_U,		// Sounds: U
		Group3_O,		// Sounds: O
		Group4_CDGK,	// Sounds: C, D, G, K, N, R, S, Y, Z
		Group5_FV,		// Sounds: F, V
		Group6_LTh,		// Sounds: L, th
		Group7_MBP,		// Sounds: M, B, P
		Group8_WQ,		// Sounds: W, Q
		Group9_Rest		// Sounds: the entire rest.

		// NOTE: The above segregation is according to some lip reading sample image I found on pinterest.
		// No warranties are given concerning the validity, reliability and completeness of the selection!
	}
}
