using System.Xml.Serialization;

namespace KDC2Keybinder.Core.Models
{
	public class Action
	{
		[XmlAttribute] public string name { get; set; }
		[XmlAttribute] public string map { get; set; }
		[XmlAttribute] public string onPress { get; set; }
		[XmlAttribute] public string onRelease { get; set; }
		[XmlAttribute] public string onHold { get; set; }
		[XmlAttribute] public string retriggerable { get; set; }
		[XmlAttribute] public string holdTriggerDelay { get; set; }
		[XmlAttribute] public string holdRepeatDelay { get; set; }
	}
}
