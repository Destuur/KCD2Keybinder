using KDC2Keybinder.Core;

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

			string? outputModDir = GetOption(args, "-o", "--output");

			if (!string.IsNullOrWhiteSpace(outputModDir))
			{
				outputModDir = Path.GetFullPath(outputModDir);
			}

			var manager = new KeybindManager(exeDir, pakFolder, modFolder, outputModDir);
			manager.Generate();
		}

		static string? GetOption(string[] args, params string[] names)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (names.Contains(args[i], StringComparer.OrdinalIgnoreCase))
				{
					if (i + 1 < args.Length)
						return args[i + 1];
				}
			}

			return null;
		}
	}
}
