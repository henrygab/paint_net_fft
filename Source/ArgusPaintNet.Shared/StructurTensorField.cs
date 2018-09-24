using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using PaintDotNet;
using PaintDotNet.Effects;

namespace ArgusPaintNet.Shared
{
    public struct TensorCharacteristics
	{
		public float MinEigenvalue { get; set; }
		public float MaxEigenvalue { get; set; }
		public float DominantDirection { get; set; }
	}

	public struct StructurTensor
	{
		public float Value11 { get; set; }
		public float Value12 { get; set; }
		public float Value22 { get; set; }
		public float Value21 { get { return this.Value12; } set { this.Value12 = value; } }

		public StructurTensor(float[] values)
			:this()
		{
			this.Value11 = values[0];
			this.Value12 = values[1];
			this.Value22 = values[2];
		}

		public float[] ToArray()
		{
			return new float[] { this.Value11, this.Value12, this.Value22 };
		}

		public Pair<float,float> GetEigenvalues()
		{
			double aPd = (double)this.Value11 + this.Value22;
			double aMd = (double)this.Value11 - this.Value22;
			double bc = (double)this.Value12 * this.Value21; // Always >= 0, since v12 == v21
			double sqrt = Math.Sqrt(aMd * aMd + 4 * bc);
			return new Pair<float, float>((float)(0.5 * (aPd - sqrt)), (float)(0.5 * (aPd + sqrt)));
		}

		public float GetMinEigenvalue() { return this.GetEigenvalues().First; }
		public float GetMaxEigenvalue() { return this.GetEigenvalues().Second; }

		public VectorFloat GetEigenvector(float eigenvalue)
		{
			double ev = (double)eigenvalue;
			double num = ev - this.Value12 - this.Value22;
			double den = ev - this.Value11 - this.Value21;
			float x, y;
			if (num < den)
			{
				y = 1;
				x = -(float)(num / den);
			}
			else
			{
				x = 1;
				y = -(float)(den / num);
			}
			return new VectorFloat(x, y);
		}

		public TensorCharacteristics GetCharacteristics()
		{
			Pair<float, float> evs = this.GetEigenvalues();
			var RetVal = new TensorCharacteristics();
			RetVal.MinEigenvalue = evs.First;
			RetVal.MaxEigenvalue = evs.Second;
			VectorFloat vec = this.GetEigenvector(RetVal.MaxEigenvalue);
			RetVal.DominantDirection = (float)Math.Atan2(vec.Y, vec.X);
			return RetVal;
		}
	}

	public class StructurTensorField
	{
        private readonly float[,,] _values;
        private readonly Effect _callingEffect;

		public int Width { get { return this._values.GetLength(0); } }
		public int Height { get { return this._values.GetLength(1); } }
		public bool IsCancelRequested { get { return this._callingEffect != null && this._callingEffect.IsCancelRequested; } }

		public StructurTensorField(int width, int height, Effect callingEffect = null)
		{
			this._callingEffect = callingEffect;
			this._values = new float[width, height, 3];
		}

		public float Get11(int x, int y) { return this._values[x, y, 0]; }
		public void Set11(int x, int y, float value) { this._values[x, y, 0] = value; }
		public float Get12(int x, int y) { return this._values[x, y, 1]; }
		public void Set12(int x, int y, float value) { this._values[x, y, 1] = value; }
		public float Get22(int x, int y) { return this._values[x, y, 2]; }
		public void Set22(int x, int y, float value) { this._values[x, y, 2] = value; }

		public StructurTensor this[int x, int y]
		{
			get
			{
				return new StructurTensor()
				{
					Value11 = this.Get11(x, y),
					Value12 = this.Get12(x, y),
					Value22 = this.Get22(x, y)
				};
			}
			set
			{
				this.Set11(x, y, value.Value11);
				this.Set12(x, y, value.Value12);
				this.Set22(x, y, value.Value22);
			}
		}

		public Rectangle GetBounds() { return new Rectangle(0, 0, this.Width, this.Height); }

		public static async Task<StructurTensorField> FromIntensityImageAsync(IntensityImage image, Rectangle bounds, Matrix diffX = null, Matrix diffY = null, Effect callingEffect = null)
		{
			return await Task.Run<StructurTensorField>(() => { return StructurTensorField.FromIntensityImage(image, bounds, diffX, diffY, callingEffect); });
		}

		public static StructurTensorField FromIntensityImage(IntensityImage image, Rectangle bounds, Matrix diffX = null, Matrix diffY = null, Effect callingEffect = null)
		{
			if (diffX == null)
				diffX = new float[,] { { -1, -8, 0, 8, 1 } };
			if (diffY == null)
				diffY = diffX.GetTransposed();
			var RetVal = new StructurTensorField(bounds.Width, bounds.Height, callingEffect);
			Parallel.For(bounds.Left, bounds.Right, (x, loopState) =>
			{
				if (RetVal.IsCancelRequested)
				{
					loopState.Stop();
					return;
				}

				int tx = x - bounds.Left;
				for (int y = bounds.Top; y < bounds.Bottom; y++)
				{
					if (RetVal.IsCancelRequested)
						return;
					float Ix = image.Convolve(x, y, diffX);
					float Iy = image.Convolve(x, y, diffY);
					int ty = y - bounds.Top;
					RetVal.Set11(tx, ty, Ix * Ix);
					RetVal.Set12(tx, ty, Ix * Iy);
					RetVal.Set22(tx, ty, Iy * Iy);
				}
			});
			return RetVal;
		}

