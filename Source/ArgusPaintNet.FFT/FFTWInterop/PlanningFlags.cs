namespace ArgusPaintNet.FFT.FFTWInterop
{
    [System.Flags]
    public enum PlanningFlags : uint
    {
        FFTW_MEASURE = (0U),
        FFTW_DESTROY_INPUT = (1U << 0),
        FFTW_UNALIGNED = (1U << 1),
        FFTW_CONSERVE_MEMORY = (1U << 2),
        FFTW_EXHAUSTIVE = (1U << 3), /* NO_EXHAUSTIVE is default */
        FFTW_PRESERVE_INPUT = (1U << 4), /* cancels FFTW_DESTROY_INPUT */
        FFTW_PATIENT = (1U << 5), /* IMPATIENT is default */
        FFTW_ESTIMATE = (1U << 6),
        FFTW_WISDOM_ONLY = (1U << 21),

        Default = FFTW_MEASURE
    }
}
