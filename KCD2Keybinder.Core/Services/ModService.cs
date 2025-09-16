using KDC2Keybinder.Core.Models;
using System.IO.Compression;
using System.Xml.Linq;

namespace KDC2Keybinder.Core.Services
{
	public class ModService
	{
		private string modDirectory;

		public ModService(string modDirectory)
		{
			this.modDirectory = modDirectory;
		}

		public List<ModDescription> ModCollection { get; private set; } = new();
		public List<ModDescription> ExternalModCollection { get; private set; } = new();


		private void FillModCollection(string modPath, List<ModDescription> collection)
		{
			var modDescription = ReadModMetadata(modPath);

			if (!collection.Any(x => x.Id == modDescription.Id))
			{
				collection.Add(modDescription);
			}
		}

		public ModDescription CreateNewMod(string name, string description, string author, string version, DateTime createdOn, string modId, bool modifiesLevel, List<string> supportedGameVersions)
		{
			if (string.IsNullOrWhiteSpace(name) ||
				string.IsNullOrWhiteSpace(modId) ||
				string.IsNullOrWhiteSpace(version))
			{
				return new ModDescription();
			}

			if (ModCollection.FirstOrDefault(x => x.Id == modId) is not null)
			{
				return new ModDescription();
			}

			var newMod = new ModDescription
			{
				Name = name,
				Description = description,
				Author = author,
				ModVersion = version,
				CreatedOn = createdOn.ToString("yyyy-MM-dd"),
				Id = modId,
				ModifiesLevel = modifiesLevel,
			};

			ModCollection.Add(newMod);
			return newMod;
		}

		private ModDescription ReadModMetadata(string modPath)
		{
			var modFiles = Directory.GetFiles(modPath);
			if (modFiles.Length == 0)
			{
				var msg = $"No files found in mod folder: {modPath}";
				throw new FileNotFoundException(msg);
			}

			var modFileUri = modFiles.FirstOrDefault(x => x.Contains("manifest"));
			if (string.IsNullOrEmpty(modFileUri))
			{
				var msg = $"Mod info file missing in {modPath}";
				throw new FileNotFoundException(msg);
			}

			XDocument doc;
			try
			{
				doc = XDocument.Load(modFileUri);
			}
			catch (Exception ex)
			{
				throw;
			}

			var root = doc.Root ?? throw new InvalidDataException("Mod file has no root element");
			var info = root.Element("info") ?? throw new InvalidDataException("Missing <info> element");

			var modifiesLevel = bool.TryParse(info.Element("modifies_level")?.Value, out var modifies) && modifies;

			var modDescription = new ModDescription
			{
				Name = info.Element("name")?.Value,
				Description = info.Element("description")?.Value,
				Author = info.Element("author")?.Value,
				ModVersion = info.Element("version")?.Value,
				CreatedOn = info.Element("created_on")?.Value,
				Id = info.Element("modid")?.Value,
				ModifiesLevel = modifiesLevel,
			};

			return modDescription;
		}

		private bool IsValidModDescription(ModDescription mod)
		{
			return mod != null
				&& !string.IsNullOrWhiteSpace(mod.Id)
				&& !string.IsNullOrWhiteSpace(mod.Name);
		}

		public bool WriteModManifest(ModDescription mod)
		{
			if (IsValidModDescription(mod) == false)
			{
				return false;
			}

			try
			{
				var modRootPath = Path.Combine(modDirectory, "zz" + mod.Id);
				var dataPath = Path.Combine(modRootPath, "Data");
				var localizationPath = Path.Combine(modRootPath, "Localization");
				var manifestPath = Path.Combine(modRootPath, "mod.manifest");

				Directory.CreateDirectory(dataPath);

				if (File.Exists(manifestPath))
				{
					return true;
				}

				var doc = new XDocument(
					new XDeclaration("1.0", "utf-8", null),
					new XElement("kcd_mod",
						new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
						new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
						new XElement("info",
							new XElement("name", mod.Name),
							new XElement("description", mod.Description),
							new XElement("author", mod.Author),
							new XElement("version", mod.ModVersion),
							new XElement("created_on", mod.CreatedOn),
							new XElement("modid", mod.Id),
							new XElement("modifies_level", mod.ModifiesLevel.ToString().ToLowerInvariant())
						)
					)
				);

				doc.Save(manifestPath);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public void CreateModPak(string baseFolder, string pakFilePath)
		{
			try
			{
				if (File.Exists(pakFilePath))
				{
					File.Delete(pakFilePath);
				}

				var outputFolder = Path.GetDirectoryName(pakFilePath);
				if (!string.IsNullOrEmpty(outputFolder) && !Directory.Exists(outputFolder))
				{
					Directory.CreateDirectory(outputFolder);
				}

				using var fs = new FileStream(pakFilePath, FileMode.CreateNew);
				using var archive = new ZipArchive(fs, ZipArchiveMode.Create);

				var files = Directory.GetFiles(baseFolder, "*", SearchOption.AllDirectories);

				foreach (var file in files)
				{
					if (Path.GetFullPath(file) == Path.GetFullPath(pakFilePath))
						continue;

					AddFileToArchive(archive, file, baseFolder);
				}

			}
			catch (Exception ex)
			{
				throw;
			}
		}

		private void AddFileToArchive(ZipArchive archive, string filePath, string baseFolder)
		{
			var relativePath = Path.GetRelativePath(baseFolder, filePath).Replace('\\', '/');
			var entry = archive.CreateEntry(relativePath, CompressionLevel.NoCompression);

			using var entryStream = entry.Open();
			using var fileStream = File.OpenRead(filePath);
			fileStream.CopyTo(entryStream);

		}
	}
}
