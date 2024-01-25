using System;
using System.IO;
using System.Threading.Tasks;
using GameHelper.Utils;

namespace GameHelper;

internal class Program
{
	private static async Task Main()
	{
		AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs exceptionArgs)
		{
			string value = "Program exited with message:\n " + exceptionArgs.ExceptionObject;
			File.AppendAllText("Error.log", $"{DateTime.Now:g} {value}\r\n{new string('-', 30)}\r\n");
			Environment.Exit(1);
		};
		using (Core.Overlay = new GameOverlay(MiscHelper.GenerateRandomString()))
		{
			await Core.Overlay.Run();
		}
	}
}
