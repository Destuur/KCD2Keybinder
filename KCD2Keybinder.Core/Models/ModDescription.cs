namespace KDC2Keybinder.Core.Models
{
	public class ModDescription
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Author { get; set; } = string.Empty;
		public string ModVersion { get; set; } = string.Empty;
		public string CreatedOn { get; set; } = string.Empty;
		public string Id { get; set; } = string.Empty;
		public bool ModifiesLevel { get; set; }
		public string path { get; set; } = string.Empty;
		public List<Keybind> Keybinds { get; set; } = new List<Keybind>();
	}
}
