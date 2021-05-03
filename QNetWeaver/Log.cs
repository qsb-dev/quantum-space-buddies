using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QNetWeaver
{
	public static class Log
	{
		public static void Warning(string msg)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine($"WARN : {msg}");
			Console.ResetColor();
		}

		public static void Error(string msg)
		{
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine($"ERR : {msg}");
			Console.ResetColor();
		}
	}
}
