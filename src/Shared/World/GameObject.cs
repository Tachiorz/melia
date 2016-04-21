using System;
using Melia.Shared.Network;

namespace Melia.Shared.World
{
	public class GameObject
	{
		/// <summary>
		/// Object unique id
		/// </summary>
		public long Id { get; set; }

		public PropertyCache PropertyCache { get; private set; } = new PropertyCache();
	}
}
