using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Reflection;

namespace ArgusPaintNet.Shared
{
	public class EnumDropDownValues<T> where T : struct
	{
		Dictionary<object, T> _dict1;
		Dictionary<T, object> _dict2;

		public EnumDropDownValues()
		{
			if (!typeof(T).IsEnum)
				throw new InvalidOperationException("T is expected to be an Enum.");

			T[] values = (T[])Enum.GetValues(typeof(T));
			this._dict1 = new Dictionary<object, T>(values.Length);
			this._dict2 = new Dictionary<T, object>(values.Length);
			Assembly assembly = typeof(T).Assembly;
			string name = assembly.GetName().Name;
            ResourceManager rm = new ResourceManager(name + ".Properties.Resources", assembly);
			string prefix = typeof(T).Name + '_';
			foreach (T val in values)
			{
				string text = rm.GetString(prefix + val.ToString());
				if (string.IsNullOrEmpty(text))
					text = val.ToString();
				this._dict1.Add(text, val);
				this._dict2.Add(val, text);
			}
		}

		public object[] Values
		{
			get
			{
				object[] values = new object[this._dict1.Count];
				this._dict1.Keys.CopyTo(values, 0);
				return values;
			}
		}

		public object[] GetValues(params T[] leaveOut)
		{
			if (leaveOut == null || leaveOut.Length < 1)
				return this.Values;

			List<object> values = new List<object>(_dict1.Count);
			List<T> lOut = new List<T>(leaveOut);
			foreach (KeyValuePair<T, object> item in _dict2)
			{
				if (!lOut.Contains(item.Key))
					values.Add(item.Value);
			}
			return values.ToArray();
		}

		public bool GetEnumMember(object val, out T RetVal)
		{
			return this._dict1.TryGetValue(val, out RetVal);
		}

		public T GetEnumMember(object val)
		{
			T RetVal;
			if (!this._dict1.TryGetValue(val, out RetVal))
				return new T();
			return RetVal;
		}

		public object GetValue(T val)
		{
			object RetVal;
			if (!this._dict2.TryGetValue(val, out RetVal))
				return null;
			return RetVal;
		}
	}
}
