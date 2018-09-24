using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaintDotNet;
using PaintDotNet.Rendering;
using PaintDotNet.Effects;
using System.Drawing;

namespace ArgusPaintNet.Shared
{
	public static class ExtensionMethods
	{
		public static Rectangle GetSelectionBounds(this EffectEnvironmentParameters para)
		{
			return para.GetSelection(para.SourceSurface.Bounds).GetBoundsInt();
		}

		public static Rectangle[] GetSelectionScanlines(this EffectEnvironmentParameters para)
		{
			return para.GetSelection(para.SourceSurface.Bounds).GetRegionScansInt();
		}

		public static Pair<ArgusColor, ArgusColor> GetMeanAndStdDeviation(this Surface surface, Rectangle[] rois = null, Effect callingEffect = null)
		{
			return surface.GetMeanAndStdDeviation(0, 0xFFFFFFFF, rois, callingEffect);
		}

		/// <summary>
		/// Implements: https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Computing_shifted_data
		/// </summary>
		/// <param name="surface"></param>
		/// <param name="rects"></param>
		/// <param name="stdDeviation"></param>
		/// <returns>The mean color.</returns>
		public static Pair<ArgusColor,ArgusColor> GetMeanAndStdDeviation(this Surface surface, ArgusColor minValue, ArgusColor maxValue, Rectangle[] rois = null, Effect callingEffect = null)
		{
			rois = rois ?? new Rectangle[] { surface.Bounds };
			object _lock = new object();
			double r_tot = 0;
			double g_tot = 0;
			double b_tot = 0;
			double a_tot = 0;
			double r2_tot = 0;
			double g2_tot = 0;
			double b2_tot = 0;
			double a2_tot = 0;
			long n_tot = 0;
			ColorBgra K = surface[rois[0].Left, rois[0].Top];

			Parallel.For(0, rois.Length, (i, loopStateI) =>
			{
				if (callingEffect?.IsCancelRequested ?? false)
				{
					loopStateI.Stop();
					return;
				}
				Rectangle rect = rois[i];
				Parallel.For(rect.Top, rect.Bottom, (y, loopStateY) =>
				{
					if (callingEffect?.IsCancelRequested ?? false)
					{
						loopStateY.Stop();
						return;
					}
					double r = 0;
					double g = 0;
					double b = 0;
					double a = 0;
					double r2 = 0;
					double g2 = 0;
					double b2 = 0;
					double a2 = 0;
					long n = 0;
					for (int x = rect.Left; x < rect.Right; x++)
					{
						ColorBgra c = surface[x, y];
						if (c.R < minValue.R || c.R > maxValue.R)
                        {
                            continue;
                        }

                        if (c.G < minValue.G || c.G > maxValue.G)
                        {
                            continue;
                        }

                        if (c.B < minValue.B || c.B > maxValue.B)
                        {
                            continue;
                        }

                        if (c.A < minValue.A || c.A > maxValue.A)
                        {
                            continue;
                        }

                        int v = c.R - K.R;
						r += v;
						r2 += v * v;

						v = c.G - K.G;
						g += v;
						g2 += v * v;

						v = c.B - K.B;
						b += v;
						b2 += v * v;

						v = c.A - K.A;
						a += v;
						a2 += v * v;

						n++;
					}
					lock (_lock)
					{
						r_tot += r;
						r2_tot += r2;
						g_tot += g;
						g2_tot += g2;
						b_tot += b;
						b2_tot += b2;
						a_tot += a;
						a2_tot += a2;
						n_tot += n;
					}
				});
			});

			n_tot = Math.Max(1, n_tot);
            var mean = new ArgusColor
            {
                R = (byte)Math.Round(r_tot / n_tot + K.R),
                G = (byte)Math.Round(g_tot / n_tot + K.G),
                B = (byte)Math.Round(b_tot / n_tot + K.B),
                A = (byte)Math.Round(a_tot / n_tot + K.A)
            };

            double devR = Math.Round(Math.Sqrt((r2_tot - (r_tot * r_tot / n_tot)) / n_tot));
			double devG = Math.Round(Math.Sqrt((g2_tot - (g_tot * g_tot / n_tot)) / n_tot));
			double devB = Math.Round(Math.Sqrt((b2_tot - (b_tot * b_tot / n_tot)) / n_tot));
			double devA = Math.Round(Math.Sqrt((a2_tot - (a_tot * a_tot / n_tot)) / n_tot));
			var dev = new ArgusColor((byte)devR, (byte)devG, (byte)devB, (byte)devA);
			return new Pair<ArgusColor, ArgusColor>(mean, dev);
		}

