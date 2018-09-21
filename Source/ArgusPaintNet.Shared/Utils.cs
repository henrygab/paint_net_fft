using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaintDotNet;
using System.Drawing;
using PaintDotNet.Effects;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace ArgusPaintNet.Shared
{
	static class CommonExtensions
	{
		public static Rectangle GetBounds<T>(this T[,] array)
		{
			return new Rectangle(0, 0, array.GetLength(0), array.GetLength(1));
		}
	}

	public static class Utils
	{
		public static Surface GetSurfaceFromClipboard()
		{
			Image image = Utils.GetImageFromClipboard();
			if (image == null)
				return null;
			if (image is Bitmap)
				return Surface.CopyFromBitmap((Bitmap)image);
			return Surface.CopyFromGdipImage(image);
		}

		public static Image GetImageFromClipboard()
		{
			if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
				return Utils.GetImageFromClipboardCore();

			Image image = null;
			Thread thread = new Thread(() => { image = Utils.GetImageFromClipboardCore(); });
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
			return image;
		}

		static Image GetImageFromClipboardCore()
		{
			using (MemoryStream ms = Clipboard.GetData("PNG") as MemoryStream)
			{
				if (ms != null)
				{
					try { return Image.FromStream(ms); }
					catch { }
				}
			}

			StringCollection paths = Clipboard.GetFileDropList();
			if (paths != null && paths.Count > 0)
			{
				foreach (string file in paths)
				{
					try { return Image.FromFile(file); }
					catch { }
				}
			}

			byte[] data;
			using (MemoryStream ms = Clipboard.GetData(DataFormats.Dib) as MemoryStream)
			{
				if (ms == null)
				{
					return Clipboard.GetImage();
				}
				data = ms.ToArray();
			}

			int width = BitConverter.ToInt32(data, 4);
			int stride = width * 4;
			int height = BitConverter.ToInt32(data, 8);
			int bpp = BitConverter.ToInt16(data, 14);
			if (bpp != 32)
				return Clipboard.GetImage();

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			try
			{
				IntPtr ptr = new IntPtr((long)handle.AddrOfPinnedObject() + 40);
				return new Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, ptr);
			}
			catch
			{
				return Clipboard.GetImage();
			}
			finally
			{
				handle.Free();
			}
		}
	
		public static float[] GetGaussian(int length)
		{
			if ((length & 0x1) != 1 || length < 1)
				throw new ArgumentException("length must be positive and odd");
			double sigma2 = length * length / 36.0;
			double a = 1 / Math.Sqrt(2 * Math.PI * sigma2);
			double b = -1 / (2 * sigma2);
			float[] RetVal = new float[length];
			for (int i = 0; i < length; i++)
			{
				int x = i - length / 2;
				RetVal[i] = (float)(a * Math.Exp(x * x * b));
			}
			return RetVal;
		}

		public static Matrix GetGaussianKernelX(int length)
		{
			float[] gaussian = Utils.GetGaussian(length);
			return new Matrix(1, length, gaussian);
        }

		public static Matrix GetGaussianKernelY(int length)
		{
			float[] gaussian = Utils.GetGaussian(length);
			return new Matrix(length, 1, gaussian);
		}

		public static double NormalizePeriodic(double val, double T)
		{
			while (val < 0)
				val += T;
			while (val > T)
				val -= T;
			return val;
		}
	}
}
