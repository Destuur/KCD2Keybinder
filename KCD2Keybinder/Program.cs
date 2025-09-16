using KDC2Keybinder.Core;
using KDC2Keybinder.Core.Models;
using KDC2Keybinder.Core.Utils;
using Action = KDC2Keybinder.Core.Models.Action;

namespace KCD2Keybinder
{
	internal class Program
	{
		static void Main(string[] args)
		{
			string exeDir = AppContext.BaseDirectory;
			string pakFolder = Path.Combine(exeDir, "..", "..", "Data");
			pakFolder = Path.GetFullPath(pakFolder);
			string modFolder = Path.Combine(exeDir, "..", "..", "Mods");
			modFolder = Path.GetFullPath(modFolder);
			var manager = new KeybindManager(exeDir, pakFolder, modFolder);
			manager.Generate();
		}
	}
}
