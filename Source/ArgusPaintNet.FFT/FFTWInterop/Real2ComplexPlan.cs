using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ArgusPaintNet.FFT.FFTWInterop
{
    internal class Real2ComplexPlan : IDisposable
	{
        private uint _width;
        private uint _height;
        private uint _mHeight;
        private IntPtr _in;
        private IntPtr _out;
        private IntPtr _plan;

		public int Width { get { return (int)this._width; } }
		public int Height { get { return (int)this._height; } }
		public double NormalizationConstant { get { return 1.0 / Math.Sqrt((double)this._width * this._height); } }

		internal Real2ComplexPlan(int width, int height, IntPtr inReal, IntPtr outComplex, IntPtr plan)
		{
			this._width = (uint)width;
			this._height = (uint)height;
			this._mHeight = this._height / 2 + 1;
			this._in = inReal;
			this._out = outComplex;
			this._plan = plan;
		}

		public static Real2ComplexPlan GetInstance(int width, int height) { return FFTW.fftw_plan_dft_r2c_2d(width, height); }

		public void Execute() { lock (this) { FFTW.fftw_execute(this._plan); } }

		public unsafe void SetInput(int x, int y, double value)
		{
			long index = x * this._height + y;
			double* ptr = (double*)this._in.ToPointer();
			ptr[index] = value;
		}

		public unsafe Complex GetOutput(int x, int y)
		{
			bool conj = false;
			if (y > this._mHeight)
			{
				y = (int)(2 * this._mHeight) - y;
				x = (int)this._width - x - 1;
				conj = true;
			}
			long index = x * this._mHeight + y;
			Complex* ptr = (Complex*)this._out.ToPointer();
			Complex RetVal = ptr[index];
			if (conj)
				RetVal = Complex.Conjugate(RetVal);
			return RetVal;
		}

		public void Dispose()
		{
			FFTW.fftw_free(ref this._in);
			FFTW.fftw_free(ref this._out);
			FFTW.fftw_destroy_plan(ref this._plan);
		}
	}
}
