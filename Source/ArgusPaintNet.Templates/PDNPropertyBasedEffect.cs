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

namespace ArgusPaintNet.Templates
{
    [PluginSupportInfo(typeof(PDNPluginSupportInfo))]
    public class PDNPropertyBasedEffect : PropertyBasedEffect
    {
        #region Static

        public static string StaticName => throw new NotImplementedException();
        public static Image StaticImage => null;
        public static string StaticSubMenuName => throw new NotImplementedException();

        #endregion

        public PDNPropertyBasedEffect()
            : base(StaticName, StaticImage, StaticSubMenuName, EffectFlags.Configurable)
        { }

        #region Effect Properties

        public enum PropertyNames
        {
            Value1
        }

        private bool _value1 = false;

        #endregion

        #region Overridden Methods

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return new PropertyCollection(new Property[] {
                new BooleanProperty(PropertyNames.Value1, this._value1)
            },
            new PropertyCollectionRule[] {

            });
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo cInfo = base.OnCreateConfigUI(props);

            cInfo.SetPropertyControlValue(PropertyNames.Value1, ControlInfoPropertyNames.DisplayName, string.Empty);
            cInfo.SetPropertyControlValue(PropertyNames.Value1, ControlInfoPropertyNames.Description, "Copy Background to Clipboard");
            return cInfo;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this._value1 = newToken.GetProperty<BooleanProperty>(PropertyNames.Value1).Value;
            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            if (length < 1)
            {
                return;
            }

            Surface srcSurface = this.SrcArgs.Surface;
            Surface dstSurface = this.DstArgs.Surface;

            foreach (Rectangle rect in renderRects)
            {
                this.Render(rect, srcSurface, dstSurface);
            }
        }

        private void Render(Rectangle rect, Surface srcSurface, Surface dstSurface)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
