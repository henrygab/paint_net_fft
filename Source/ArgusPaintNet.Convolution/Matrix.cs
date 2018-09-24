using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace ArgusPaintNet.Convolution
{
	public class Matrix : IXmlSerializable
	{
        private float[,] _matrix;

		public int RowCount { get { return this._matrix.GetLength(0); } }
		public int ColumnCount { get { return this._matrix.GetLength(1); } }

		public Matrix()
			: this(new float[0, 0])
		{ }

		public Matrix(float[,] matrix)
		{
			if (matrix == null)
				throw new ArgumentNullException();
			this._matrix = matrix;
		}

		public Matrix(int rows, int columns)
		{
			this._matrix = new float[rows, columns];
		}

		public Matrix(Matrix copy)
			: this((float[,])copy)
		{
		}

		public float this[int row, int column]
		{
			get { return this._matrix[row, column]; }
			set { this._matrix[row, column] = value; }
		}

		public float GetNormalizationFactor()
		{
			double pos = 0;
			double neg = 0;
			foreach (double val in this._matrix)
			{
				if (val < 0)
					neg += val;
				else
					pos += val;
			}

			double factor = 1;
			double sum = Math.Abs(neg + pos);
			if (sum > float.Epsilon)
			{
				factor = 1 / sum;
			}
			else if (pos > float.Epsilon)
			{
				factor = 1 / pos;
			}

			if (factor == 0 || double.IsNaN(factor) || double.IsInfinity(factor))
				factor = 1;
			return (float)factor;
		}

		public static implicit operator Matrix(float[,] matrix)
		{
			return new Matrix(matrix);
		}

		public static implicit operator float[,](Matrix matrix)
		{
			float[,] RetVal = new float[matrix.RowCount, matrix.ColumnCount];
			Array.Copy(matrix._matrix, RetVal, RetVal.Length);
			return RetVal;
		}

		public string[] RowsToString(IFormatProvider formatProvider)
		{
			string[,] str = new string[this.RowCount, this.ColumnCount];
			int[] len = new int[this.ColumnCount];
			for (int c = 0; c < this.ColumnCount; c++)
			{
				len[c] = 0;
				for (int r = 0; r < this.RowCount; r++)
				{
					str[r, c] = this[r, c].ToString(formatProvider);
					len[c] = Math.Max(len[c], str[r,c].Length);
				}

				for (int r = 0; r < this.RowCount; r++)
				{
					int d = len[c] - str[r, c].Length;
					if (d > 0)
						str[r, c] = new string(' ', d) + str[r, c];
				}
			}

			var sb = new StringBuilder();
			string[] RetVal = new string[this.RowCount];

			for (int r = 0; r < this.RowCount; r++)
			{
				sb.Clear();
				for (int c = 0; c < this.ColumnCount; c++)
				{
					sb.Append(str[r, c]);
					sb.Append(' ');
				}
				sb.Remove(sb.Length - 1, 1);
				RetVal[r] = sb.ToString();
			}

			return RetVal;
		}

		public string[] RowsToString() { return this.RowsToString(CultureInfo.CurrentCulture); }

		public string ToString(IFormatProvider formatProvider)
		{
			string[] rows = this.RowsToString(formatProvider);
			int capacity = 0;
			for (int i = 0; i < rows.Length; i++)
				capacity += rows[i].Length + 4;
			var sb = new StringBuilder(capacity);
			foreach (string row in rows)
				sb.AppendLine(row);
			return sb.ToString().TrimEnd();
		}

		public override string ToString()
		{
			return this.ToString(NumberFormatInfo.CurrentInfo);
		}

		public static bool TryParse(string[] rows, IFormatProvider formatProvider, out Matrix value)
		{
			value = null;
			if (rows == null || rows.Length < 1)
				return false;

			for (int r = 0; r < rows.Length; r++)
			{
				string[] cells = rows[r].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (value == null)
					value = new Matrix(rows.Length, cells.Length);
				else if (value.ColumnCount != cells.Length)
					return false;

				for (int c = 0; c < cells.Length; c++)
				{
					float val;
					if (!float.TryParse(cells[c].Trim(), NumberStyles.Number, formatProvider, out val))
						return false;
					value[r, c] = val;
				}
			}
			return true;
		}

		public static bool TryParse(string[] rows, out Matrix value)
		{
			return Matrix.TryParse(rows, NumberFormatInfo.CurrentInfo, out value);
		}

		public static bool TryParse(string text, IFormatProvider formatProvider, out Matrix value)
		{
			string[] rows = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			return Matrix.TryParse(rows, formatProvider, out value);
		}

		public static bool TryParse(string text, out Matrix value)
		{
			return Matrix.TryParse(text, NumberFormatInfo.CurrentInfo, out value);
		}

		XmlSchema IXmlSerializable.GetSchema() { return null; }

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			string[] rows = this.RowsToString(NumberFormatInfo.InvariantInfo);
			foreach (string row in rows)
			{
				writer.WriteElementString("Row", row);
			}
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			var rows = new List<string>();
			string name = reader.LocalName;
			while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != name)
			{
				if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Row")
					rows.Add(reader.ReadElementContentAsString());
				else
					reader.Read();
			}
			reader.ReadEndElement();

			Matrix matrix;
			if (!Matrix.TryParse(rows.ToArray(), NumberFormatInfo.InvariantInfo, out matrix))
				throw new XmlException("Xml content cannot be converted to Matrix.");
			this._matrix = matrix._matrix;
		}
	}
}