		public static async Task<Pair<ArgusColor,ArgusColor>> GetMeanAndStdDeviationAsync(this Surface surface, ArgusColor minValue, ArgusColor maxValue, Rectangle[] rois = null, Effect callingEffect = null)
		{
			return await Task.Run(() => { return surface.GetMeanAndStdDeviation(minValue, maxValue, rois, callingEffect); });
		}

		public static async Task<Pair<ArgusColor, ArgusColor>> GetMeanAndStdDeviationAsync(this Surface surface, Rectangle[] rois = null, Effect callingEffect = null)
		{
			return await Task.Run(() => { return surface.GetMeanAndStdDeviation(rois, callingEffect); });
		}

		public static ArgusColor GetDominantColor(this Surface surface, byte maxDeviation = 10, int maxIterations = 100, Rectangle[] rois = null, Effect callingEffect = null)
		{
			return surface.GetDominantColor(0, 0xFFFFFFFF, maxDeviation, maxIterations, rois, callingEffect);
		}

		public static ArgusColor GetDominantColor(this Surface surface, ArgusColor minValue, ArgusColor maxValue, byte maxDeviation = 10, int maxIterations = 100, Rectangle[] rois = null, Effect callingEffect = null)
		{
			Pair<ArgusColor, ArgusColor> meanAndDev = surface.GetMeanAndStdDeviation(minValue, maxValue, rois, callingEffect);
			ArgusColor dev = meanAndDev.Second;
			ArgusColor mean = meanAndDev.First;
			int count = 1;
			while (!(callingEffect?.IsCancelRequested ?? false) && count < maxIterations && (dev.R > maxDeviation || dev.G > maxDeviation || dev.B > maxDeviation || dev.A > maxDeviation))
			{
				minValue.R = (byte)Math.Max(0, mean.R - dev.R);
				minValue.G = (byte)Math.Max(0, mean.G - dev.G);
				minValue.B = (byte)Math.Max(0, mean.B - dev.B);
				minValue.A = (byte)Math.Max(0, mean.A - dev.A);

				maxValue.R = (byte)Math.Min(255, mean.R + dev.R);
				maxValue.G = (byte)Math.Min(255, mean.G + dev.G);
				maxValue.B = (byte)Math.Min(255, mean.B + dev.B);
				maxValue.A = (byte)Math.Min(255, mean.A + dev.A);

				meanAndDev = surface.GetMeanAndStdDeviation(minValue, maxValue, rois, callingEffect);
				mean = meanAndDev.First;
				dev = meanAndDev.Second;
				count++;
			}
			return mean;
		}

		public static async Task<ArgusColor> GetDominantColorAsync(this Surface surface, ArgusColor minValue, ArgusColor maxValue, byte maxDeviation = 10, int maxIterations = 100, Rectangle[] rois = null, Effect callingEffect = null)
		{
			return await Task.Run(() => { return surface.GetDominantColor(minValue, maxValue, maxDeviation, maxIterations, rois, callingEffect); });
		}

		public static async Task<ArgusColor> GetDominantColorAsync(this Surface surface, byte maxDeviation = 10, int maxIterations = 100, Rectangle[] rois = null, Effect callingEffect = null)
		{
			return await Task.Run(() => { return surface.GetDominantColor(maxDeviation, maxIterations, rois, callingEffect); });
		}
	}
}
