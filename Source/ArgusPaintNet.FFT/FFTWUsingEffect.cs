using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System.Drawing;
using ArgusPaintNet.FFT.FFTWInterop;

namespace ArgusPaintNet.FFT
{
    public abstract class FFTWUsingEffect : PropertyBasedEffect
    {
        public FFTWUsingEffect(string name, Image image, string subMenuName, EffectFlags flags)
            : base(name, image, subMenuName, flags)
        { }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            if (FFTW.IsAvailable)
            {
                return this.OnCreatePropertyCollectionCore();
            }
            else
            {
                return new PropertyCollection(new Property[]{
                    new StringProperty("fftw_not_avialable", string.Format(Properties.Resources.FFTWNotAvialableMessage, FFTW.DllName),0,true)
                });
            }
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo cInfo = base.OnCreateConfigUI(props);
            if (FFTW.IsAvailable)
            {
                this.OnCreateConfigUICore(cInfo);
            }
            else
            {
                cInfo.SetPropertyControlValue("fftw_not_avialable", ControlInfoPropertyNames.Multiline, true);
                cInfo.SetPropertyControlValue("fftw_not_avialable", ControlInfoPropertyNames.DisplayName, string.Empty);
            }
            return cInfo;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            if (FFTW.IsAvailable)
            {
                this.OnSetRenderInfoCore(newToken, dstArgs, srcArgs);
            }

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            if (length < 1 || this.IsCancelRequested | !FFTW.IsAvailable)
            {
                return;
            }

            this.OnRenderCore(renderRects, startIndex, length);
        }

        protected abstract PropertyCollection OnCreatePropertyCollectionCore();
        protected abstract void OnCreateConfigUICore(ControlInfo controlInfo);
        protected abstract void OnSetRenderInfoCore(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs);
        protected abstract void OnRenderCore(Rectangle[] renderRects, int startIndex, int length);
    }
}
