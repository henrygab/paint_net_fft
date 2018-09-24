#if DEBUG
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System.IO;

namespace ArgusPaintNet.Shared
{
	public class TestEffect : Effect
	{
        private readonly IPdnDataObject _clipboard;
        private Surface _bitmap;
		public TestEffect()
			: base("Test", null, "ArgusTest")
		{
			this._clipboard = PdnClipboard.GetDataObject();
		}

		protected override void OnSetRenderInfo(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs)
		{
			if (this._clipboard != null)
			{
				using (Image image = Utils.GetImageFromClipboard())
				{
					if (image != null)
					{
						if (image is Bitmap)
                        {
                            this._bitmap = Surface.CopyFromBitmap((Bitmap)image);
                        }
                        else
                        {
                            this._bitmap = Surface.CopyFromGdipImage(image);
                        }
                    }
				}
			}
		}

		public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
		{
			if (this._bitmap == null)
            {
                return;
            }

            Rectangle bounds = srcArgs.Bounds;
			foreach (Rectangle rect in rois)
			{
				for (int y = rect.Top; y < rect.Bottom; y++)
				{
					int by = y - bounds.Top;
					if (by < 0 || by >= this._bitmap.Height)
                    {
                        continue;
                    }

                    for (int x = rect.Left; x < rect.Right; x++)
					{
						int bx = x - bounds.Left;
						if (bx < 0 || bx >= this._bitmap.Width)
                        {
                            continue;
                        }

                        dstArgs.Surface[x, y] = this._bitmap[x, y];
					}
				}
			}
		}
	}
}
#endif