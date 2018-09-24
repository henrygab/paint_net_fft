using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;

namespace ArgusPaintNet.FFT.FFTWInterop
{
    /// <summary>
    /// http://fftw.org/fftw3_doc/
    /// </summary>
    internal static class FFTW
	{
        private static readonly object _lock = new object();
		public const string DllName = "libfftw3-3.dll";
        private const string WisdomFile = "fftw.wisdom";

		public static bool IsAvailable { get; private set; }

		[DllImport("Kernel32.dll")]
        private static bool SetDllDirectory(string path);

		static FFTW()
		{
			string path;
			if (Environment.Is64BitProcess)
				path = "Effects/NativeBinaries/x64";
			else
				path = "Effects/NativeBinaries/x86";

			path = Path.Combine(Environment.CurrentDirectory, path);
			SetDllDirectory(path);

			IsAvailable = true;
			try { fftw_init_threads(); }
			catch { IsAvailable = false; }

			if (IsAvailable)
			{
				fftw_plan_with_nthreads(Environment.ProcessorCount);

				path = Path.Combine(Path.GetTempPath(), WisdomFile);
				if (File.Exists(path))
					fftw_import_wisdom_from_filename(path);

				Application.ApplicationExit += (sender, e) => { fftw_export_wisdom(); };
			}
		}

		#region Threading

		[DllImport(DllName)]
        private static int fftw_init_threads();

		[DllImport(DllName)]
        private static void fftw_plan_with_nthreads(int numberOfThreads);

		#endregion

		#region Memory Management

		[DllImport(DllName)]
        private static IntPtr fftw_malloc(int size);

		[DllImport(DllName)]
        private static void fftw_free(IntPtr ptr);

		public static void fftw_free(ref IntPtr ptr) { lock (_lock) { fftw_free(ptr); ptr = IntPtr.Zero; } }

		[DllImport(DllName)]
        private static IntPtr fftw_alloc_real(int size);

		[DllImport(DllName)]
        private static IntPtr fftw_alloc_complex(int size);

		[DllImport(DllName)]
        private static void fftw_destroy_plan(IntPtr plan);

		public static void fftw_destroy_plan(ref IntPtr plan) { lock (_lock) { fftw_destroy_plan(plan); plan = IntPtr.Zero; } }

		#endregion

		#region Planners

		[DllImport(DllName)]
        private static IntPtr fftw_plan_dft_r2c_2d(int n0, int n1, IntPtr doubleIn, IntPtr complexOut, PlanningFlags flags = PlanningFlags.Default);

		public static Real2ComplexPlan fftw_plan_dft_r2c_2d(int width, int height)
		{
			lock(_lock)
			{
				IntPtr inReal = FFTW.fftw_alloc_real(width*height);
				IntPtr outComplex = FFTW.fftw_alloc_complex(width*(height/2+1));
				IntPtr plan = FFTW.fftw_plan_dft_r2c_2d(width, height, inReal, outComplex);
				return new Real2ComplexPlan(width, height, inReal, outComplex, plan);
			}
		}

		[DllImport(DllName)]
        private static IntPtr fftw_plan_dft_2d(int n0, int n1, IntPtr complexIn, IntPtr complexOut, int sign, PlanningFlags flags = PlanningFlags.Default);

		public static ComplexPlan fftw_plan_dft_2d(int width, int height, bool forward)
		{
			lock (_lock)
			{
				IntPtr inComplex = FFTW.fftw_alloc_complex(width * height);
				IntPtr outComplex = FFTW.fftw_alloc_complex(width * height);
				int sign = -1;
				if (forward)
					sign = 1;
				IntPtr plan = FFTW.fftw_plan_dft_2d(width, height, inComplex, outComplex, sign);
				return new ComplexPlan(width, height, inComplex, outComplex, plan);
			}
		}

		public static TwoWayPlan GetTwoWayPlan(int width, int height)
		{
			lock(_lock)
			{
				int size = width*height;
				IntPtr _in = FFTW.fftw_alloc_complex(size);
				IntPtr _out = FFTW.fftw_alloc_complex(size);
				IntPtr planForwards = FFTW.fftw_plan_dft_2d(width, height, _in, _out, 1);
				IntPtr planBackwards = FFTW.fftw_plan_dft_2d(width, height, _out, _in, -1);
				return new TwoWayPlan(width, height, _in, _out, planForwards, planBackwards);
			}
		}

		#endregion

		#region Execute

		[DllImport(DllName)]
		public static extern void fftw_execute(IntPtr fftw_plan); // Thread-Safe

		#endregion

		#region Wisdom

		[DllImport(DllName)]
        private static int fftw_export_wisdom_to_filename(string filename);

		[DllImport(DllName)]
        private static int fftw_import_wisdom_from_filename(string filename);

        private static bool fftw_export_wisdom()
		{
			lock (_lock)
			{
				string path = Path.Combine(Path.GetTempPath(), WisdomFile);
				return fftw_export_wisdom_to_filename(path) != 0;
			}
		}

		#endregion

		[DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
		public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
	}
}
