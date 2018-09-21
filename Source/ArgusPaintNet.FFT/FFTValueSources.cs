using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Resources;
using System.Numerics;
using ArgusPaintNet.FFT.FFTWInterop;
using PaintDotNet.Rendering;
using ArgusPaintNet.Shared;

namespace ArgusPaintNet.FFT
{
	public enum ValueSources
	{
		Intensity,
		Red,
		Green,
		Blue
	}

	static class ValueSourcesExtensions
	{
		public static Func<ColorBgra, double> GetGetValueFunc(this ValueSources valueSource)
		{
			if (valueSource == ValueSources.Red)
				return (color) => { return color.R / 255.0; };
			if (valueSource == ValueSources.Green)
				return (color) => { return color.G / 255.0; };
			if (valueSource == ValueSources.Blue)
				return (color) => { return color.B / 255.0; };

			//if (valueSource == ValueSources.Intensity)
			return (color) => { return color.GetIntensity(); };
		}

		public static Func<double, ColorBgra> GetGetColorFunc(this ValueSources valueSource)
		{
			if (valueSource  == ValueSources.Red)
				return (red) => { return ColorBgra.FromBgr(0, 0, (byte)Math.Round(255 * red)); };
			if (valueSource == ValueSources.Green)
				return (green) => { return ColorBgra.FromBgr(0, (byte)Math.Round(255 * green), 0); };
			if (valueSource == ValueSources.Blue)
				return (blue) => { return ColorBgra.FromBgr((byte)Math.Round(255 * blue), 0, 0); };

			//if (valueSource == ValueSources.Intensity)
			return (intensity) =>
			{
				byte i = (byte)Math.Round(255 * intensity);
				return ColorBgra.FromBgr(i, i, i);
			};
		}
	}
}
