using System.IO.Compression;

namespace KDC2Keybinder.Core.Services
{
	public class PakReader : IDisposable
	{
		private readonly ZipArchive _archive;
		private readonly Dictionary<string, ZipArchiveEntry> _entries;

		public PakReader(string pakPath)
		{
			var stream = new FileStream(pakPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			_archive = new ZipArchive(stream, ZipArchiveMode.Read);
			_entries = _archive.Entries.ToDictionary(e => e.FullName.Replace('\\', '/'), StringComparer.OrdinalIgnoreCase);
		}

		public string ReadFile(string nameOrPath)
		{
			var normalizedPath = nameOrPath.Replace('\\', '/');

			if (_entries.TryGetValue(normalizedPath, out var entry))
			{
				using var reader = new StreamReader(entry.Open());
				return reader.ReadToEnd();
			}

			var fallback = _entries.Values
				.FirstOrDefault(e => e.FullName.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase));

			if (fallback != null)
			{
				using var reader = new StreamReader(fallback.Open());
				return reader.ReadToEnd();
			}

			Console.WriteLine($"[WARN] Could not find file for path: {normalizedPath}");
			return null;
		}

		public IEnumerable<string> GetAllFiles()
		{
			return _entries.Keys;
		}

		public void Dispose()
		{
			_archive.Dispose();
		}
	}
}