		public static async Task<StructurTensorField> FromIntensityImageAsync(IntensityImage image, Matrix diffX = null, Matrix diffY = null, Effect callingEffect = null)
		{
			return await Task.Run<StructurTensorField>(() => { return StructurTensorField.FromIntensityImage(image, diffX, diffY, callingEffect); });
		}

		public static StructurTensorField FromIntensityImage(IntensityImage image, Matrix diffX = null, Matrix diffY = null, Effect callingEffect = null)
		{
			return StructurTensorField.FromIntensityImage(image, image.GetBounds(), diffX, diffY, callingEffect);
		}

		public static StructurTensorField FromSurface(Surface surface, Rectangle bounds, Matrix diffX = null, Matrix diffY = null, Effect callingEffect = null)
		{
			var image = IntensityImage.FromSurface(surface, bounds, callingEffect);
			return StructurTensorField.FromIntensityImage(image, image.GetBounds(), diffX, diffY, callingEffect);
		}

		public static StructurTensorField FromSurface(Surface surface, Matrix diffX = null, Matrix diffY = null, Effect callingEffect = null)
		{
			return StructurTensorField.FromSurface(surface, surface.Bounds, diffX, diffY, callingEffect);
		}

		public static async Task<StructurTensorField> FromSurfaceAsync(Surface surface, Rectangle bounds, Matrix diffX=null,Matrix diffY=null,Effect callingEffect = null)
		{
			IntensityImage image = await IntensityImage.FromSurfaceAsync(surface, bounds, callingEffect);
			return await StructurTensorField.FromIntensityImageAsync(image, image.GetBounds(), diffX, diffY, callingEffect);
		}

		public static async Task<StructurTensorField> FromSurfaceAsync(Surface surface, Matrix diffX = null, Matrix diffY = null, Effect callingEffect = null)
		{
			return await StructurTensorField.FromSurfaceAsync(surface, surface.Bounds, diffX, diffY, callingEffect);
		}

		public StructurTensor Convolve(int x, int y, Matrix kernel)
		{
			var RetVal = new StructurTensor();
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

					float factor = kernel[ky, kx];
					RetVal.Value11 += this.Get11(sx, sy) * factor;
					RetVal.Value12 += this.Get12(sx, sy) * factor;
					RetVal.Value22 += this.Get22(sx, sy) * factor;
				}
			}
			return RetVal;
		}

		public async Task<StructurTensorField> ConvolveAsync(Rectangle bounds, Matrix kernel)
		{
			return await Task.Run<StructurTensorField>(() => { return this.Convolve(bounds, kernel); });
		}

		public StructurTensorField Convolve(Rectangle bounds, Matrix kernel)
		{
			var RetVal = new StructurTensorField(bounds.Width, bounds.Height, this._callingEffect);
			Parallel.For(bounds.Left, bounds.Right, (x, loopState) =>
			{
				if (RetVal.IsCancelRequested)
				{
					loopState.Stop();
					return;
				}

				int tx = x - bounds.Left;
				for (int y = bounds.Top; y < bounds.Bottom; y++)
				{
					if (RetVal.IsCancelRequested)
						return;
					int ty = y - bounds.Top;
					RetVal[tx, ty] = this.Convolve(x, y, kernel);
				}
			});
			return RetVal;
		}

		public async Task<StructurTensorField> ConvolveAsync(Matrix kernel)
		{
			return await Task.Run<StructurTensorField>(() => { return this.Convolve(kernel); });
		}

		public StructurTensorField Convolve(Matrix kernel)
		{
			return this.Convolve(this.GetBounds(), kernel);
		}

		public async Task<Pair<float,float>> GetMeanAndStdDeviationOfMaxEVAsync(Rectangle[] rois = null, float threshold = 0.001f)
		{
			return await Task.Run<Pair<float, float>>(() => { return this.GetMeanAndStdDeviationOfMaxEV(rois, threshold); });
		}

		public Pair<float,float> GetMeanAndStdDeviationOfMaxEV(Rectangle[] rois = null, float threshold = 0.001f)
		{
			if (rois == null)
				rois = new Rectangle[] { this.GetBounds() };
			object _lock = new object();
			double val = 0;
			double valSq = 0;
			long n_tot = 0;
			double K = this[rois[0].Left, rois[0].Top].GetMaxEigenvalue();

			Parallel.For(0, rois.Length, (i, loopStateI) =>
			{
				if (this.IsCancelRequested)
				{
					loopStateI.Stop();
					return;
				}
				Rectangle rect = rois[i];
				Parallel.For(rect.Top, rect.Bottom, (y, loopStateY) =>
				{
					if (this.IsCancelRequested)
					{
						loopStateY.Stop();
						return;
					}
					double v = 0;
					double vSq = 0;
					long n = 0;
					for (int x = rect.Left; x < rect.Right; x++)
					{
						float maxEv = this[x, y].GetMaxEigenvalue();
						if (maxEv < threshold)
							continue;

						double diff = maxEv - K;
						v += diff;
						vSq += diff * diff;
						n++;
					}
					lock (_lock)
					{
						val += v;
						valSq += vSq;
						n_tot += n;
					}
				});
			});

			n_tot = Math.Max(1, n_tot);
			float mean = (float)(val / n_tot + K);
			float dev = (float)Math.Sqrt((valSq - (val * val / n_tot)) / n_tot);
			return new Pair<float, float>(mean, dev);
		}
	}
}
