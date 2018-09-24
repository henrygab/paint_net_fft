using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using PaintDotNet;

namespace ArgusPaintNet.ReverseBlend
{
	public class PluginSupportInfo : IPluginSupportInfo
	{
        public string Author => "Argus Magnus";
        public string Copyright => ((AssemblyCopyrightAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;

        public string DisplayName => "Reverse Blend";

        public Version Version => base.GetType().Assembly.GetName().Version;

        public Uri WebsiteUri => new Uri("http://forums.getpaint.net/index.php?/topic/32165-reverse-blend/");
    }
}
