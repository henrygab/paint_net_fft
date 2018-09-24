using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;

namespace ArgusPaintNet.Convolution
{
    internal static class PresetExtensions
	{
		public static Dictionary<string,ConvolutionConfigEffectToken> ToDictionary(this Preset[] presets)
		{
			return Preset.ToDictionary(presets);
		}

		public static Preset[] ToPresets(this Dictionary<string,ConvolutionConfigEffectToken> dict)
		{
			return Preset.FromDictionary(dict);
		}
	}

	public class Preset
	{
		public string Name { get; set; }
		public ConvolutionConfigEffectToken Token { get; set; }

		public Preset()
		{
			this.Name = null;
			this.Token = null;
		}

		public Preset(string name, ConvolutionConfigEffectToken token)
		{
			this.Name = name;
			this.Token = new ConvolutionConfigEffectToken(token);
		}

		public static Dictionary<string,ConvolutionConfigEffectToken> ToDictionary(Preset[] presets)
		{
			Dictionary<string, ConvolutionConfigEffectToken> dict = new Dictionary<string, ConvolutionConfigEffectToken>(presets.Length);
			foreach (Preset preset in presets)
				dict.Add(preset.Name, preset.Token);
			return dict;
		}

		public static Preset[] FromDictionary(Dictionary<string,ConvolutionConfigEffectToken> dict)
		{
			Preset[] RetVal = new Preset[dict.Count];
			int index = 0;
			foreach (KeyValuePair<string,ConvolutionConfigEffectToken> item in dict)
			{
				RetVal[index] = new Preset(item.Key, item.Value);
				index++;
			}
			return RetVal;
		}

		public static Preset[] LoadFromFile(string filename)
		{
			if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
				return new Preset[0];
			Preset[] presets;
			XmlSerializer xs = new XmlSerializer(typeof(Preset[]));
			using (FileStream stream = new FileStream(filename, FileMode.Open))
			{
				try { presets = xs.Deserialize(stream) as Preset[]; }
				catch { return new Preset[0]; }
				if (presets == null)
					return new Preset[0];
			}
			List<Preset> RetVal = new List<Preset>(presets.Length);
			foreach(Preset preset in presets)
			{
				if (string.IsNullOrEmpty(preset.Name))
					continue;
				if (preset.Token == null)
					continue;
				if (preset.Token.Kernel == null)
					continue;
				RetVal.Add(preset);
			}
			return RetVal.ToArray();
		}

		public static void SaveToFile(string filename, Preset[] presets)
		{
			XmlSerializer xs = new XmlSerializer(typeof(Preset[]));
			using (FileStream stream = new FileStream(filename, FileMode.Create))
			{
				xs.Serialize(stream, presets);
			}
		}

		public static Preset[] DefaultPresets
		{
			get
			{
				return new Preset[]
				{
					new Preset("Identity", new ConvolutionConfigEffectToken(new float[,] { { 1} }, 1, true)),
					new Preset("EdgeDetection", new ConvolutionConfigEffectToken(new float[,] {
						{ -1, -1, -1 },
						{ -1, 8, -1 },
						{ -1, -1, -1 } }, 1, true)),
					new Preset("Sobel3X", new ConvolutionConfigEffectToken(new float[,] {
						{ 1, 0, -1 },
						{ 2, 0, -2 },
						{ 1, 0, -1 } }, 1, true)),
					new Preset("Sobel3Y", new ConvolutionConfigEffectToken(new float[,] {
						{ 1, 2, 1 },
						{ 0, 0, 0 },
						{ -1, -2, -1 } }, 1, true)),
					new Preset("Sobel5X", new ConvolutionConfigEffectToken(new float[,] {
						{ -4, -5, 0, 5, 4 },
						{ -8, -10, 0, 10, 8 },
						{ -10, -20, 0, 20, 10 },
						{ -8, -10, 0, 10, 8 },
						{ -4, -5, 0, 5, 4 } }, 1, true)),
					new Preset("Sobel5Y", new ConvolutionConfigEffectToken(new float[,] {
						{ -4, -8, -10, -8, -4 },
						{ -5, -10, -20, -10, -5 },
						{ 0, 0, 0, 0, 0 },
						{ 5, 10, 20, 10, 5 },
						{ 4, 8, 10, 8, 4 } }, 1, true))
				};
			}
		}
	}
}
