using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaintDotNet;
using System.Drawing;
using PaintDotNet.Effects;

namespace ArgusPaintNet.Shared
{
	public class IntensityImage
	{
        private readonly float[,] _pixels;
        private readonly Effect _callingEffect;

		public int Width { get { return this._pixels.GetLength(0); } }
		public int Height { get { return this._pixels.GetLength(1); } }
		public bool IsCancelRequested { get { return this._callingEffect != null && this._callingEffect.IsCancelRequested; } }

		public IntensityImage(int width, int height, Effect callingEffect = null)
		{
			this._callingEffect = callingEffect;
			this._pixels = new float[width, height];
		}

		public float this[int x, int y]
		{
			get { return this._pixels[x, y]; }
			set { this._pixels[x, y] = value; }
		}

		public static async Task<IntensityImage> FromSurfaceAsync(Surface surface, Rectangle bounds, Effect callingEffect = null)
		{
			return await Task.Run<IntensityImage>(() => { return IntensityImage.FromSurface(surface, bounds, callingEffect); });
		}

		public static IntensityImage FromSurface(Surface surface, Rectangle bounds, Effect callingEffect = null)
		{
			var RetVal = new IntensityImage(bounds.Width, bounds.Height, callingEffect);
			Parallel.For(bounds.Left, bounds.Right, (x, loopState) =>
			  {
				  if (RetVal.IsCancelRequested)
				  {
					  loopState.Stop();
					  return;
				  }

				  for (int y = bounds.Top; y < bounds.Bottom; y++)
				  {
					  if (RetVal.IsCancelRequested)
						  return;
					  RetVal[x - bounds.Left, y - bounds.Top] = (float)surface[x, y].GetIntensity();
				  }
			  });
			return RetVal;
		}

		public static async Task<IntensityImage> FromSurfaceAsync(Surface surface, Effect callingEffect = null)
		{
			return await Task.Run<IntensityImage>(() => { return IntensityImage.FromSurface(surface, callingEffect); });
		}

		public static IntensityImage FromSurface(Surface surface, Effect callingEffect = null)
		{
			return IntensityImage.FromSurface(surface, surface.Bounds, callingEffect);
		}

		public Rectangle GetBounds() { return new Rectangle(0, 0, this.Width, this.Height); }

		public float Convolve(int x, int y, Matrix kernel)
		{
			float RetVal = 0;
			for (int kx = 0; kx < kernel.ColumnCount; kx++)
			{
				int sx = x + kx - kernel.ColumnCount / 2;
				if (sx < 0)
					sx = -sx;
				else if (sx >= this.Width)
					sx = 2 * (this.Width-1) - sx;
				
				for (int ky = 0; ky < kernel.RowCount; ky++)
				{
					int sy = y + ky - kernel.RowCount / 2;
					if (sy < 0)
						sy = -sy;
					else if (sy >= this.Height)
						sy = 2 * (this.Height-1) - sy;

					RetVal += this[sx, sy] * kernel[ky, kx];
				}
			}
			return RetVal;
		}

		public async Task<IntensityImage> ConvolveAsync(Rectangle bounds, Matrix kernel)
		{
			return await Task.Run<IntensityImage>(() => { return this.Convolve(bounds, kernel); });
		}

		public IntensityImage Convolve(Rectangle bounds, Matrix kernel)
		{
			var RetVal = new IntensityImage(bounds.Width, bounds.Height, this._callingEffect);
			Parallel.For(bounds.Left, bounds.Right, (x, loopState) =>
			  {
				  if (RetVal.IsCancelRequested)
				  {
					  loopState.Stop();
					  return;
				  }

				  for (int y = bounds.Top; y < bounds.Bottom; y++)
				  {
					  if (RetVal.IsCancelRequested)
						  return;
					  RetVal[x - bounds.Left, y - bounds.Top] = this.Convolve(x, y, kernel);
				  }
			  });
			return RetVal;
		}

		public async Task<IntensityImage> ConvolveAsync(Matrix kernel)
		{
			return await Task.Run<IntensityImage>(() => { return this.Convolve(kernel); });
		}

		public IntensityImage Convolve(Matrix kernel)
		{
			Rectangle bounds = this.GetBounds();
			return this.Convolve(bounds, kernel);
		}
	}
}
