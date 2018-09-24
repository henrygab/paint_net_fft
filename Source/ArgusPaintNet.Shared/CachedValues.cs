using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ArgusPaintNet.Shared
{
	public class CachedValues<T>
	{
        private Dictionary<Rectangle, T[,]> _dict;
        private readonly Action<Rectangle, T[,]> _actionFill;
        private readonly int capacity;

		public CachedValues(int capacity, Action<Rectangle,T[,]> fillMethod)
		{
			if (fillMethod == null)
				throw new ArgumentNullException();
			this._actionFill = fillMethod;
			this.capacity = capacity;
			this.Invalidate();
		}

		public void Invalidate()
		{
			lock(this)
			{
				this._dict = new Dictionary<Rectangle, T[,]>(this.capacity);
			}
		}

        private bool InitValue(Rectangle roi, out T[,] value)
		{
			lock(this)
			{
				if (!this._dict.TryGetValue(roi, out value))
				{
					value = new T[roi.Width, roi.Height];
					this._dict.Add(roi, value);
					return true;
				}
				return false;
			}
		}

		public T[,] GetValues(Rectangle roi)
		{
			T[,] RetVal;
			if (this.InitValue(roi,out RetVal))
			{
				this._actionFill(roi, RetVal);
			}
			return RetVal;
		}
	}
}
