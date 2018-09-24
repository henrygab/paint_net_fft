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
	[PluginSupportInfo(typeof(PluginSupportInfo))]
	public class HighPassFilterEffect : BandPassFilterEffect
	{
        public static new string StaticName => "High Pass Filter";
        public static new Image StaticImage => null;
        public static new string StaticSubMenuName => "Signal Processing";

        public HighPassFilterEffect()
			: base(StaticName, StaticImage, StaticSubMenuName)
		{ }

        protected override double InitialCutoffHighPassX => 0.01;
        protected override double InitialCutoffHighPassY => 0.01;
        protected override double InitialCutoffLowPassX => 1;
        protected override double InitialCutoffLowPassY => 1;
        protected override bool ShowSlidersHighPass => true;
        protected override bool ShowSlidersLowPass => false;
    }

	[PluginSupportInfo(typeof(PluginSupportInfo))]
	public class LowPassFilterEffect : BandPassFilterEffect
	{
        public static new string StaticName => "Low Pass Filter";
        public static new Image StaticImage => null;
        public static new string StaticSubMenuName => "Signal Processing";

        public LowPassFilterEffect()
			: base(StaticName, StaticImage, StaticSubMenuName)
		{ }

        protected override double InitialCutoffHighPassX => 0;
        protected override double InitialCutoffHighPassY => 0;
        protected override double InitialCutoffLowPassX => 0.5;
        protected override double InitialCutoffLowPassY => 0.5;
        protected override bool ShowSlidersHighPass => false;
        protected override bool ShowSlidersLowPass => true;
    }

	[PluginSupportInfo(typeof(PluginSupportInfo))]
	public class BandPassFilterEffect : FFTWUsingEffect
	{
        public static string StaticName => "Band Pass Filter";
        public static Image StaticImage => null;
        public static string StaticSubMenuName => "Signal Processing";

        public enum PropertyNames
		{
			ValueSource,
			CutoffFrequencyLowPassX,
			CutoffFrequencyLowPassY,
			CutoffFrequencyHighPassX,
			CutoffFrequencyHighPassY,
			LinkCutoffFrequencies,
		}

        private TwoWayPlan _fftPlan;
        private EnumDropDownValues<ValueSources> _enumDropDown;
        private ValueSources _valueSource;
        private double _lowPassX;
        private double _highPassX;
        private double _lowPassY;
        private double _highPassY;
        private bool _linkFrequencies = true;

		public BandPassFilterEffect()
			: this(StaticName, StaticImage, StaticSubMenuName)
		{ }

		protected BandPassFilterEffect(string name, Image image, string subMenuName)
			: base(name, null, subMenuName, EffectFlags.Configurable | EffectFlags.SingleRenderCall)
		{ }

        protected virtual double InitialCutoffLowPassX => 0.5;
        protected virtual double InitialCutoffHighPassX => 0.01;
        protected virtual double InitialCutoffLowPassY => 0.5;
        protected virtual double InitialCutoffHighPassY => 0.01;
        protected virtual bool ShowSlidersLowPass => true;
        protected virtual bool ShowSlidersHighPass => true;

        protected override PropertyCollection OnCreatePropertyCollectionCore()
		{
			RectInt32 bounds = this.EnvironmentParameters.GetSelection(this.EnvironmentParameters.SourceSurface.Bounds).GetBoundsRectInt32();
			double maxX = bounds.Width / 2.0;
			double maxY = bounds.Height / 2.0;
			this._lowPassX = Math.Round(this.InitialCutoffLowPassX * maxX);
			this._lowPassY = Math.Round(this.InitialCutoffLowPassY * maxY);
			this._highPassX = Math.Round(this.InitialCutoffHighPassX * maxX);
			this._highPassY = Math.Round(this.InitialCutoffHighPassY * maxY);

			this._enumDropDown = new EnumDropDownValues<ValueSources>();

			var props = new List<Property>();
			var rules = new List<PropertyCollectionRule>();
			props.Add(new StaticListChoiceProperty(PropertyNames.ValueSource, this._enumDropDown.Values));
			if (this.ShowSlidersLowPass)
			{
				props.Add(new DoubleProperty(PropertyNames.CutoffFrequencyLowPassX, this._lowPassX, 0, maxX));
				props.Add(new DoubleProperty(PropertyNames.CutoffFrequencyLowPassY, this._lowPassY, 0, maxY));
				rules.Add(new ReadOnlyBoundToBooleanRule(PropertyNames.CutoffFrequencyLowPassY, PropertyNames.LinkCutoffFrequencies, false));
			}
			if (this.ShowSlidersHighPass)
			{
				props.Add(new DoubleProperty(PropertyNames.CutoffFrequencyHighPassX, this._highPassX, 0, maxX));
				props.Add(new DoubleProperty(PropertyNames.CutoffFrequencyHighPassY, this._highPassY, 0, maxY));
				rules.Add(new ReadOnlyBoundToBooleanRule(PropertyNames.CutoffFrequencyHighPassY, PropertyNames.LinkCutoffFrequencies, false));
			}
			if (this.ShowSlidersLowPass || this.ShowSlidersHighPass)
			{
				props.Add(new BooleanProperty(PropertyNames.LinkCutoffFrequencies, this._linkFrequencies));
			}
			if (this.ShowSlidersLowPass && this.ShowSlidersHighPass)
			{
				rules.Add(new SoftMutuallyBoundMinMaxRule<double, DoubleProperty>(PropertyNames.CutoffFrequencyHighPassX, PropertyNames.CutoffFrequencyLowPassX));
				rules.Add(new SoftMutuallyBoundMinMaxRule<double, DoubleProperty>(PropertyNames.CutoffFrequencyHighPassY, PropertyNames.CutoffFrequencyLowPassY));
			}
			return new PropertyCollection(props, rules);
		}

		protected override void OnCreateConfigUICore(ControlInfo controlInfo)
		{
			controlInfo.SetPropertyControlValue(PropertyNames.ValueSource, ControlInfoPropertyNames.DisplayName, "Value Source");
			if (this.ShowSlidersLowPass)
			{
				controlInfo.SetPropertyControlValue(PropertyNames.CutoffFrequencyLowPassX, ControlInfoPropertyNames.DisplayName, "Low Pass Cutoff Frequency in X Direction");
				controlInfo.SetPropertyControlValue(PropertyNames.CutoffFrequencyLowPassY, ControlInfoPropertyNames.DisplayName, "Low Pass Cutoff Frequency in Y Direction");
			}
			if (this.ShowSlidersHighPass)
			{
				controlInfo.SetPropertyControlValue(PropertyNames.CutoffFrequencyHighPassX, ControlInfoPropertyNames.DisplayName, "High Pass Cutoff Frequency in X Direction");
				controlInfo.SetPropertyControlValue(PropertyNames.CutoffFrequencyHighPassY, ControlInfoPropertyNames.DisplayName, "High Pass Cutoff Frequency in Y Direction");
			}
			if (this.ShowSlidersLowPass || this.ShowSlidersHighPass)
			{
				controlInfo.SetPropertyControlValue(PropertyNames.LinkCutoffFrequencies, ControlInfoPropertyNames.DisplayName, string.Empty);
				controlInfo.SetPropertyControlValue(PropertyNames.LinkCutoffFrequencies, ControlInfoPropertyNames.Description, "Link Cutoff Frequencies");
			}
		}

		protected override void OnSetRenderInfoCore(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
		{
			RectInt32 bounds = this.EnvironmentParameters.GetSelection(this.EnvironmentParameters.SourceSurface.Bounds).GetBoundsRectInt32();
			if (this._enumDropDown == null)
            {
                this._enumDropDown = new EnumDropDownValues<ValueSources>();
            }

            this._valueSource = this._enumDropDown.GetEnumMember(newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.ValueSource).Value);

			this._linkFrequencies = newToken.GetProperty<BooleanProperty>(PropertyNames.LinkCutoffFrequencies).Value;
			if (this.ShowSlidersLowPass)
			{
				this._lowPassX = newToken.GetProperty<DoubleProperty>(PropertyNames.CutoffFrequencyLowPassX).Value;
				if (this._linkFrequencies)
                {
                    this._lowPassY = this._lowPassX / bounds.Width * bounds.Height;
                }
                else
                {
                    this._lowPassY = newToken.GetProperty<DoubleProperty>(PropertyNames.CutoffFrequencyLowPassY).Value;
                }
            }
			if (this.ShowSlidersHighPass)
			{
				this._highPassX = newToken.GetProperty<DoubleProperty>(PropertyNames.CutoffFrequencyHighPassX).Value;
				if (this._linkFrequencies)
                {
                    this._highPassY = this._highPassX / bounds.Width * bounds.Height;
                }
                else
                {
                    this._highPassY = newToken.GetProperty<DoubleProperty>(PropertyNames.CutoffFrequencyHighPassY).Value;
                }
            }
		}

		protected override void OnRenderCore(Rectangle[] renderRects, int startIndex, int length)
		{
			Surface src = this.SrcArgs.Surface;
			Surface dst = this.DstArgs.Surface;
			RectInt32 bounds = this.EnvironmentParameters.GetSelection(src.Bounds).GetBoundsRectInt32();
			var center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);

			lock (this)
			{
				if (this._fftPlan == null)
                {
                    this._fftPlan = TwoWayPlan.GetInstance(bounds.Width, bounds.Height);
                }
            }

			Func<ColorBgra, double> getValue = this._valueSource.GetGetValueFunc();

			if (this.IsCancelRequested)
            {
                return;
            }

            this._fftPlan.ClearData();

			Parallel.For(0, renderRects.Length, (i, loopStateI) =>
			{
				if (this.IsCancelRequested)
				{
					loopStateI.Stop();
					return;
				}
				Rectangle rect = renderRects[i];
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
						this._fftPlan.SetData(x - bounds.Left, y - bounds.Top, val);
					}
				});
			});

			if (this.IsCancelRequested)
            {
                return;
            }

            this._fftPlan.ExecuteForwards();

			Parallel.For(0, this._fftPlan.Height, (y, loopState) =>
			{
				if (this.IsCancelRequested)
				{
					loopState.Stop();
					return;
				}
				double fy = Math.Min(y, this._fftPlan.Height - y - 1);
				for (int x = 0; x < this._fftPlan.Width; x++)
				{
					double fx = Math.Min(x, this._fftPlan.Width - x - 1);
					if ((fx > this._lowPassX||fy > this._lowPassY) || (fx < this._highPassX && fy < this._highPassY))
					{
						this._fftPlan.SetTransformedData(x, y, Complex.Zero);
					}
				}
			});

			if (this.IsCancelRequested)
            {
                return;
            }

            this._fftPlan.ExecuteBackwards();

			if (this.IsCancelRequested)
            {
                return;
            }

            Func<double, ColorBgra> getColor = this._valueSource.GetGetColorFunc();
			double factor = this._fftPlan.NormalizationConstantTwoWays;

			Parallel.For(0, renderRects.Length, (i, loopStateI) =>
			{
				if (this.IsCancelRequested)
				{
					loopStateI.Stop();
					return;
				}
				Rectangle rect = renderRects[i];
				Parallel.For(rect.Top, rect.Bottom, (y, loopStateY) =>
				{
					if (this.IsCancelRequested)
					{
						loopStateY.Stop();
						return;
					}
					for (int x = rect.Left; x < rect.Right; x++)
					{
						Complex val = this._fftPlan.GetData(x - bounds.Left, y - bounds.Top);
						ColorBgra color = getColor(val.Magnitude * factor);
						dst[x, y] = color;
					}
				});
			});
		}
	}
}
