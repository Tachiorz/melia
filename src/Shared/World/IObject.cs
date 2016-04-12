using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Melia.Shared.Const;
using Melia.Shared.Network;
using Melia.Shared.Util;

namespace Melia.Shared.World
{
	public enum PropertyType
	{
		STRING,
		INT,
		FLOAT
	}

	public class PropertyCache
	{
		public PropertyType type;
		public Delegate get;
	}

	public class PropertyAttribute : Attribute
	{
		public short Id { get; private set; }
		public PropertyType Type { get; private set; }

		public PropertyAttribute(short Id, PropertyType Type)
		{
			this.Id = Id;
			this.Type = Type;
		}
	}



	public class IObject
	{
		/// <summary>
		/// Object UID
		/// </summary>
		public long Id { get; set; }
		private bool _isPropertiesCached = false;
		private Dictionary<short, PropertyCache> _propertyCache;

		public void PutPropererties(PacketBuffer buf, params short[] properties)
		{
			if (!_isPropertiesCached)
			{
				_propertyCache = new Dictionary<short, PropertyCache>();
				var T = this.GetType();
				foreach (var propInfo in T.GetProperties())
				{
					var attr = Attribute.GetCustomAttribute(propInfo, typeof (PropertyAttribute)) as PropertyAttribute;
					if (attr == null) continue;
					_propertyCache[attr.Id] = new PropertyCache() { type = attr.Type };
					if (attr.Type == PropertyType.STRING)
						_propertyCache[attr.Id].get = Delegate.CreateDelegate(typeof(Func<string>), this, propInfo.GetMethod);
					else if (attr.Type == PropertyType.INT || attr.Type == PropertyType.FLOAT)
					{
						if (propInfo.GetMethod.ReturnType == typeof (int))
						{
							_propertyCache[attr.Id].type = PropertyType.INT;
							_propertyCache[attr.Id].get = Delegate.CreateDelegate(typeof(Func<int>), this, propInfo.GetMethod);
						}
						else if (propInfo.GetMethod.ReturnType == typeof (float))
						{
							_propertyCache[attr.Id].type = PropertyType.FLOAT;
							_propertyCache[attr.Id].get = Delegate.CreateDelegate(typeof (Func<float>), this, propInfo.GetMethod);
						}
					}
				}
			}

			Func<PacketBuffer, KeyValuePair<short, PropertyCache>, bool> addProp = (b, prop) =>
			{
				if (prop.Value.type == PropertyType.STRING)
				{
					b.PutShort(prop.Key);
					b.PutLpString(((Func<string>)(prop.Value.get))());
				}
				else if (prop.Value.type == PropertyType.INT)
				{
					b.PutShort(prop.Key);
					b.PutFloat(((Func<int>)(prop.Value.get))());
				}
				else if (prop.Value.type == PropertyType.FLOAT)
				{
					b.PutShort(prop.Key);
					b.PutFloat(((Func<float>)(prop.Value.get))());
				}
				return true;
			};

			if (properties == null || properties.Length == 0)
			{
				foreach (var p in _propertyCache)
					addProp(buf, p);
			}
			else
			{
				foreach (var p in properties)
					addProp(buf, new KeyValuePair<short, PropertyCache>(p, _propertyCache[p]));
			}
		}
	}
}
