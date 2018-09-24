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
using ArgusPaintNet.Shared;

namespace ArgusPaintNet.ReverseBlend
{
	[PluginSupportInfo(typeof(PluginSupportInfo))]
	public class ReverseBlendEffectPlugin : PropertyBasedEffect
	{
        public static string StaticName => "Reverse Blend";
        public static Image StaticImage => null;
        public static string StaticSubMenuName => "Colors";

        #region UI

        public enum PropertyNames
		{
			BackgroundSource,
			ColorWheel,
			ToleranceRGB,
			ToleranceHue,
			ToleranceSat,
			ToleranceVal,
			DominantColorMaxDeviation
		}

		public enum BackgroundSources
		{
			PrimaryColor,
			SecondaryColor,
			ColorWheel,
			DominantColor,
			ClipboardAverage,
			ClipboardDominant
		}

        private int _tolRGB = 0;
        private int _tolHue = 0;
        private int _tolSat = 0;
        private int _tolVal = 0;
        private BackgroundSources _bgSource = BackgroundSources.DominantColor;
        private int _domColMaxDev = 10;
        private Task<ArgusColor> _taskBGColor;
        private Surface _clipboardSurface = null;

		#endregion

		public ReverseBlendEffectPlugin()
			: base(StaticName, StaticImage, StaticSubMenuName, EffectFlags.Configurable)
		{
		}

		protected override void OnDispose(bool disposing)
		{
			this._taskBGColor?.Dispose();
			base.OnDispose(disposing);
		}

		protected override PropertyCollection OnCreatePropertyCollection()
		{
			return new PropertyCollection(new Property[]
			{
				StaticListChoiceProperty.CreateForEnum<BackgroundSources>(PropertyNames.BackgroundSource, this._bgSource, false),
				new Int32Property(PropertyNames.ColorWheel, 0, 0 , 0xFFFFFF),
				new Int32Property(PropertyNames.ToleranceRGB, this._tolRGB, 0, 255),
				new Int32Property(PropertyNames.ToleranceHue, this._tolHue, 0 ,360),
				new Int32Property(PropertyNames.ToleranceSat, this._tolSat, 0 ,100),
				new Int32Property(PropertyNames.ToleranceVal, this._tolVal, 0 ,100),
				new Int32Property(PropertyNames.DominantColorMaxDeviation, this._domColMaxDev, 1,255)
			}, new PropertyCollectionRule[]
			{
				new ReadOnlyBoundToValueRule<object,StaticListChoiceProperty>(PropertyNames.DominantColorMaxDeviation, PropertyNames.BackgroundSource, new object[] { BackgroundSources.DominantColor, BackgroundSources.ClipboardDominant },true),
				new ReadOnlyBoundToValueRule<object,StaticListChoiceProperty>(PropertyNames.ColorWheel, PropertyNames.BackgroundSource, BackgroundSources.ColorWheel, true)
			});
		}

		protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
		{
			ControlInfo cInfo = base.OnCreateConfigUI(props);
			cInfo.SetPropertyControlType(PropertyNames.ColorWheel, PropertyControlType.ColorWheel);
			cInfo.SetPropertyControlValue(PropertyNames.BackgroundSource, ControlInfoPropertyNames.DisplayName, "Background Color Source");
			cInfo.SetPropertyControlValue(PropertyNames.ColorWheel, ControlInfoPropertyNames.DisplayName, string.Empty);
			cInfo.SetPropertyControlValue(PropertyNames.ToleranceRGB, ControlInfoPropertyNames.DisplayName, "Tolerance (RGB)");
			cInfo.SetPropertyControlValue(PropertyNames.ToleranceHue, ControlInfoPropertyNames.DisplayName, "Tolerance (HSV - Hue)");
			cInfo.SetPropertyControlValue(PropertyNames.ToleranceSat, ControlInfoPropertyNames.DisplayName, "Tolerance (HSV - Saturation)");
			cInfo.SetPropertyControlValue(PropertyNames.ToleranceVal, ControlInfoPropertyNames.DisplayName, "Tolerance (HSV - Value)");
			cInfo.SetPropertyControlValue(PropertyNames.DominantColorMaxDeviation, ControlInfoPropertyNames.DisplayName, "Dominant Color: Max. Deviation");
			return cInfo;
		}

		protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
		{
			var newBgSource = (BackgroundSources)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.BackgroundSource).Value;
			this._tolRGB = newToken.GetProperty<Int32Property>(PropertyNames.ToleranceRGB).Value;
			this._tolHue = newToken.GetProperty<Int32Property>(PropertyNames.ToleranceHue).Value;
			this._tolSat = newToken.GetProperty<Int32Property>(PropertyNames.ToleranceSat).Value;
			this._tolVal = newToken.GetProperty<Int32Property>(PropertyNames.ToleranceVal).Value;
			this._domColMaxDev = newToken.GetProperty<Int32Property>(PropertyNames.DominantColorMaxDeviation).Value;

			if (newBgSource != this._bgSource && (newBgSource == BackgroundSources.ClipboardAverage || newBgSource == BackgroundSources.ClipboardDominant))
			{
				this._clipboardSurface = Utils.GetSurfaceFromClipboard();
			}

			this._bgSource = newBgSource;

			if (this._bgSource == BackgroundSources.DominantColor)
			{
				this._taskBGColor = srcArgs.Surface.GetDominantColorAsync((byte)this._domColMaxDev, 100, this.EnvironmentParameters.GetSelectionScanlines(), this);
			}
			else if (this._bgSource == BackgroundSources.ClipboardDominant)
			{
				this._taskBGColor = this._clipboardSurface?.GetDominantColorAsync((byte)this._domColMaxDev, 100, null, this) ?? Task.FromResult<ArgusColor>(this.EnvironmentParameters.PrimaryColor);
			}
			else if (this._bgSource == BackgroundSources.ClipboardAverage)
			{
				this._taskBGColor = Task.Run(() => { return this._clipboardSurface?.GetMeanAndStdDeviation(null, this).First ?? this.EnvironmentParameters.PrimaryColor; });
			}
			else if (this._bgSource == BackgroundSources.ColorWheel)
			{
				this._taskBGColor = Task.FromResult<ArgusColor>(ArgusColor.FromRGB(newToken.GetProperty<Int32Property>(PropertyNames.ColorWheel).Value));
			}
			else if (this._bgSource == BackgroundSources.SecondaryColor)
			{
				this._taskBGColor = Task.FromResult<ArgusColor>(this.EnvironmentParameters.SecondaryColor);
			}
			else
			{
				this._taskBGColor = Task.FromResult<ArgusColor>(this.EnvironmentParameters.PrimaryColor);
			}

			base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
		}

		protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
		{
			if (renderRects.Length < 1)
				return;
			Surface src = this.SrcArgs.Surface;
			Surface dst = this.DstArgs.Surface;

			foreach (Rectangle rect in renderRects)
			{
				if (this.IsCancelRequested)
					return;
				this.Render(src, dst, rect, this._taskBGColor.Result);
			}
		}

        private void Render(Surface src, Surface dst, Rectangle rect, ArgusColor bgColor)
		{
			HsvColorF hsvBg = bgColor;
			for (int y = rect.Top; y < rect.Bottom; y++)
			{
				for (int x = rect.Left; x < rect.Right; x++)
				{
					ArgusColor px = src[x, y];
					if (Math.Abs(px.R - bgColor.R) < this._tolRGB && Math.Abs(px.G - bgColor.G) < this._tolRGB && Math.Abs(px.B - bgColor.B) < this._tolRGB)
						dst[x, y] = ColorBgra.FromUInt32(0);
					else
					{
						HsvColorF hsv = px;
						double dHue = Math.Abs(hsv.Hue - hsvBg.Hue);
						if (dHue > 180)
							dHue = 360 - dHue;
						if (dHue < this._tolHue && Math.Abs(hsv.Saturation - hsvBg.Saturation) < this._tolSat && Math.Abs(hsv.Value - hsvBg.Value) < this._tolVal)
							dst[x, y] = ColorBgra.FromUInt32(0);
						else
							dst[x, y] = px.ReverseBlend(bgColor);
					}
				}
			}
		}
	}
}