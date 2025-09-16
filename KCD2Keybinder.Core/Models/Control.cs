using System.Xml.Serialization;

namespace KDC2Keybinder.Core.Models
{
	public class Control
	{
		[XmlAttribute("input")]
		public string Input { get; set; }
		[XmlAttribute("controller")]
		public string Controller { get; set; }
	}
}
