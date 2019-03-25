using System;
using UnityEngine;

namespace ProceduralAnim
{
	public struct PAChainMapping
	{
		#region Constructors

		public PAChainMapping(PAChain _chain, PABodyMapping _bodyMapping, bool _isStart)
		{
			chain = _chain;
			bodyMapping = _bodyMapping;
			isStart = _isStart;
		}

		#endregion
		#region Fields

		public readonly PAChain chain;
		public readonly PABodyMapping bodyMapping;
		public readonly bool isStart;

		public static PAChainMapping None => new PAChainMapping(null, PABodyMapping.None, false);

		#endregion
	}
}
