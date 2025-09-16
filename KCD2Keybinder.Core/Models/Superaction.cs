using System.Xml.Serialization;

namespace KDC2Keybinder.Core.Models
{
	[XmlRoot("superaction")]
	public class Superaction
	{
		[XmlAttribute("name")]
		public string Name { get; set; }
		[XmlAttribute("ui_group")]
		public string UiGroup { get; set; }
		[XmlAttribute("ui_name")]
		public string UiName { get; set; }
		[XmlAttribute("ui_tooltip")]
		public string UiTooltip { get; set; }
		[XmlAttribute("keyboard")]
		public string Keyboard { get; set; }
		[XmlElement("action")]
		public List<Action> Actions { get; set; } = new List<Action>();
		[XmlElement("control")]
		public List<Control> Controls { get; set; } = new List<Control>();
	}

	public class ActionMap
	{
		[XmlAttribute] public string Name { get; set; }
		[XmlAttribute] public string Priority { get; set; } = "default";
		[XmlAttribute] public string Exclusivity { get; set; } = "0";
		[XmlElement("action")] public List<Action> Actions { get; set; } = new();
		[XmlElement("include")] public List<string> Includes { get; set; } = new();
	}
}
