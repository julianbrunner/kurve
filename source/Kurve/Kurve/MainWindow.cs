using System;
using Gtk;
using Gdk;
using Cairo;

public partial class MainWindow: Gtk.Window
{	
	public MainWindow(): base (Gtk.WindowType.Toplevel)
	{
		Build();
	}
	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}
	protected void OnDrawingarea1ExposeEvent(object o, ExposeEventArgs args)
	{
		using (Context context = CairoHelper.Create(drawingarea1.GdkWindow))
		{
			context.MoveTo(10, 10);
			context.LineTo(100, 200);

			context.Color = new Cairo.Color(0, 0, 1);
			context.Stroke();

			context.Target.Dispose();
		}
	}

}
