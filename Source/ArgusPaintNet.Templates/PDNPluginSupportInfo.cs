using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using PaintDotNet;

namespace ArgusPaintNet.Templates
{
	public class PDNPluginSupportInfo : IPluginSupportInfo
	{
		public string Author
		{
			get
			{
				return "Argus Magnus";
			}
		}
		public string Copyright
		{
			get
			{
				return ((AssemblyCopyrightAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
			}
		}

		public string DisplayName
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public Version Version
		{
			get
			{
				return base.GetType().Assembly.GetName().Version;
			}
		}

		public Uri WebsiteUri
		{
			get
			{
				return new Uri("http://www.getpaint.net/redirect/plugins.html");
			}
		}
	}
}
