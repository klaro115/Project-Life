using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundGen
{
	[System.Serializable]
	public class SGGeneratorGraph : ScriptableObject
	{
		#region Fields

		private SGGeneratorNode startNode = null;
		private SGGeneratorNode endNode = null;
		[SerializeField]
		private List<SGGeneratorNode> nodes = new List<SGGeneratorNode>();

		public const int minAllowedSampleCount = 256;

		#endregion
		#region Properties

		public List<SGGeneratorNode> NodeList => nodes;

		#endregion
		#region Methods

		public void ClearGraph()
		{
			if(nodes != null)
			{
				// Clear buffers on all nodes, then cut ties between them:
				foreach(SGGeneratorNode node in nodes)
				{
					node.ClearOutputBuffer();
					node.ClearInputNodes();
				}
				nodes.Clear();
			}
		}

		public bool VerifyGraph(int minSampleCount)
		{
			// Get a baseline minimum sample count that all members of this graph must have prepared:
			minSampleCount = Mathf.Max(minSampleCount, minAllowedSampleCount);

			// Make sure the node list has been properly initialized:
			if (nodes == null)
			{
				Debug.LogWarning("Warning! Graph node list must not be null!");
				nodes = new List<SGGeneratorNode>();
			}

			// Verify all individual nodes in this graph:
			foreach(SGGeneratorNode node in nodes)
			{
				if (!node.VerifyNode(minSampleCount))
				{
					Debug.LogError($"Error! node verification for node of type '{node.GetType()}' has failed!");
					return false;
				}
			}

			return true;
		}

		public bool GenerateAudio(SGSoundGenerator gen, float[] sampleBuffer, ref float timestamp)
		{
			if (gen == null || sampleBuffer == null) return false;

			// Bind sample buffer to the end node's output:
			endNode.SetOutputBuffer(sampleBuffer);

			// 
			startNode.GenerateAudio(gen, timestamp);

			// Update timestamps on generator, then return success:
			timestamp += gen.duration;
			return true;
		}

		#endregion
	}
}
