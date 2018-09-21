

# Install Instructions

To install these or one of these plugins, follow the instructions at 

<http://forums.getpaint.net/index.php?/topic/1708-how-to-install-pluginsgeneral-plugin-troubleshooting-thread/>

The individual plugins reside in the "Effect Assemblies/ArgusPaintNet.<PluginName>.dll" files.

If you only want to install specific plugins, you can copy the common ArgusPaintNet.Shared.dll and the files listed below.

# Plugins and their required files

Reverse Blend: <http://forums.getpaint.net/index.php?/topic/32165-reverse-blend/>
- ArgusPaintNet.ReverseBlend.dll
- ArgusPaintNet.Shared.dll 

FFT (Fast Fourier Transform related): <http://forums.getpaint.net/index.php?/topic/32205-fft-ifft-effect/>
- ArgusPaintNet.FFT.dll
- ArgusPaintNet.Shared.dll
- NativeBinaries folder

Convolution Filter: <http://forums.getpaint.net/index.php?/topic/32295-convolution-filter/>
- ArgusPaintNet.Convolution.dll
- ArgusPaintNet.Shared.dll

Edge Detection: <http://forums.getpaint.net/index.php?/topic/32327-edge-detection/>
- ArgusPaintNet.EdgeDetection.dll
- ArgusPaintNet.Shared.dll


# For Developers

These Plugins were developed in Visual Studio and are distributed under the Ms-Pl License (see License.txt).
You can find the VS Solution in the Source Folder. The Solution and Projects are set up so that they can be debugged with VS, as long as paint.net is installed to the Solution Directory (the paint.net Folder must be in the same folder as the ArgusPaintNet.sln File). For further Instructions, see the Comments in the Source/StartUpDummy/Main.cs File.

The FFT plugin uses FFTW binaries from <http://www.fftw.org/download.html>; Could also use NuGet package <https://www.nuget.org/packages/FFTW.NET>).  See 
<https://github.com/ArgusMagnus/FFTW.NET>

# Original Author
Author:		ArgusMagnus
Contact:	argusmagnus@outlook.com

