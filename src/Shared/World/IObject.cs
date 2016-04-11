using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Melia.Shared.Network;
using Melia.Shared.Util;

namespace Melia.Shared.World
{

	public class ObjectProperty<T>
	{
		protected T _value;

		public bool IsDirty { get; private set; }

		public ObjectProperty(T val)
		{
			if (!EqualityComparer<T>.Default.Equals(_value, val))
			{
				_value = val;
				this.IsDirty = true;
			}
		}

		public void Clean()
		{
			this.IsDirty = false;
		}

		public static implicit operator T(ObjectProperty<T> prop)
		{
			return prop._value;
		}

		public static implicit operator ObjectProperty<T>(T val)
		{
			return new ObjectProperty<T>(val);
		}
	}

	public class IntProperty : ObjectProperty<int>
	{
		public IntProperty(int val) : base(val) {}

		public static implicit operator float(IntProperty prop)
		{
			return prop._value;
		}

		public static implicit operator IntProperty(float val)
		{
			return new IntProperty((int)val);
		}
	}

	public class StringProperty : ObjectProperty<string>
	{
		public StringProperty(string val) : base(val) { }
	}


	public enum PropertyType
	{
		INT,
		STRING
	}

	public class PropertyAttribute : Attribute
	{
		public int Id { get; private set; }
		public PropertyType Type { get; private set; }

		public PropertyAttribute(int Id, PropertyType Type)
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

		public void PutChangedPropererties(PacketBuffer buf)
		{
			var T = this.GetType();
			foreach (var propInfo in T.GetProperties())
			{
				var attr = Attribute.GetCustomAttribute(propInfo, typeof(PropertyAttribute)) as PropertyAttribute;
				if (attr == null) continue;
				if (attr.Type == PropertyType.STRING)
				{
					StringProperty p = propInfo.GetValue(this) as StringProperty;
					if (p.IsDirty)
					{
						buf.PutShort(attr.Id);
						buf.PutLpString(p);
						p.Clean();
					}
				}
				else if (attr.Type == PropertyType.INT)
				{
					IntProperty p = propInfo.GetValue(this) as IntProperty;
					if (p.IsDirty)
					{
						buf.PutShort(attr.Id);
						buf.PutFloat(p);
						p.Clean();
					}
				}
			}
		}
	}
}
