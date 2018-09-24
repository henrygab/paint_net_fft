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
        internal static class NativeMethods
        {
            [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern bool SetDllDirectory(string path);

            [DllImport(DllName)]
            internal static extern int fftw_init_threads();

            [DllImport(DllName)]
            internal static extern void fftw_plan_with_nthreads(int numberOfThreads);

            [DllImport(DllName)]
            internal static extern IntPtr fftw_malloc(int size);

            [DllImport(DllName)]
            internal static extern void fftw_free(IntPtr ptr);

            [DllImport(DllName)]
            internal static extern IntPtr fftw_alloc_real(int size);

            [DllImport(DllName)]
            internal static extern IntPtr fftw_alloc_complex(int size);

            [DllImport(DllName)]
            internal static extern void fftw_destroy_plan(IntPtr plan);

            [DllImport(DllName)]
            internal static extern IntPtr fftw_plan_dft_r2c_2d(int n0, int n1, IntPtr doubleIn, IntPtr complexOut, PlanningFlags flags = PlanningFlags.Default);

            [DllImport(DllName)]
            internal static extern IntPtr fftw_plan_dft_2d(int n0, int n1, IntPtr complexIn, IntPtr complexOut, int sign, PlanningFlags flags = PlanningFlags.Default);

            [DllImport(DllName)]
            internal static extern void fftw_execute(IntPtr fftw_plan); // Thread-Safe

            [DllImport(DllName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            internal static extern int fftw_export_wisdom_to_filename(string filename);

            [DllImport(DllName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            internal static extern int fftw_import_wisdom_from_filename(string filename);

            [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
            internal static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
        }

        private static readonly object _lock = new object();
        public const string DllName = "libfftw3-3.dll";
        private const string WisdomFile = "fftw.wisdom";

        public static bool IsAvailable { get; private set; }

        static FFTW()
        {
            string path;
            if (Environment.Is64BitProcess)
            {
                path = "Effects/NativeBinaries/x64";
            }
            else
            {
                path = "Effects/NativeBinaries/x86";
            }

            path = Path.Combine(Environment.CurrentDirectory, path);
            NativeMethods.SetDllDirectory(path);

            IsAvailable = true;
            try { NativeMethods.fftw_init_threads(); }
            catch { IsAvailable = false; }

            if (IsAvailable)
            {
                NativeMethods.fftw_plan_with_nthreads(Environment.ProcessorCount);

                path = Path.Combine(Path.GetTempPath(), WisdomFile);
                if (File.Exists(path))
                {
                    NativeMethods.fftw_import_wisdom_from_filename(path);
                }

                Application.ApplicationExit += (sender, e) => { fftw_export_wisdom(); };
            }
        }
        #region Memory Management

        public static void fftw_free(ref IntPtr ptr)
        {
            lock (_lock)
            {
                if (ptr != IntPtr.Zero)
                {
                    NativeMethods.fftw_free(ptr);
                    ptr = IntPtr.Zero;
                }
            }
        }

        public static void fftw_destroy_plan(ref IntPtr plan)
        {
            lock (_lock)
            {
                if (IntPtr.Zero != plan)
                {
                    NativeMethods.fftw_destroy_plan(plan);
                    plan = IntPtr.Zero;
                }
            }
        }

        #endregion

        #region Planners

        public static Real2ComplexPlan fftw_plan_dft_r2c_2d(int width, int height)
        {
            lock (_lock)
            {
                IntPtr inReal = NativeMethods.fftw_alloc_real(width * height);
                IntPtr outComplex = NativeMethods.fftw_alloc_complex(width * (height / 2 + 1));
                IntPtr plan = NativeMethods.fftw_plan_dft_r2c_2d(width, height, inReal, outComplex);
                return new Real2ComplexPlan(width, height, inReal, outComplex, plan);
            }
        }

        public static ComplexPlan fftw_plan_dft_2d(int width, int height, bool forward)
        {
            lock (_lock)
            {
                IntPtr inComplex = NativeMethods.fftw_alloc_complex(width * height);
                IntPtr outComplex = NativeMethods.fftw_alloc_complex(width * height);
                int sign = -1;
                if (forward)
                {
                    sign = 1;
                }

                IntPtr plan = NativeMethods.fftw_plan_dft_2d(width, height, inComplex, outComplex, sign);
                return new ComplexPlan(width, height, inComplex, outComplex, plan);
            }
        }

        public static TwoWayPlan GetTwoWayPlan(int width, int height)
        {
            lock (_lock)
            {
                int size = width * height;
                IntPtr _in = NativeMethods.fftw_alloc_complex(size);
                IntPtr _out = NativeMethods.fftw_alloc_complex(size);
                IntPtr planForwards = NativeMethods.fftw_plan_dft_2d(width, height, _in, _out, 1);
                IntPtr planBackwards = NativeMethods.fftw_plan_dft_2d(width, height, _out, _in, -1);
                return new TwoWayPlan(width, height, _in, _out, planForwards, planBackwards);
            }
        }

        #endregion

        #region Wisdom
        private static bool fftw_export_wisdom()
        {
            lock (_lock)
            {
                string path = Path.Combine(Path.GetTempPath(), WisdomFile);
                return NativeMethods.fftw_export_wisdom_to_filename(path) != 0;
            }
        }
        #endregion
    }
}
