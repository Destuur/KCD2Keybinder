using KDC2Keybinder.Core.Models;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Action = KDC2Keybinder.Core.Models.Action;

namespace KDC2Keybinder.Core.Services
{
	public class KeybindService
	{
		private readonly string gameRoot;
		private readonly string dataRoot;
		private readonly string modsRoot;

		private readonly Dictionary<string, ActionMap> vanillaActionMaps = new();
		private readonly Dictionary<string, Superaction> vanillaSuperactions = new();
		private XDocument vanillaDefaultProfileDoc;
		private XDocument vanillaSuperactionsDoc;

		public KeybindService(string gameRoot, string dataRoot, string modsRoot)
		{
			this.gameRoot = gameRoot;
			this.dataRoot = dataRoot;
			this.modsRoot = modsRoot;
		}

		#region --- Vanilla Loading ---

		public void LoadVanillaPak(string pakFileName)
		{
			var pakPath = Path.Combine(dataRoot, pakFileName);
			using var reader = new PakReader(pakPath);

			// defaultProfile.xml komplett halten
			var defaultProfileXml = reader.ReadFile("Libs/Config/defaultProfile.xml");
			if (!string.IsNullOrEmpty(defaultProfileXml))
			{
				vanillaDefaultProfileDoc = XDocument.Parse(defaultProfileXml);
				// Optional: bestehende ActionMaps in Dictionary für schnelle Suche extrahieren
				ParseDefaultProfile(defaultProfileXml, vanillaActionMaps);
			}

			// keybindSuperactions.xml komplett halten
			var keybindXml = reader.ReadFile("Libs/Config/keybindSuperactions.xml");
			if (!string.IsNullOrEmpty(keybindXml))
			{
				vanillaSuperactionsDoc = XDocument.Parse(keybindXml);
				ParseSuperactions(keybindXml, vanillaSuperactions);
			}
		}

		private void ParseDefaultProfile(string xmlContent, Dictionary<string, ActionMap> target)
		{
			var doc = XDocument.Parse(xmlContent);
			foreach (var mapEl in doc.Root.Elements("actionmap"))
			{
				var map = new ActionMap
				{
					Name = mapEl.Attribute("name")?.Value ?? "",
					Priority = mapEl.Attribute("priority")?.Value ?? "default",
					Exclusivity = mapEl.Attribute("exclusivity")?.Value ?? "0"
				};

				foreach (var actionEl in mapEl.Elements("action"))
				{
					var action = new Action
					{
						name = actionEl.Attribute("name")?.Value,
						map = actionEl.Attribute("map")?.Value,
						onPress = actionEl.Attribute("onPress")?.Value,
						onRelease = actionEl.Attribute("onRelease")?.Value,
						onHold = actionEl.Attribute("onHold")?.Value,
						retriggerable = actionEl.Attribute("retriggerable")?.Value,
						holdTriggerDelay = actionEl.Attribute("holdTriggerDelay")?.Value,
						holdRepeatDelay = actionEl.Attribute("holdRepeatDelay")?.Value
					};
					map.Actions.Add(action);
				}

				target[map.Name] = map;
			}
		}

		private void ParseSuperactions(string xmlContent, Dictionary<string, Superaction> target)
		{
			var doc = XDocument.Parse(xmlContent);
			foreach (var saEl in doc.Root.Elements("superaction"))
			{
				var sa = new Superaction
				{
					Name = saEl.Attribute("name")?.Value,
					UiGroup = saEl.Attribute("ui_group")?.Value,
					UiName = saEl.Attribute("ui_name")?.Value,
					UiTooltip = saEl.Attribute("ui_tooltip")?.Value,
					Keyboard = saEl.Attribute("keyboard")?.Value
				};

				foreach (var act in saEl.Elements("action"))
				{
					sa.Actions.Add(new Action
					{
						name = act.Attribute("name")?.Value,
						map = act.Attribute("map")?.Value,
						onPress = act.Attribute("onPress")?.Value,
						onRelease = act.Attribute("onRelease")?.Value,
						onHold = act.Attribute("onHold")?.Value,
						retriggerable = act.Attribute("retriggerable")?.Value,
						holdTriggerDelay = act.Attribute("holdTriggerDelay")?.Value,
						holdRepeatDelay = act.Attribute("holdRepeatDelay")?.Value
					});
				}

				foreach (var control in saEl.Elements("control"))
				{
					sa.Controls.Add(new Control
					{
						Input = control.Attribute("input")?.Value,
						Controller = control.Attribute("controller")?.Value
					});
				}

				target[sa.Name] = sa;
			}
		}

