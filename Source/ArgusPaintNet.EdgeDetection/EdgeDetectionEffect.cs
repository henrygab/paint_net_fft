using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgusPaintNet.Shared;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using PaintDotNet.IndirectUI;
using PaintDotNet.Rendering;
using System.Drawing;
using System.Windows.Forms;

namespace ArgusPaintNet.EdgeDetection
{
	[PluginSupportInfo(typeof(PluginSupportInfo))]
	public class EdgeDetectionEffect : PropertyBasedEffect
	{
		public enum PropertyNames
		{
			Mode,
			LowerThreshold,
			UpperThreshold,
			Color,
			Color2,
			HueDirection,
			Angle,
			DifferenceFilter,
			SmoothingRadius
		}

		public enum Modes
		{
			Edge,
			Corner
		}

		public enum HueDirection
		{
			Shortest,
			Longest,
			Clockwise,
			CounterClockwise
		}

        private const float MaxThresholdValue = 1f;
        private Modes _mode = Modes.Edge;
        private float _lowerThres = 0.0f * MaxThresholdValue;
        private float _upperThres = 0.2f * MaxThresholdValue;
        private HsvColor _color = new HsvColor();
        private HsvColor _color2 = new HsvColor();
        private Matrix _diffX = new Matrix(new float[,] { { -1, -8, 0, 8, 1 } });
        private int _radius = 2;
        private Matrix _weight;
        private HueDirection _hueDirection;
        private double _angle = 0;
        private Task<StructurTensorField> _taskSTField = null;
        private Effect _cancelToken = null;
        private CachedValues<TensorCharacteristics> _cachedCharacteristics;
        private Form _form;

		public static string StaticName { get { return "Edge Detection (Argus)"; } }
		public static Image StaticIcon { get { return null; } }
		public static string StaticSubMenuName { get { return SubmenuNames.Stylize; } }

		public EdgeDetectionEffect()
			: base(StaticName, StaticIcon, StaticSubMenuName, EffectFlags.Configurable)
		{
		}

		protected override void OnDispose(bool disposing)
		{
			if (this._taskSTField != null)
			{
				this._taskSTField.Dispose();
				this._taskSTField = null;
			}
			base.OnDispose(disposing);
		}

		protected override PropertyCollection OnCreatePropertyCollection()
		{
			this.Initialize();
			return new PropertyCollection(new Property[]
			{
				StaticListChoiceProperty.CreateForEnum<Modes>(PropertyNames.Mode, this._mode, false),
				new DoubleProperty(PropertyNames.LowerThreshold, this._lowerThres,0,MaxThresholdValue),
				new DoubleProperty(PropertyNames.UpperThreshold, this._upperThres,0,MaxThresholdValue),
				new Int32Property(PropertyNames.Color, (int)(this._color.ToColor().ToArgb()&0xFFFFFF), 0, 16777215),
				new Int32Property(PropertyNames.Color2, (int)(this._color2.ToColor().ToArgb()&0xFFFFFF),0, 16777215),
				StaticListChoiceProperty.CreateForEnum<HueDirection>(PropertyNames.HueDirection, this._hueDirection, false),
				new DoubleProperty(PropertyNames.Angle, this._angle,0,180),
				new StringProperty(PropertyNames.DifferenceFilter, this._diffX.ToString()),
				new Int32Property(PropertyNames.SmoothingRadius, this._radius, 0, 100)
			}, new PropertyCollectionRule[]
			{
				new SoftMutuallyBoundMinMaxRule<double,DoubleProperty>(PropertyNames.LowerThreshold, PropertyNames.UpperThreshold)
			});
		}

        private void Initialize()
		{
			Surface surface = this.EnvironmentParameters.SourceSurface;
			Rectangle bounds = this.EnvironmentParameters.GetSelection(surface.Bounds).GetBoundsInt();
			if (this._cachedCharacteristics == null)
			{
				this._cachedCharacteristics = new CachedValues<TensorCharacteristics>(bounds.Height, this.GetCharacteristics);
			}

			lock (this)
			{
				if (this._taskSTField == null)
				{
					this._cancelToken = new EdgeDetectionEffect();
					this._taskSTField = StructurTensorField.FromSurfaceAsync(surface, bounds, this._diffX.GetNormalized(), null, this._cancelToken);
					this._cachedCharacteristics.Invalidate();
				}
			}
		}

		protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
		{
			ControlInfo cInfo = base.OnCreateConfigUI(props);
			cInfo.SetPropertyControlValue(PropertyNames.LowerThreshold, ControlInfoPropertyNames.DecimalPlaces, 3);
			cInfo.SetPropertyControlValue(PropertyNames.LowerThreshold, ControlInfoPropertyNames.UpDownIncrement, 0.001);
			cInfo.SetPropertyControlValue(PropertyNames.LowerThreshold, ControlInfoPropertyNames.SliderLargeChange, 0.01);
			cInfo.SetPropertyControlValue(PropertyNames.LowerThreshold, ControlInfoPropertyNames.SliderSmallChange, 0.001);
			cInfo.SetPropertyControlValue(PropertyNames.UpperThreshold, ControlInfoPropertyNames.DecimalPlaces, 3);
			cInfo.SetPropertyControlValue(PropertyNames.UpperThreshold, ControlInfoPropertyNames.UpDownIncrement, 0.001);
			cInfo.SetPropertyControlValue(PropertyNames.UpperThreshold, ControlInfoPropertyNames.SliderLargeChange, 0.01);
			cInfo.SetPropertyControlValue(PropertyNames.UpperThreshold, ControlInfoPropertyNames.SliderSmallChange, 0.001);
			cInfo.SetPropertyControlType(PropertyNames.Color, PropertyControlType.ColorWheel);
			cInfo.SetPropertyControlType(PropertyNames.Color2, PropertyControlType.ColorWheel);
			cInfo.SetPropertyControlType(PropertyNames.Angle, PropertyControlType.AngleChooser);
			cInfo.SetPropertyControlValue(PropertyNames.DifferenceFilter, ControlInfoPropertyNames.ShowResetButton, true);

			cInfo.SetPropertyControlValue(PropertyNames.LowerThreshold, ControlInfoPropertyNames.DisplayName, "Lower Threshold");
			cInfo.SetPropertyControlValue(PropertyNames.UpperThreshold, ControlInfoPropertyNames.DisplayName, "Upper Threshold");
			cInfo.SetPropertyControlValue(PropertyNames.HueDirection, ControlInfoPropertyNames.DisplayName, "Hue Direction");
			cInfo.SetPropertyControlValue(PropertyNames.DifferenceFilter, ControlInfoPropertyNames.DisplayName, "Difference Filter");
			cInfo.SetPropertyControlValue(PropertyNames.SmoothingRadius, ControlInfoPropertyNames.DisplayName, "Smoothing Radius");
			return cInfo;
		}

		protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
		{
			this.Initialize();

			if (this._form == null)
			{
				this._form = Form.ActiveForm;
				if (this._form != null)
				{
					if (this._form.Text != StaticName)
					{
						this._form = null;
					}
					else
					{
						this._form.FormClosed += (sender, e) =>
						{
							if (((Form)sender).DialogResult != DialogResult.OK)
								this._cancelToken.SignalCancelRequest();
						};
						var c = this._form.CancelButton as Control;
						if (c != null)
							c.Click += (sender, e) => { this._cancelToken.SignalCancelRequest(); };
					}
				}
			}

			this._mode = (Modes)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.Mode).Value;
			this._lowerThres = (float)newToken.GetProperty<DoubleProperty>(PropertyNames.LowerThreshold).Value;
			this._upperThres = (float)newToken.GetProperty<DoubleProperty>(PropertyNames.UpperThreshold).Value;
			this._color = HsvColor.FromColor(ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Color).Value).ToColor());
			this._color2 = HsvColor.FromColor(ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Color2).Value).ToColor());
			this._hueDirection = (HueDirection)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.HueDirection).Value;
			this._radius = newToken.GetProperty<Int32Property>(PropertyNames.SmoothingRadius).Value;
			this._angle = MathUtil.DegreesToRadians(newToken.GetProperty<DoubleProperty>(PropertyNames.Angle).Value);
			string diffStr = newToken.GetProperty<StringProperty>(PropertyNames.DifferenceFilter).Value;

			Matrix diff;
			if (Matrix.TryParse(diffStr, out diff))
			{
				if (diff.GetNormalized() != this._diffX.GetNormalized())
				{
					this._diffX = diff;
					this._cancelToken.SignalCancelRequest();
					this._taskSTField = null;
					this.Initialize();
				}
			}

			int length = 2 * this._radius + 1;
			if (length < 3)
				this._weight = null;
			else if (this._weight == null || length != this._weight.RowCount)
			{
				this._weight = Utils.GetGaussianKernelY(length) * Utils.GetGaussianKernelX(length);
				this._cachedCharacteristics.Invalidate();
			}

			base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
		}

        private void GetCharacteristics(Rectangle rect, TensorCharacteristics[,] values)
		{
			StructurTensorField stField = this._taskSTField.Result;
			Rectangle bounds = this.EnvironmentParameters.GetSelection(this.SrcArgs.Bounds).GetBoundsInt();
			for (int y = rect.Top; y < rect.Bottom; y++)
			{
				for (int x = rect.Left; x < rect.Right; x++)
				{
					StructurTensor tensor;
					if (this._weight != null)
						tensor = stField.Convolve(x - bounds.Left, y - bounds.Top, this._weight);
					else
						tensor = stField[x - bounds.Left, y - bounds.Top];
					values[x - rect.Left, y - rect.Top] = tensor.GetCharacteristics();
				}
			}
		}

        private Pair<int,int> GetHues(HueDirection hueDirection)
		{
			int val1 = this._color.Hue;
			int val2 = this._color2.Hue;
			if (hueDirection == HueDirection.Clockwise)
			{
				if (val1 > val2)
					val1 -= 360;
				return new Pair<int, int>(val1, val2);
			}
			if (hueDirection == HueDirection.CounterClockwise)
			{
				if (val2 > val1)
					val2 -= 360;
				return new Pair<int, int>(val1, val2);
			}

			Pair<int, int> clk = this.GetHues(HueDirection.Clockwise);
			int dClk = Math.Abs(clk.Second - clk.First);
			Pair<int, int> cclk = this.GetHues(HueDirection.CounterClockwise);
			int dCclk = Math.Abs(cclk.Second - cclk.First);
			if (hueDirection == HueDirection.Shortest)
			{
				if (dClk < dCclk)
					return clk;
				return cclk;
			}
			else //if (hueDirection == HueDirection.Longest)
			{
				if (dClk > dCclk)
					return clk;
				return cclk;
			}
		}

		protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
		{
			float factor = 255 / (this._upperThres - this._lowerThres);
			bool interpolate = this._color != this._color2;
			Pair<int, int> hues = this.GetHues(this._hueDirection);
			double fHue = (hues.Second - hues.First) / MathUtil.PIOver2;
			double fSat = (this._color2.Saturation - this._color.Saturation) / MathUtil.PIOver2;
			double fVal = (this._color2.Value - this._color.Value) / MathUtil.PIOver2;

			for (int i = startIndex; i < startIndex + length; i++)
			{
				Rectangle rect = renderRects[i];
				//Pair<float, float>[,] evs = this._cachedEVs.GetValues(rect);
				TensorCharacteristics[,] charact = this._cachedCharacteristics.GetValues(rect);

				for (int y = rect.Top; y < rect.Bottom; y++)
				{
					for (int x = rect.Left; x < rect.Right; x++)
					{
						float ev;
						TensorCharacteristics tChar = charact[x - rect.Left, y - rect.Top];
                        if (this._mode == Modes.Edge)
							ev = tChar.MaxEigenvalue;
						else
							ev = tChar.MinEigenvalue;

						ev -= this._lowerThres;
						ev *= factor;
						ev = Math.Max(0, Math.Min(255, ev));
						HsvColor color = this._color;
						if (ev > 0 && interpolate)
						{
							double ang = Utils.NormalizePeriodic(tChar.DominantDirection-this._angle, Math.PI);
							if (ang > MathUtil.PIOver2)
								ang = Math.PI - ang;

							color.Hue = (int)Utils.NormalizePeriodic(ang * fHue + hues.First, 360);
							color.Saturation = (int)Math.Max(0, Math.Min(100, ang * fSat + color.Saturation));
							color.Value = (int)Math.Max(0, Math.Min(100, ang * fVal + color.Value));
						}
						var c = ColorBgra.FromColor(color.ToColor());
						this.DstArgs.Surface[x, y] = c.NewAlpha((byte)ev);
					}
				}
			}
		}
	}
}
