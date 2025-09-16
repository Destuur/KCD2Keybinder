namespace KDC2Keybinder.Core.Utils
{
	public static class Logger
	{
		private static readonly string LogFile = "KeybindTool.log";

		public static void Log(string message)
		{
			Console.WriteLine(message);
			File.AppendAllText(LogFile, $"{DateTime.Now:HH:mm:ss} - {message}\n");
		}
	}
}
