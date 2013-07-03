using System;
using Gtk;

namespace Kurve
{
	class MainClass
	{
		public static void Main(string[] parameters)
		{
			Application.Init();

			new MainWindow(parameters).Show();

			Application.Run();
		}
	}
}
