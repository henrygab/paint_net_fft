using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using PaintDotNet;

namespace ArgusPaintNet.EdgeDetection
{
	public class PluginSupportInfo : IPluginSupportInfo
	{
        public string Author => "Argus Magnus";
        public string Copyright => ((AssemblyCopyrightAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;

        public string DisplayName => EdgeDetectionEffect.StaticName;

        public Version Version => base.GetType().Assembly.GetName().Version;

        public Uri WebsiteUri => new Uri("http://forums.getpaint.net/index.php?/topic/32327-edge-detection/");
    }
}
