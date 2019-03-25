using System;

namespace ProceduralAnim
{
	public struct PAEndpointMapping
	{
		#region Constructors

		public PAEndpointMapping(PAEndpoint _endpoint, PAEndpointType _type, PABodyMapping _bodyMapping)
		{
			endpoint = _endpoint;
			type = _type;
			bodyMapping = _bodyMapping;
		}

		#endregion
		#region Fields

		public readonly PAEndpoint endpoint;
		public readonly PAEndpointType type;
		public readonly PABodyMapping bodyMapping;

		#endregion
		#region Properties

		public static PAEndpointMapping None => new PAEndpointMapping(null, PAEndpointType.Pointer, PABodyMapping.None);

		#endregion
	}
}
