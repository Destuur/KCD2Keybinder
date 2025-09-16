namespace KDC2Keybinder.Core.Models
{
	public class Keybind
	{
		public string Name { get; set; }          // z.B. "super_jump" oder "hw_toggle"
		public string UiGroup { get; set; }       // z.B. modId
		public string UiName { get; set; }        // z.B. "ui_keybind_super_jump"
		public string Description { get; set; }   // aus Lua-Kommentar oder commands.txt
		public string Map { get; set; }           // Actionmap Name
	}
}
