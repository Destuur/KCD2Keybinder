using System.Xml.Serialization;

namespace KDC2Keybinder.Core.Models
{
	[XmlRoot("conflict")]
	public class Conflict
	{
		[XmlAttribute("name")]
		public string Name { get; set; }
		[XmlElement("superaction")]
		public List<Superaction> Superactions { get; set; } = new List<Superaction>();
		[XmlElement("include")]
		public List<IncludeConflict> Includes { get; set; } = new List<IncludeConflict>();
	}
}
