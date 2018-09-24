using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ArgusPaintNet.FFT.FFTWInterop
{
    internal class TwoWayPlan : IDisposable 
	{
        private readonly uint _width;
        private readonly uint _height;
        private IntPtr _in;
        private IntPtr _out;
        private IntPtr _planForwards;
        private IntPtr _planBackwards;

        public int Width => (int)this._width;
        public int Height => (int)this._height;
        public double NormalizationConstantOneWay { get; private set;}
		public double NormalizationConstantTwoWays { get; private set; }

		internal TwoWayPlan(int width, int height, IntPtr _in, IntPtr _out, IntPtr planForwards, IntPtr planBackwards)
		{
			this._width = (uint)width;
			this._height = (uint)height;
			this._in = _in;
			this._out = _out;
			this._planForwards = planForwards;
			this._planBackwards = planBackwards;
			this.NormalizationConstantTwoWays = 1.0/(this._width*this._height);
			this.NormalizationConstantOneWay = 1.0 /Math.Sqrt(this._width * this._height);
		}

		public static TwoWayPlan GetInstance(int width, int height) { return FFTW.GetTwoWayPlan(width,height); }

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

		public void ExecuteForwards() { lock (this) { FFTW.fftw_execute(this._planForwards); } }
		public void ExecuteBackwards() { lock (this) { FFTW.fftw_execute(this._planBackwards); } }

        private long GetIndex(int x, int y) { return x * this._height + y; }

		public unsafe void SetData(int x, int y, Complex value)
		{
			long index = this.GetIndex(x, y);
			var ptr = (Complex*)this._in.ToPointer();
			ptr[index] = value;
		}

		public unsafe Complex GetData(int x, int y)
		{
			long index = this.GetIndex(x, y);
			var ptr = (Complex*)this._in.ToPointer();
			return ptr[index];
		}

		public unsafe Complex GetTransformedData(int x, int y)
		{
			long index = this.GetIndex(x, y);
			var ptr = (Complex*)this._out.ToPointer();
			return ptr[index];
		}

		public unsafe void SetTransformedData(int x, int y, Complex value)
		{
			long index = this.GetIndex(x, y);
			var ptr = (Complex*)this._out.ToPointer();
			ptr[index] = value;
		}

		public unsafe void ClearData()
		{
			MemSet(this._in, 0, (int)(this._width * this._height * sizeof(Complex)));
		}

		public unsafe void ClearTransformedData()
		{
			MemSet(this._out, 0, (int)(this._width * this._height * sizeof(Complex)));
		}

		public void Dispose()
		{
			FFTW.fftw_free(ref this._in);
			FFTW.fftw_free(ref this._out);
			FFTW.fftw_destroy_plan(ref this._planForwards);
			FFTW.fftw_destroy_plan(ref this._planBackwards);
		}

		[DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr MemSet(IntPtr dest, int c, int count);
	}
}
