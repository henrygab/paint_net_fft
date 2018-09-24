using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ArgusPaintNet.FFT.FFTWInterop
{
    internal class ComplexPlan : IDisposable 
	{
        private readonly uint _width;
        private readonly uint _height;
        private IntPtr _in;
        private IntPtr _out;
        private IntPtr _plan;

		public int Width { get { return (int)this._width; } }
		public int Height { get { return (int)this._height; } }
		public double NormalizationConstant { get { return 1.0 / Math.Sqrt((double)this._width * this._height); } }

		internal ComplexPlan(int width, int height, IntPtr inComplex, IntPtr outComplex, IntPtr plan)
		{
			this._width = (uint)width;
			this._height = (uint)height;
			this._in = inComplex;
			this._out = outComplex;
			this._plan = plan;
		}

		public static ComplexPlan GetInstance(int width, int height, bool forward = true) { return FFTW.fftw_plan_dft_2d(width, height, forward); }

		//public unsafe bool SetInputForwards(double[,] input)
		//{
		//	if (input.GetLength(0) != this._width || input.GetLength(1) != this._height)
		//		return false;

		//	lock (this)
		//	{
		//		GCHandle handle = GCHandle.Alloc(input, GCHandleType.Pinned);
		//		IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(input, 0);
		//		FFTW.CopyMemory(this._in, ptr, this._width * this._height * (uint)Marshal.SizeOf(typeof(double)));
		//		handle.Free();
		//	}
		//	return true;
		//}

		public void Execute() { lock (this) { FFTW.fftw_execute(this._plan); } }

		public unsafe void SetInput(int x, int y, Complex value)
		{
			long index = x * this._height + y;
			Complex* ptr = (Complex*)this._in.ToPointer();
			ptr[index] = value;
		}

		public unsafe Complex GetOutput(int x, int y)
		{
			long index = x * this._height + y;
			Complex* ptr = (Complex*)this._out.ToPointer();
			return ptr[index];
		}

		public void Dispose()
		{
			FFTW.fftw_free(ref this._in);
			FFTW.fftw_free(ref this._out);
			FFTW.fftw_destroy_plan(ref this._plan);
		}
	}
}
