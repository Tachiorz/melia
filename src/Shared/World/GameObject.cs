using System;
using System.Collections.Generic;
using System.Reflection;
using Melia.Shared.Network;

namespace Melia.Shared.World
{
	public enum PropertyType
	{
		String,
		Int,
		Float
	}

  	[Flags]
    public enum PropertyOption
    {
    	Calculated = 1, // Property calculated at runtime using getter, no need to save to DB
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
	}

	public class PropertyAttribute : Attribute
	{
		public short Id { get; private set; }
		public PropertyType Type { get; private set; }
	  	public PropertyOption Options { get; private set; }

		public PropertyAttribute(short id)
		{
			this.Id = id;
		}

		public PropertyAttribute(short id, PropertyOption options)
		{
			this.Id = Id;
			this.Options = options;
		}
	}

	public class GameObject
	{
		/// <summary>
		/// Object UID
		/// </summary>
		public long Id { get; set; }
		private Guid _cachedType = Guid.Empty;
		private static Dictionary<Guid, Dictionary<short, PropertyCache>> _propertyCache = new Dictionary<Guid, Dictionary<short, PropertyCache>>();
		private Dictionary<short, PropertyValueCache> _propertyValuesCache;

		public void CacheProperties<T>() where T : GameObject
		{
			if (_cachedType == Guid.Empty) _cachedType = typeof(T).GUID;
			foreach (var propInfo in this.GetType().GetProperties())
			{
				var attr = Attribute.GetCustomAttribute(propInfo, typeof(PropertyAttribute)) as PropertyAttribute;
				if (attr == null) continue;
				if (!_propertyCache.ContainsKey(_cachedType)) _propertyCache[_cachedType] = new Dictionary<short, PropertyCache>();
				_propertyCache[_cachedType][attr.Id] = new PropertyCache {Type = attr.Type};
				MethodInfo m = propInfo.GetMethod;
				if (m.ReturnType == typeof(string))
				{
					_propertyCache[_cachedType][attr.Id].Type = PropertyType.String;
					_propertyCache[_cachedType][attr.Id].Get = Delegate.CreateDelegate(typeof (Func<T, string>), null, m);
				}
				else if (m.ReturnType == typeof(int))
				{
					_propertyCache[_cachedType][attr.Id].Type = PropertyType.Int;
					_propertyCache[_cachedType][attr.Id].Get = Delegate.CreateDelegate(typeof(Func<T, int>), null, m);
				}
				else if (m.ReturnType == typeof(float))
				{
					_propertyCache[_cachedType][attr.Id].Type = PropertyType.Float;
					_propertyCache[_cachedType][attr.Id].Get = Delegate.CreateDelegate(typeof (Func<T, float>), null, m);
				}
				else throw new InvalidOperationException();
			}
		}

		public void PutPropererties<T>(PacketBuffer buf, params short[] properties) where T : GameObject
		{
			if (_cachedType == Guid.Empty)
			{
				_cachedType = typeof(T).GUID;
				_propertyValuesCache = new Dictionary<short, PropertyValueCache>();
				if (!_propertyCache.ContainsKey(_cachedType)) CacheProperties<T>();
			}

			Func<PacketBuffer, KeyValuePair<short, PropertyCache>, bool> addProp = (b, prop) =>
			{
				if (!_propertyValuesCache.ContainsKey(prop.Key)) _propertyValuesCache[prop.Key] = new PropertyValueCache();
				var valCache = _propertyValuesCache[prop.Key];
				switch (prop.Value.Type)
				{
					case PropertyType.String:
						var stringVal = ((Func<T, string>) (prop.Value.Get))((T) this);
						if (valCache.StringValue == stringVal) break;
						valCache.StringValue = stringVal;
						b.PutShort(prop.Key);
						b.PutLpString(stringVal);
						break;
					case PropertyType.Int:
						var intVal = ((Func<T, int>) (prop.Value.Get))((T) this);
						if (valCache.IntValue == intVal) break;
						valCache.IntValue = intVal;
						b.PutShort(prop.Key);
						b.PutFloat(intVal);
						break;
					case PropertyType.Float:
						var floatVal = ((Func<T, float>) (prop.Value.Get))((T) this);
						if (valCache.FloatValue == floatVal) break;
						valCache.FloatValue = floatVal;
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
