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
using R2ComplexPlan = ArgusPaintNet.FFT.FFTWInterop.ComplexPlan;

namespace ArgusPaintNet.FFT
{
	[PluginSupportInfo(typeof(PluginSupportInfo))]
	public class FFTEffectPlugin : FFTWUsingEffect
	{
        private R2ComplexPlan _fftPlanForwards;
        private ComplexPlan _fftPlanBackwards;
        private bool _isForwards = true;
        private ValueSources _valueSource = ValueSources.Intensity;
        private EnumDropDownValues<ValueSources> _enumDropDown;

		public enum PropertyNames
		{
			ValueSource,
			FFTDirectionIsForward,
		}

		public FFTEffectPlugin()
			: base("Fast Fourier Transform", null, "Transform", EffectFlags.Configurable | EffectFlags.SingleRenderCall) { }

		protected override void OnDispose(bool disposing)
		{
			if (this._fftPlanForwards != null)
            {
                this._fftPlanForwards.Dispose();
            }

            if (this._fftPlanBackwards != null)
            {
                this._fftPlanBackwards.Dispose();
            }

            base.OnDispose(disposing);
		}

		protected override PropertyCollection OnCreatePropertyCollectionCore()
		{
			this._enumDropDown = new EnumDropDownValues<ValueSources>();
			return new PropertyCollection(new Property[] {
					new StaticListChoiceProperty(PropertyNames.ValueSource, this._enumDropDown.Values),
					new BooleanProperty(PropertyNames.FFTDirectionIsForward,!this._isForwards)});
		}

		protected override void OnCreateConfigUICore(ControlInfo controlInfo)
		{
			controlInfo.SetPropertyControlValue(PropertyNames.ValueSource, ControlInfoPropertyNames.DisplayName, "Value Source");
			controlInfo.SetPropertyControlValue(PropertyNames.FFTDirectionIsForward, ControlInfoPropertyNames.DisplayName, string.Empty);
			controlInfo.SetPropertyControlValue(PropertyNames.FFTDirectionIsForward, ControlInfoPropertyNames.Description, "Inverse FFT");
		}

		protected override void OnSetRenderInfoCore(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
		{
			if (this._enumDropDown == null)
            {
                this._enumDropDown = new EnumDropDownValues<ValueSources>();
            }

            this._isForwards = !newToken.GetProperty<BooleanProperty>(PropertyNames.FFTDirectionIsForward).Value;
			this._valueSource = this._enumDropDown.GetEnumMember(newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.ValueSource).Value);
		}

		protected override void OnRenderCore(Rectangle[] renderRects, int startIndex, int length)
		{
			Surface src = this.SrcArgs.Surface;
			Surface dst = this.DstArgs.Surface;
			RectInt32 bounds = this.EnvironmentParameters.GetSelection(src.Bounds).GetBoundsRectInt32();
			var center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);

			bool isForwards = this._isForwards;
			this.InitializePlans(isForwards, bounds);


			if (this.IsCancelRequested)
            {
                return;
            }

            this.SetInput(src, renderRects, bounds, isForwards);

			if (this.IsCancelRequested)
            {
                return;
            }

            this.ExecutePlan(isForwards);

			if (this.IsCancelRequested)
            {
                return;
            }

            this.RenderOutput(dst, renderRects, bounds, isForwards);
		}

        #region Helper Methods

        private void InitializePlans(bool isForwards, RectInt32 bounds)
		{
			if (isForwards)
			{
				lock (this)
				{
					if (this._fftPlanForwards == null)
                    {
                        this._fftPlanForwards = R2ComplexPlan.GetInstance(bounds.Width, bounds.Height);
                    }
                }
			}
			else
			{
				lock (this)
				{
					if (this._fftPlanBackwards == null)
                    {
                        this._fftPlanBackwards = ComplexPlan.GetInstance(bounds.Width, bounds.Height, true);
                    }
                }
			}
		}

        private void ExecutePlan(bool isForwards)
		{
			if (isForwards)
            {
                this._fftPlanForwards.Execute();
            }
            else
            {
                this._fftPlanBackwards.Execute();
            }
        }

        private void SetInput(Surface src, Rectangle[] rects, RectInt32 bounds, bool isForwards)
		{
			if (isForwards)
            {
                this.SetInputForwards(src, rects, bounds);
            }
            else
            {
                this.SetInputBackwards(src, rects, bounds);
            }
        }

