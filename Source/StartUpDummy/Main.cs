using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartUpDummy
{
	/// <summary>
	/// This project is set up to copy the effect assemblies in this solution to the paint.net/Effects folder
	/// and use paint.net as StartUp Program used for debugging.
	/// </summary>
	/// <remarks>
	/// This project is set up to copy the effect assemblies in this solution to the paint.net/Effects folder
	/// and use paint.net as StartUp Program used for debugging. For this to work, make sure that
	/// 1.	paint.net is installed to the solution folder (the folder containing the ArgusPaintNet.sln file
	///		should also contain the paint.net folder).
	/// 2.	All the effect assemblies are named according to the schema: ArgusPaintNet.'EffectName'.dll.
	///		If you want to use different naming schemas you need to modify this project's post-build event.
	/// 3.	Add references to the effect assemblies to this project.
	/// </remarks>
	public static class Program
	{
		public static void Main() { }
	}
}
