using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using PaintDotNet;
using PaintDotNet.Rendering;
using PaintDotNet.Effects;
using System.Drawing;

namespace ArgusPaintNet.Shared
{
	[StructLayout( LayoutKind.Explicit)]
	public struct ArgusColor
	{
		#region Fields
		[FieldOffset(0)]
		byte a;

		[FieldOffset(1)]
		byte r;

		[FieldOffset(2)]
		byte g;

		[FieldOffset(3)]
		byte b;

		[FieldOffset(0)]
		uint argb;
		#endregion

		#region Properties
		public byte R { get { return this.r; } set { this.r = value; } }
		public byte G { get { return this.g; } set { this.g = value; } }
		public byte B { get { return this.b; } set { this.b = value; } }
		public byte A { get { return this.a; } set { this.a = value; } }
		public int ARGB { get { return (int)this.argb; } }
		#endregion

		#region Constructors
		public ArgusColor(byte r, byte g, byte b, byte a = 255)
			: this()
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public ArgusColor(uint argb)
			:this()
		{
			this.argb = argb;
		}

		public ArgusColor(int argb)
			:this()
		{
			this.argb = (uint)argb;
		}

		public ArgusColor(RgbColorF color)
			: this()
		{
			this.R = (byte)Math.Round(color.Red * 255);
			this.G = (byte)Math.Round(color.Green * 255);
			this.B = (byte)Math.Round(color.Blue * 255);
			this.A = 255;
		}

		public ArgusColor(HsvColorF color)
			:this(color.ToRgbColorF())
		{ }

		public RgbColorF ToRgbColorF()
		{
			return new RgbColorF(this.R / 255.0, this.G / 255.0, this.B / 255.0);
		}

		public HsvColorF ToHsvColorF()
		{
			return this.ToRgbColorF().ToHsvColorF();
		}

		public static ArgusColor FromARGB(int argb)
		{
			return new ArgusColor(argb);
		}

		public static ArgusColor FromARGB(uint argb)
		{
			return new ArgusColor(argb);
		}

		public static ArgusColor FromRGB(int rgb)
		{
			return new ArgusColor(0xFF000000 | (uint)rgb);
		}

		public static ArgusColor FromRGBColorF(RgbColorF color)
		{
			return new ArgusColor(color);
		}

		public static ArgusColor FromHSVColorF(HsvColorF color)
		{
			return new ArgusColor(color);
		}
		#endregion

		#region Operators
		public static implicit operator uint (ArgusColor color)
		{
			return color.argb;
		}

		public static implicit operator ArgusColor(uint argb)
		{
			return new ArgusColor(argb);
		}

		public static implicit operator int(ArgusColor color)
		{
			return color.ARGB;
		}

		public static implicit operator ArgusColor(int argb)
		{
			return new ArgusColor(argb);
		}

		public static implicit operator ColorBgra(ArgusColor color)
		{
			return ColorBgra.FromBgra(color.B, color.G, color.R, color.A);
		}

		public static implicit operator ArgusColor(ColorBgra color)
		{
			return new ArgusColor(color.R, color.G, color.B, color.A);
		}

		public static implicit operator Color(ArgusColor color)
		{
			return Color.FromArgb(color.ARGB);
		}

		public static implicit operator ArgusColor(Color color)
		{
			return new ArgusColor(color.R, color.G, color.B, color.A);
		}

		public static implicit operator RgbColorF(ArgusColor color)
		{
			return color.ToRgbColorF();
		}

		public static explicit operator ArgusColor(RgbColorF color)
		{
			return new ArgusColor(color);
		}

		public static implicit operator HsvColorF(ArgusColor color)
		{
			return color.ToHsvColorF();
		}

		public static explicit operator ArgusColor(HsvColorF color)
		{
			return new ArgusColor(color);
		}
		#endregion

		#region ReverseBlend
		public ArgusColor ReverseBlend(ArgusColor background)
		{
			double a = ReverseBlend_GetAlpha(this, background);
			return ReverseBlend_GetColor(a, this, background);
		}

		static double ReverseBlend_GetAlpha(ArgusColor blend, ArgusColor background)
		{
			if (background.A < 255)
				return Math.Max(0, (blend.A - background.A) / (255.0 - background.A));

			double a = ReverseBlend_GetAlpha(blend.R, background.R);
			a = Math.Max(a, ReverseBlend_GetAlpha(blend.G, background.G));
			a = Math.Max(a, ReverseBlend_GetAlpha(blend.B, background.B));
			return a;
		}

		static double ReverseBlend_GetAlpha(byte blend, byte background)
		{
			double a = blend - background;
			if (a == 0)
				return 0;
			if (a > 0)
				return a / (255 - background);
			else
				return -a / background;
		}

		static byte ReverseBlend_GetColor(double a_f, byte blend, byte background)
		{
			if (a_f == 0)
				return 0;

			return (byte)Math.Round((blend - background) / a_f + background);
		}

		static ArgusColor ReverseBlend_GetColor(double a_f, ArgusColor blend, ArgusColor background)
		{
			if (a_f == 0)
				return new ArgusColor();

			ArgusColor RetVal = new ArgusColor();
			RetVal.A = (byte)Math.Round(a_f * 255);
			if (background.A < 255)
			{
				double a = (blend.A / 255.0) * a_f;
				double a_b = (1 - a_f) * (background.A / 255.0) / a_f;
				RetVal.R = (byte)Math.Round(blend.R * a - a_b * background.R);
				RetVal.G = (byte)Math.Round(blend.G * a - a_b * background.G);
				RetVal.B = (byte)Math.Round(blend.B * a - a_b * background.B);
			}
			else
			{
				RetVal.R = ReverseBlend_GetColor(a_f, blend.R, background.R);
				RetVal.G = ReverseBlend_GetColor(a_f, blend.G, background.G);
				RetVal.B = ReverseBlend_GetColor(a_f, blend.B, background.B);
			}

			return RetVal;
		}
		#endregion
	}
}