        private void RenderOutput(Surface dst, Rectangle[] rects, RectInt32 bounds, bool isForwards)
		{
			if (isForwards)
            {
                this.RenderOutputForwards(dst, rects, bounds);
            }
            else
            {
                this.RenderOutputBackwards(dst, rects, bounds);
            }
        }

        private void SetInputForwards(Surface src, Rectangle[] rects, RectInt32 bounds)
		{
			if (this.IsCancelRequested)
            {
                return;
            }

            Func<ColorBgra, double> getValue = this._valueSource.GetGetValueFunc();

			Parallel.For(0, rects.Length, (i, loopStateI) =>
			{
				if (this.IsCancelRequested)
				{
					loopStateI.Stop();
					return;
				}
				Rectangle rect = rects[i];
				Parallel.For(rect.Top, rect.Bottom, (y, loopStateY) =>
				{
					if (this.IsCancelRequested)
					{
						loopStateY.Stop();
						return;
					}
					for (int x = rect.Left; x < rect.Right; x++)
					{
						ColorBgra color = src[x, y];
						double val = getValue(color);
						this._fftPlanForwards.SetInput(x - bounds.Left, y - bounds.Top, val);
					}
				});
			});
		}

        private void RenderOutputForwards(Surface dst, Rectangle[] rects, RectInt32 bounds)
		{
			var center = new PointInt32(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
			double maxValue = 0;
			object _lock = new object();

			if (this.IsCancelRequested)
            {
                return;
            }

            Parallel.For(0, rects.Length, (i, loopStateI) =>
			{
				if (this.IsCancelRequested)
				{
					loopStateI.Stop();
					return;
				}
				Rectangle rect = rects[i];
				Parallel.For(rect.Top, rect.Bottom, (y, loopStateY) =>
				{
					if (this.IsCancelRequested)
					{
						loopStateY.Stop();
						return;
					}
					double mVal = 0;
					for (int x = rect.Left; x < rect.Right; x++)
					{
						Complex val = this._fftPlanForwards.GetOutput(x - bounds.Left, y - bounds.Top);
						mVal = Math.Max(mVal, val.Magnitude);
					}
					lock (_lock) { maxValue = Math.Max(maxValue, mVal); }
				});
			});

			double factor = GetFactor(maxValue);

			if (this.IsCancelRequested)
            {
                return;
            }

            Parallel.For(0, rects.Length, (i, loopStateI) =>
			{
				if (this.IsCancelRequested)
				{
					loopStateI.Stop();
					return;
				}
				Rectangle rect = rects[i];
				Parallel.For(rect.Top, rect.Bottom, (y, loopStateY) =>
				{
					if (this.IsCancelRequested)
					{
						loopStateY.Stop();
						return;
					}
					for (int x = rect.Left; x < rect.Right; x++)
					{
						int fftx = x - center.X;
						if (fftx < 0)
                        {
                            fftx += bounds.Width;
                        }

                        int ffty = y - center.Y;
						if (ffty < 0)
                        {
                            ffty += bounds.Height;
                        }

                        Complex val = this._fftPlanForwards.GetOutput(fftx, ffty);
						dst[x, y] = ComplexToColor(val, factor);
					}
				});
			});
		}

        private void SetInputBackwards(Surface src, Rectangle[] rects, RectInt32 bounds)
		{
			if (this.IsCancelRequested)
            {
                return;
            }

            var center = new PointInt32(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);

			Parallel.For(0, rects.Length, (i, loopStateI) =>
			{
				if (this.IsCancelRequested)
				{
					loopStateI.Stop();
					return;
				}
				Rectangle rect = rects[i];
				Parallel.For(rect.Top, rect.Bottom, (y, loopStateY) =>
				{
					if (this.IsCancelRequested)
					{
						loopStateY.Stop();
						return;
					}
					for (int x = rect.Left; x < rect.Right; x++)
					{
						ColorBgra color = src[x, y];
						Complex val = ColorToComplex(color);
						int fftx = x - center.X;
						if (fftx < 0)
                        {
                            fftx += bounds.Width;
                        }

                        int ffty = y - center.Y;
						if (ffty < 0)
                        {
                            ffty += bounds.Height;
                        }

                        this._fftPlanBackwards.SetInput(fftx, ffty, val);
					}
				});
			});
		}

        private void RenderOutputBackwards(Surface dst, Rectangle[] rects, RectInt32 bounds)
		{
			var center = new PointInt32(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);

			double maxValue = 0;
			object _lock = new object();

			if (this.IsCancelRequested)
            {
                return;
            }

            Parallel.For(0, rects.Length, (i, loopStateI) =>
			{
				if (this.IsCancelRequested)
				{
					loopStateI.Stop();
					return;
				}
				Rectangle rect = rects[i];
				Parallel.For(rect.Top, rect.Bottom, (y, loopStateY) =>
				{
					if (this.IsCancelRequested)
					{
						loopStateY.Stop();
						return;
					}
					double mVal = 0;
					for (int x = rect.Left; x < rect.Right; x++)
					{
						Complex val = this._fftPlanBackwards.GetOutput(x - bounds.Left, y - bounds.Top);
						mVal = Math.Max(mVal, val.Magnitude);
					}
					lock (_lock) { maxValue = Math.Max(maxValue, mVal); }
				});
			});

			double factor = 1.0 / maxValue;
			Func<double, ColorBgra> getColor = this._valueSource.GetGetColorFunc();

			if (this.IsCancelRequested)
            {
                return;
            }

            Parallel.For(0, rects.Length, (i, loopStateI) =>
			{
				if (this.IsCancelRequested)
				{
					loopStateI.Stop();
					return;
				}
				Rectangle rect = rects[i];
				Parallel.For(rect.Top, rect.Bottom, (y, loopStateY) =>
				{
					if (this.IsCancelRequested)
					{
						loopStateY.Stop();
						return;
					}
					for (int x = rect.Left; x < rect.Right; x++)
					{
						Complex val = this._fftPlanBackwards.GetOutput(x - bounds.Left, y - bounds.Top);
						ColorBgra color = getColor(val.Magnitude * factor);
						dst[x, y] = ColorBgra.FromColor(color);
					}
				});
			});
		}

        private static double GetFactor(double maxValue)
		{
			return ComplexToColor_MaxMag / maxValue;
		}

        private static ColorBgra ComplexToColor(Complex val, double factor)
		{
			//uint rg = EncodeValue2((uint)Math.Round(val.Magnitude * factor));
			uint rg = (uint)Math.Round(val.Magnitude * factor);
			uint b = (uint)Math.Round((val.Phase + Math.PI) * 255 / (2 * Math.PI));
			uint argb = (rg << 8) | b | 0xFF000000;
			return new ColorBgra() { Bgra = argb };
		}

        private const uint ComplexToColor_MaxMag = 0xFFFF;
        private const double ColorToComplex_NormFactor = 1.0 / ComplexToColor_MaxMag;

        private static Complex ColorToComplex(ColorBgra val)
		{
			uint p = val.Bgra & 0x000000FF;
			//uint m = RetrieveValue2((val.Bgra & 0x00FFFF00) >> 8);
			uint m = (val.Bgra & 0x00FFFF00) >> 8;
			if (val.A < 255)
            {
                m = 0;
            }

            double phase = -(p * 2 * Math.PI / 255.0 - Math.PI);
			double mag = m * ColorToComplex_NormFactor;
			return Complex.FromPolarCoordinates(mag, phase);
		}

        private static uint EncodeValue3(uint value)
		{
			uint rgb = 0;
			for (int i = 0; i < 8; i++)
			{
				uint mask = 1u << (i * 3);
				int shift = i * 2;
				// blue byte
				rgb |= (value & (mask << 0)) >> (shift);
				// green byte
				rgb |= (value & (mask << 2)) >> (shift + 2) << 8;
				// red byte
				rgb |= (value & (mask << 1)) >> (shift + 1) << 16;
			}
			return rgb;
		}

        private static uint EncodeValue2(uint value)
		{
			uint rg = 0;
			for (int i = 0; i < 8; i++)
			{
				uint mask = 1u << (i * 2);
				int shift = i;
				// green byte
				rg |= (value & (mask << 1)) >> (shift + 1);
				// red byte
				rg |= (value & (mask)) >> (shift) << 8;
			}
			return rg;
		}

        private static uint RetrieveValue3(uint rgb)
		{
			uint value = 0;
			for (int i = 0; i < 8; i++)
			{
				int shift = i * 2;
				value |= (rgb & (1u << i)) << (shift);
				value |= (rgb & (1u << (i + 8))) >> 8 << (shift + 2);
				value |= (rgb & (1u << (i + 16))) >> 16 << (shift + 1);
			}
			return value;
		}

        private static uint RetrieveValue2(uint rg)
		{
			uint value = 0;
			for (int i = 0; i < 8; i++)
			{
				int shift = i;
				value |= (rg & (1u << i)) << (shift + 1);
				value |= (rg & (1u << (i + 8))) >> 8 << (shift);
			}
			return value;
		}

		#endregion
	}
}
