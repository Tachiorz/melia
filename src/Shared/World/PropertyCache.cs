using System;
using System.Collections.Generic;

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

	public class PropertyAttribute : Attribute
	{
		public short Id { get; private set; }
	  	public PropertyOption Options { get; private set; }

		public PropertyAttribute(short id)
		{
			this.Id = id;
		}

		public PropertyAttribute(short id, PropertyOption options)
		{
			this.Id = id;
			this.Options = options;
		}
	}

	public struct Property
	{
		public PropertyType Type;
		public Delegate Getter;

		public Property(PropertyType type, Delegate getter)
		{
			Type = type;
			Getter = getter;
		}
	}

	public class PropertyValueCache
	{
		public float FloatValue;
		public int IntValue;
		public string StringValue;
	}

	public class PropertyCache
	{
		private Type _type;
		private static Dictionary<Type, Dictionary<short, Property>> _cache = new Dictionary<Type, Dictionary<short, Property>>();
		private Dictionary<short, PropertyValueCache> _valuesCache = new Dictionary<short, PropertyValueCache>();

		public void CacheProperties<T>() where T : GameObject
		{
			if (_type == null) _type = typeof(T);
			if (_cache.ContainsKey(_type)) return;

			_cache[_type] = new Dictionary<short, Property>();
			foreach (var propInfo in _type.GetProperties())
			{
				var attr = Attribute.GetCustomAttribute(propInfo, typeof(PropertyAttribute)) as PropertyAttribute;
				if (attr == null) continue;
				var m = propInfo.GetMethod;
				PropertyType propertyType;
				Delegate getter;
				if (m.ReturnType == typeof(string))
				{
					propertyType = PropertyType.String;
					getter = Delegate.CreateDelegate(typeof (Func<T, string>), null, m);
				}
				else if (m.ReturnType == typeof(int))
				{
					propertyType = PropertyType.Int;
					getter = Delegate.CreateDelegate(typeof(Func<T, int>), null, m);
				}
				else if (m.ReturnType == typeof(float))
				{
					propertyType = PropertyType.Float;
					getter = Delegate.CreateDelegate(typeof (Func<T, float>), null, m);
				}
				else throw new InvalidOperationException();
				_cache[_type][attr.Id] = new Property(propertyType, getter);
			}
		}

		public void GetPropererties<T>(GameObject obj, Action<short, float> floatCallback, Action<short, string> stringCallback, params short[] properties) where T : GameObject
		{
			this.CacheProperties<T>();
			if (_type == null) _type = typeof(T);

			Action<KeyValuePair<short, Property>> addProp = (prop) =>
			{
				if (!_valuesCache.ContainsKey(prop.Key)) _valuesCache[prop.Key] = new PropertyValueCache();
				var valCache = _valuesCache[prop.Key];
				switch (prop.Value.Type)
				{
					case PropertyType.String:
						var stringVal = ((Func<T, string>) (prop.Value.Getter))((T) obj);
						if (valCache.StringValue == stringVal) break;
						valCache.StringValue = stringVal;
						stringCallback(prop.Key, stringVal);
						break;
					case PropertyType.Int:
						var intVal = ((Func<T, int>) (prop.Value.Getter))((T) obj);
						if (valCache.IntValue == intVal) break;
						valCache.IntValue = intVal;
						floatCallback(prop.Key, intVal);
						break;
					case PropertyType.Float:
						var floatVal = ((Func<T, float>) (prop.Value.Getter))((T) obj);
						if (valCache.FloatValue == floatVal) break;
						valCache.FloatValue = floatVal;
						floatCallback(prop.Key, floatVal);
						break;
				}
			};

			if (properties == null || properties.Length == 0)
			{
				foreach (var p in _cache[_type])
					addProp(p);
			}
			else
			{
				foreach (var p in properties)
					addProp(new KeyValuePair<short, Property>(p, _cache[_type][p]));
			}
		}
	}
}