//#define UNSAFE
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaintDotNet;
using PaintDotNet.Effects;
using System.Xml.Serialization;
using System.IO;

namespace ArgusPaintNet.Convolution
{
	public class ConvolutionEffect : Effect<ConvolutionConfigEffectToken>
	{
		public static string StaticName { get { return "Convolution"; } }
		public static Image StaticIcon { get { return null; } }
		public static string StaticSubMenuName { get { return "Advanced"; } }

		public ConvolutionEffect()
			: base(StaticName, StaticIcon, StaticSubMenuName, EffectFlags.Configurable)
		{
		}

#if !DESIGNING
		public override EffectConfigDialog CreateConfigDialog()
		{
			return new ConvolutionEffectDialog();
		}
#endif

		protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
		{
			if (length < 1)
				return;

			foreach(Rectangle rect in renderRects)
			{
				if (this.IsCancelRequested)
					return;
				this.Render(rect, this.SrcArgs.Surface, this.DstArgs.Surface);
			}
		}

#if UNSAFE
		unsafe void Render(Rectangle rect, Surface srcSurface, Surface dstSurface)
#else
		void Render(Rectangle rect, Surface srcSurface, Surface dstSurface)
#endif
		{
			Matrix kernel = this.Token.Kernel;
			float factor = this.Token.Factor * kernel.GetNormalizationFactor();
			int kWidth = kernel.ColumnCount;
			int kHeight = kernel.RowCount;
			Rectangle srcBounds = this.EnvironmentParameters.SourceSurface.Bounds;
			for (int y = rect.Top; y < rect.Bottom; y++)
			{
				if (this.IsCancelRequested)
					return;

#if UNSAFE
				ColorBgra*[] pointers = new ColorBgra*[kHeight];
				for (int row = 0; row < kHeight; row++)
				{
					int sy = y + row - kHeight / 2;
					if (sy < 0 || sy >= srcBounds.Height)
					{
						pointers[row] = null;
						continue;
					}
					pointers[row] = srcSurface.GetPointAddress(0, sy);
				}
#endif

#if UNSAFE
				for (int x = 0; x < rect.Width; x++)
#else
				for (int x = rect.Left; x < rect.Right; x++)
#endif
				{
					float r = 0;
					float g = 0;
					float b = 0;

					for (int col = 0; col < kWidth; col++)
					{
						int sx = x + col - kWidth / 2;
						if (sx < 0 || sx >= srcBounds.Width)
							continue;
						for (int row = 0; row < kHeight; row++)
						{
#if UNSAFE
							if (pointers[row] == null)
								continue;
							ColorBgra px = pointers[row][sx];
#else
							int sy = y + row - kHeight / 2;
							if (sy < 0 || sy >= srcBounds.Height)
								continue;
							ColorBgra px = srcSurface[sx, sy];
#endif
							r += kernel[row, col] * px.R * factor;
							g += kernel[row, col] * px.G * factor;
							b += kernel[row, col] * px.B * factor;
						}
					}

					dstSurface[x, y] = ColorBgra.FromBgraClamped(Math.Abs(b), Math.Abs(g), Math.Abs(r), 255);
				}
			}
		}
	}
}