		#region --- DefaultProfile Merge & Export ---

		public void MergeModActionMaps(List<Keybind> modKeybinds)
		{
			// Alle existierenden ModIds ermitteln
			var existingModIds = Directory.GetDirectories(modsRoot)
										  .Select(d => Path.GetFileName(d))
										  .ToHashSet(StringComparer.OrdinalIgnoreCase);

			// Alte Actions von nicht mehr existierenden Mods entfernen
			foreach (var map in vanillaActionMaps.Values)
			{
				map.Actions.RemoveAll(a => !string.IsNullOrWhiteSpace(a.map) && !existingModIds.Contains(a.map));
			}

			// Mod-Keybinds in die passende ActionMap einfügen oder neue ActionMaps erzeugen
			foreach (var kb in modKeybinds)
			{
				if (string.IsNullOrWhiteSpace(kb.Map)) kb.Map = "movement"; // default fallback

				if (!vanillaActionMaps.TryGetValue(kb.Map, out var map))
				{
					map = new ActionMap
					{
						Name = kb.Map,
						Priority = "pure_include",
						Exclusivity = "0"
					};
					vanillaActionMaps[kb.Map] = map;
				}

				// Prüfen, ob Aktion schon existiert
				if (!map.Actions.Any(a => a.name == kb.Name))
				{
					map.Actions.Add(new Action
					{
						name = kb.Name,
						map = kb.Map,
						onPress = "1",
						onRelease = "1"
						// Optional: andere Standardattribute setzen
					});
				}
			}
		}


		public void ExportKeybinds(string outputDir, List<Keybind> keybinds)
		{
			Directory.CreateDirectory(outputDir);

			// --- Superactions ---
			var saDoc = vanillaSuperactionsDoc ?? new XDocument(new XElement("keybinds"));

			// Alle ModIds sammeln
			var modIds = keybinds.Select(kb => kb.UiGroup ?? "unknown").Distinct();

			// ui_group für jede ModId hinzufügen, falls noch nicht vorhanden
			foreach (var modId in modIds)
			{
				if (!saDoc.Root.Elements("ui_group").Any(x => (string)x.Attribute("name") == modId))
				{
					var modGroup = new XElement("ui_group",
						new XAttribute("name", modId),
						new XAttribute("ui_label", $"ui_keybinds_group_{modId}")
					);
					saDoc.Root.Add(modGroup);
				}
			}

			// Superactions ergänzen
			foreach (var kb in keybinds)
			{
				if (saDoc.Root.Elements("superaction").Any(x => (string)x.Attribute("name") == kb.Name))
					continue;

				var saEl = new XElement("superaction",
					new XAttribute("name", kb.Name),
					new XAttribute("ui_group", kb.UiGroup ?? "unknown"),
					new XAttribute("ui_name", kb.UiName ?? $"ui_keybind_{kb.Name}"),
					new XAttribute("keyboard", "writeable"),
					new XElement("action",
						new XAttribute("name", kb.Name),
						new XAttribute("map", kb.Map ?? "open_menu")
					),
					new XElement("control",
						new XAttribute("input", ""),
						new XAttribute("controller", "keyboard")
					)
				);

				saDoc.Root.Add(saEl);
			}

			// --- DefaultProfile ---
			var dpDoc = vanillaDefaultProfileDoc ?? new XDocument(new XElement("defaultProfile"));

			foreach (var kb in keybinds)
			{
				var mapName = kb.Map ?? "open_menu";

				var mapEl = dpDoc.Root.Elements("actionmap")
					.FirstOrDefault(x => (string)x.Attribute("name") == mapName);

				if (mapEl == null)
				{
					mapEl = new XElement("actionmap",
						new XAttribute("name", mapName),
						new XAttribute("priority", "pure_include"),
						new XAttribute("exclusivity", "0"),
						new XComment("only for include")
					);
					dpDoc.Root.Add(mapEl);
				}

				// Prüfen, ob Action bereits existiert
				if (mapEl.Elements("action").Any(x => (string)x.Attribute("name") == kb.Name))
					continue;

				var actionEl = new XElement("action",
					new XAttribute("name", kb.Name),
					new XAttribute("consoleCmd", "1"),
					new XAttribute("onRelease", "1"),
					new XAttribute("keyboard", "_keybinds_ref_")
				);

				// _ctrl Varianten
				if (kb.Name.EndsWith("_ctrl"))
				{
					actionEl.SetAttributeValue("holdTriggerDelay", "0.5");
					actionEl.SetAttributeValue("onHold", "1");
					actionEl.SetAttributeValue("holdRepeatDelay", "-1");
					actionEl.SetAttributeValue("retriggerable", "0");
					actionEl.SetAttributeValue("xboxpad", "");
					actionEl.SetAttributeValue("pspad", "");
				}

				mapEl.Add(actionEl);
			}

			// --- Speichern ---
			var saPath = Path.Combine(outputDir, "keybindSuperactions.xml");
			saDoc.Save(saPath);

			var dpPath = Path.Combine(outputDir, "defaultProfile.xml");
			dpDoc.Save(dpPath);
		}




