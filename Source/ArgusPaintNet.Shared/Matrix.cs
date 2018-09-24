using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace ArgusPaintNet.Shared
{
	/// <summary>
	/// This class is designed to hold small matrices typically used as kernels in image processing (3x3, 5x5, etc.)
	/// and therefore is NOT optimized for speed: No complex, optimized algorithms (e.g. for Matrix Multiplication),
	/// no parallel computing, etc.
	/// </summary>
	public class Matrix : IXmlSerializable
	{
        private float[,] _matrix;

        public int RowCount => this._matrix.GetLength(0);
        public int ColumnCount => this._matrix.GetLength(1);

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

		public Matrix(int rows, int columns, float[] values)
		{
			if (values == null)
				throw new ArgumentNullException();
			if (rows * columns != values.Length)
				throw new ArgumentException("values must contain rows*columns elements.");
			this._matrix = new float[rows, columns];
			Buffer.BlockCopy(values, 0, this._matrix, 0, values.Length * sizeof(float));
		}

		public Matrix(Matrix copy)
			: this((float[,])copy)
		{
		}

		public float this[int row, int column]
        {
            get => this._matrix[row, column];
            set => this._matrix[row, column] = value;
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

		public void Multiply(float factor)
		{
			for (int row = 0; row < this.RowCount; row++)
			{
				for (int col = 0; col < this.ColumnCount; col++)
				{
					this[row, col] *= factor;
				}
			}
		}

		public static Matrix operator *(float factor,Matrix m)
		{
			var RetVal = new Matrix(m);
			RetVal.Multiply(factor);
			return RetVal;
		}

		public static Matrix operator*(Matrix m, float factor)
		{
			return factor * m;
		}

		public static Matrix operator*(Matrix m1, Matrix m2)
		{
			if (m1.ColumnCount != m2.RowCount)
				throw new ArgumentException("m1.ColumnCount must be equal to m2.RowCount.");
			var RetVal = new Matrix(m1.RowCount, m2.ColumnCount);
			for (int row = 0; row < RetVal.RowCount; row++)
			{
				for (int col = 0; col < RetVal.ColumnCount; col++)
				{
					float value = 0;
					for (int i = 0; i < m1.ColumnCount; i++)
						value += m1[row, i] * m2[i, col];
					RetVal[row, col] = value;
				}
			}
			return RetVal;
		}

		public void Add(Matrix m)
		{
			if (this.ColumnCount != m.ColumnCount || this.RowCount != m.ColumnCount)
				throw new ArgumentException("Dimension mismatch.");

			for (int row = 0; row < this.RowCount; row++)
			{
				for (int col = 0; col < this.ColumnCount; col++)
				{
					this[row, col] += m[row, col];
				}
			}
		}

		public static Matrix operator+(Matrix m1,Matrix m2)
		{
			if (m1.ColumnCount != m2.ColumnCount || m1.RowCount != m2.ColumnCount)
				throw new ArgumentException("Dimension mismatch.");

			var RetVal = new Matrix(m1);
			RetVal.Add(m2);
			return RetVal;
		}

		public void Subtract(Matrix m)
		{
			if (this.ColumnCount != m.ColumnCount || this.RowCount != m.ColumnCount)
				throw new ArgumentException("Dimension mismatch.");

			for (int row = 0; row < this.RowCount; row++)
			{
				for (int col = 0; col < this.ColumnCount; col++)
				{
					this[row, col] -= m[row, col];
				}
			}
		}

		public static Matrix operator -(Matrix m1, Matrix m2)
		{
			if (m1.ColumnCount != m2.ColumnCount || m1.RowCount != m2.ColumnCount)
				throw new ArgumentException("Dimension mismatch.");

			var RetVal = new Matrix(m1);
			RetVal.Subtract(m2);
			return RetVal;
		}

        private float[] GetValues()
		{
			float[] RetVal = new float[this._matrix.Length];
			Buffer.BlockCopy(this._matrix, 0, RetVal, 0, RetVal.Length * sizeof(float));
			return RetVal;
		}

		public Matrix GetTransposed()
		{
			var RetVal = new Matrix(this.ColumnCount, this.RowCount);
			Buffer.BlockCopy(this._matrix, 0, RetVal._matrix, 0, sizeof(float)*this._matrix.Length);
			return RetVal;
		}

		public Matrix GetNormalized()
		{
			float factor = this.GetNormalizationFactor();
			return factor * this;
		}

		public static bool operator ==(Matrix m1, Matrix m2)
		{
			if (object.ReferenceEquals(m1, m2))
				return true;

			if (object.ReferenceEquals(m1, null))
				return false;
			if (object.ReferenceEquals(m2, null))
				return false;

			if (m1.RowCount != m2.RowCount || m1.ColumnCount != m2.ColumnCount)
				return false;

			for (int row = 0; row < m1.RowCount; row++)
			{
				for (int col = 0; col < m1.ColumnCount; col++)
				{
					if (m1[row, col] != m2[row, col])
						return false;
				}
			}
			return true;
		}

		public static bool operator !=(Matrix m1, Matrix m2)
		{
			return !(m1 == m2);
		}

		public override bool Equals(object obj)
		{
			var m = obj as Matrix;
			if (m == null)
				return false;
			return (this == m);
		}
	}
}
