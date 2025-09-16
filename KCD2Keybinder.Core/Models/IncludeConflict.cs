using System.Xml.Serialization;

namespace KDC2Keybinder.Core.Models
{
	public class IncludeConflict
	{
		[XmlAttribute("conflict")]
		public string Conflict { get; set; }
	}
}
