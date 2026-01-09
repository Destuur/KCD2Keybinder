using KDC2Keybinder.Core.Models;
using KDC2Keybinder.Core.Services;
using KDC2Keybinder.Core.Utils;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace KDC2Keybinder.Core
{
	public class KeybindManager
	{
		private readonly string gameDir;
		private readonly string dataDir;
		private readonly string modsDir;
		private readonly string? outputModDir;

		public KeybindManager(string gameDir, string dataDir, string modsDir, string? outputModDir)
		{
			this.gameDir = gameDir;
			this.dataDir = dataDir;
			this.modsDir = modsDir;
			this.outputModDir = outputModDir;
		}

		public void Generate()
		{
			var vanillaPakPath = Path.Combine(dataDir, "IPL_GameData.pak");
			var keybindService = new KeybindService(gameDir, dataDir, modsDir);
			keybindService.LoadVanillaPak("IPL_GameData.pak");
			var modKeybinds = keybindService.ScanMods();

			keybindService.MergeModActionMaps(modKeybinds);
			var modId = "keybinder";

			string modFolder;

			if (!string.IsNullOrEmpty(outputModDir))
			{
				modFolder = Path.Combine(outputModDir, "zz" + modId);
			}
			else
			{
				modFolder = Path.Combine(modsDir, "zz" + modId);
			}

			var outputDir = Path.Combine(modFolder, "Data", "Libs", "Config");
			Directory.CreateDirectory(outputDir);
			keybindService.ExportKeybinds(outputDir, modKeybinds);
			var modDescription = new ModDescription
			{
				Id = modId,
				Name = "KCD2 Keybinder",
				ModVersion = "1.0",
				Author = "Destuur",
				CreatedOn = DateTime.Now.ToString("yyyy-MM-dd"),
				ModifiesLevel = false,
			};
			var modService = new ModService(modsDir);
			modService.WriteModManifest(modDescription);
			var pakFilePath = Path.Combine(modFolder, "Data", $"{modId}.pak");
			modService.CreateModPak(Path.Combine(modFolder, "Data"), pakFilePath);
			Directory.Delete(Path.Combine(modFolder, "Data", "Libs"), true);
			Console.WriteLine($"Mod '{modId}' erfolgreich generiert in: {modFolder}");
		}

	}
}
