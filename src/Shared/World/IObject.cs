using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

	public class PropertyValueCache
	{
		public float FloatValue;
		public int IntValue;
		public string StringValue;
	}

	public class PropertyCache
	{
		public PropertyType Type;
		public Delegate Get;
		public ConditionalWeakTable<IObject, PropertyValueCache> PropetyValueCache;

		public PropertyCache(PropertyType type)
		{
			this.Type = type;
			this.PropetyValueCache = new ConditionalWeakTable<IObject, PropertyValueCache>();
		}
	}

	public class PropertyAttribute : Attribute
	{
		public short Id { get; private set; }
		public PropertyType Type { get; private set; }

		public PropertyAttribute(short Id)
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
		private Guid _cachedType = Guid.Empty;
		private static Dictionary<Guid, Dictionary<short, PropertyCache>> _propertyCache = new Dictionary<Guid, Dictionary<short, PropertyCache>>();

		public void CacheProperties<T>() where T : IObject
		{
			if (_cachedType == Guid.Empty) _cachedType = typeof(T).GUID;
			foreach (var propInfo in this.GetType().GetProperties())
			{
				var attr = Attribute.GetCustomAttribute(propInfo, typeof(PropertyAttribute)) as PropertyAttribute;
				if (attr == null) continue;
				if (!_propertyCache.ContainsKey(_cachedType)) _propertyCache[_cachedType] = new Dictionary<short, PropertyCache>();
				_propertyCache[_cachedType][attr.Id] = new PropertyCache(attr.Type);
				MethodInfo m = propInfo.GetMethod;
				if (m.ReturnType == typeof(string))
				{
					_propertyCache[_cachedType][attr.Id].Type = PropertyType.STRING;
					_propertyCache[_cachedType][attr.Id].Get = Delegate.CreateDelegate(typeof (Func<T, string>), null, m);
				}
				else if (m.ReturnType == typeof(int))
				{
					_propertyCache[_cachedType][attr.Id].Type = PropertyType.INT;
					_propertyCache[_cachedType][attr.Id].Get = Delegate.CreateDelegate(typeof(Func<T, int>), null, m);
				}
				else if (m.ReturnType == typeof(float))
				{
					_propertyCache[_cachedType][attr.Id].Type = PropertyType.FLOAT;
					_propertyCache[_cachedType][attr.Id].Get = Delegate.CreateDelegate(typeof (Func<T, float>), null, m);
				}
				else throw new InvalidOperationException();
			}
		}

		public void PutPropererties<T>(PacketBuffer buf, params short[] properties) where T : IObject
		{
			if (_cachedType == Guid.Empty)
			{
				_cachedType = typeof(T).GUID;
				if (!_propertyCache.ContainsKey(_cachedType)) CacheProperties<T>();
			}

			Func<PacketBuffer, KeyValuePair<short, PropertyCache>, bool> addProp = (b, prop) =>
			{
				var propValCache = prop.Value.PropetyValueCache.GetOrCreateValue(this);
				switch (prop.Value.Type)
				{
					case PropertyType.STRING:
						var stringVal = ((Func<T, string>) (prop.Value.Get))((T) this);
						if (propValCache.StringValue == stringVal) break;
						propValCache.StringValue = stringVal;
						b.PutShort(prop.Key);
						b.PutLpString(stringVal);
						break;
					case PropertyType.INT:
						var intVal = ((Func<T, int>) (prop.Value.Get))((T) this);
						if (propValCache.IntValue == intVal) break;
						propValCache.IntValue = intVal;
						b.PutShort(prop.Key);
						b.PutFloat(intVal);
						break;
					case PropertyType.FLOAT:
						var floatVal = ((Func<T, float>) (prop.Value.Get))((T) this);
						// todo: i'm not sure if TOS sends fractional floats, I guess not and we don't even need CachedFloatValue
						if ((int)propValCache.FloatValue == (int)floatVal) break;
						propValCache.FloatValue = floatVal;
						b.PutShort(prop.Key);
						b.PutFloat(floatVal);
						break;
				}
				return true;
			};

			if (properties == null || properties.Length == 0)
			{
				foreach (var p in _propertyCache[_cachedType])
					addProp(buf, p);
			}
			else
			{
				foreach (var p in properties)
					addProp(buf, new KeyValuePair<short, PropertyCache>(p, _propertyCache[_cachedType][p]));
			}
		}
	}
}
