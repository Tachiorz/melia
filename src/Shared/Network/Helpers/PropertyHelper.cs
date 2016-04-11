using System;
using Melia.Shared.World;

namespace Melia.Shared.Network.Helpers
{
	public static class PropertyHelper
	{
		/// <summary>
		/// Add propeties to the packet, return property count.
		/// </summary>
		public static int AddProperties<T>(this Packet packet, GameObject obj, params short[] properties) where T : GameObject
		{
			var count = 0;
			Action<short, float> floatCallback = (id, val) =>
			{
				packet.PutShort(id);
				packet.PutFloat(val);
				count++;
			};

			Action<short, string> stringCallback = (id, val) =>
			{
				packet.PutShort(id);
				packet.PutLpString(val);
				count++;
			};
			obj.PropertyCache.GetPropererties<T>(obj, floatCallback, stringCallback, properties);
			return count;
		}
	}
}