		#endregion


		#endregion

		#region --- Mods Scanning ---

		public List<Keybind> ScanMods()
		{
			var result = new List<Keybind>();
			foreach (var modDir in Directory.GetDirectories(modsRoot))
			{
				var modName = Path.GetFileName(modDir);
				if (modName.StartsWith("zz", StringComparison.OrdinalIgnoreCase))
					continue; // generierte Mod überspringen

				var manifest = Path.Combine(modDir, "mod.manifest");
				if (!File.Exists(manifest)) continue;

				var modId = GetModIdFromManifest(manifest);
				var pakFile = Directory.GetFiles(Path.Combine(modDir, "Data"), "*.pak").FirstOrDefault();
				if (pakFile == null) continue;

				using var pakReader = new PakReader(pakFile);
				var superactionXml = pakReader.ReadFile("Libs/Config/keybindSuperactions.xml");
				if (!string.IsNullOrEmpty(superactionXml))
				{
					ParseModSuperactions(superactionXml, modId, result);
				}

				var luaFiles = pakReader
					.GetAllFiles()
					.Where(f => f.EndsWith(".lua", StringComparison.OrdinalIgnoreCase));

				foreach (var lua in luaFiles)
				{
					var luaText = pakReader.ReadFile(lua);
					var bindings = ParseLuaBindings(luaText, modId);
					result.AddRange(bindings);
				}
			}

			return result;
		}

		private string GetModIdFromManifest(string manifestPath)
		{
			var doc = XDocument.Load(manifestPath);
			return doc.Root?.Element("info")?.Element("modid")?.Value ?? "";
		}

		private void ParseModSuperactions(string xmlContent, string modId, List<Keybind> target)
		{
			var doc = XDocument.Parse(xmlContent);
			foreach (var saEl in doc.Root.Elements("superaction"))
			{
				var kb = new Keybind
				{
					Name = saEl.Attribute("name")?.Value,
					UiGroup = modId,
					UiName = saEl.Attribute("ui_name")?.Value,
					Description = saEl.Attribute("ui_tooltip")?.Value,
					Map = saEl.Elements("action").FirstOrDefault()?.Attribute("map")?.Value
				};

				target.Add(kb);
			}
		}

		private IEnumerable<Keybind> ParseLuaBindings(string luaText, string modId)
		{
			var matches = Regex.Matches(luaText, @"---\s*@binding\s+(\w+)");
			foreach (Match match in matches)
			{
				yield return new Keybind
				{
					Name = match.Groups[1].Value,
					UiGroup = modId,
					UiName = $"ui_keybind_{match.Groups[1].Value}",
					Description = "",
					Map = ""
				};
			}
		}

		#endregion
	}
}
