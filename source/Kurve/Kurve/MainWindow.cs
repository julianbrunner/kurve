using System;
using Gtk;
using Gdk;
using Cairo;
using Kurve.Curves;
using Krach.Basics;
using System.Linq;
using Krach.Extensions;
using System.Collections.Generic;
using System.Xml.Linq;
using Kurve.Interface;
using Kurve.Curves.Optimization;
using Kurve;
using Kurve.Component;

public partial class MainWindow : Gtk.Window
{
	readonly RootComponent rootComponent;

	public MainWindow(): base(Gtk.WindowType.Toplevel)
	{
		Build();

		this.rootComponent = new RootComponent(this);
		this.rootComponent.ComponentChanged += RootComponentChanged;
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		rootComponent.Dispose();

		Application.Quit();

		a.RetVal = true;
	}
	protected void OnExposeEvent(object o, ExposeEventArgs args)
	{
		using (Context context = CairoHelper.Create(GdkWindow))
		{
			rootComponent.Draw(context);

			context.Target.Dispose();
		}
	}
	protected void OnButtonPressEvent(object o, ButtonPressEventArgs args)
	{
		Vector2Double position = new Vector2Double(args.Event.X, args.Event.Y);
		MouseButton button = (MouseButton)args.Event.Button;

		rootComponent.MouseDown(position, button);
	}
	protected void OnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
	{
		Vector2Double position = new Vector2Double(args.Event.X, args.Event.Y);
		MouseButton button = (MouseButton)args.Event.Button;

		rootComponent.MouseUp(position, button);
	}
	protected void OnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
	{
		Vector2Double position = new Vector2Double(args.Event.X, args.Event.Y);

		rootComponent.MouseMove(position);
	}
	protected void OnScrollEvent(object o, ScrollEventArgs args)
	{
		Kurve.Interface.ScrollDirection scrollDirection;

		switch (args.Event.Direction)
		{
			case Gdk.ScrollDirection.Up: scrollDirection = Kurve.Interface.ScrollDirection.Up; break;
			case Gdk.ScrollDirection.Down: scrollDirection = Kurve.Interface.ScrollDirection.Down; break;
			default: return;
		}

		rootComponent.Scroll(scrollDirection);
	}
	protected void OnKeyPressEvent(object o, KeyPressEventArgs args)
	{
		Kurve.Interface.Key key;

		switch (args.Event.Key)
		{
			case Gdk.Key.Control_L: key = Kurve.Interface.Key.Control; break;
			case Gdk.Key.Shift_L: key = Kurve.Interface.Key.Shift; break;
			case Gdk.Key.Alt_L: key = Kurve.Interface.Key.Alt; break;
			case Gdk.Key.a: key = Kurve.Interface.Key.A; break;
			case Gdk.Key.r: key = Kurve.Interface.Key.R; break;
			case Gdk.Key.n: key = Kurve.Interface.Key.N; break;
			case Gdk.Key.l: key = Kurve.Interface.Key.L; break;
			case Gdk.Key.s: key = Kurve.Interface.Key.S; break;
			default: return;
		}

		rootComponent.KeyDown(key);
	}
	protected void OnKeyReleaseEvent(object o, KeyReleaseEventArgs args)
	{
		Kurve.Interface.Key key;

		switch (args.Event.Key)
		{
			case Gdk.Key.Control_L: key = Kurve.Interface.Key.Control; break;
			case Gdk.Key.Shift_L: key = Kurve.Interface.Key.Shift; break;
			case Gdk.Key.Alt_L: key = Kurve.Interface.Key.Alt; break;
			case Gdk.Key.a: key = Kurve.Interface.Key.A; break;
			case Gdk.Key.r: key = Kurve.Interface.Key.R; break;
			case Gdk.Key.n: key = Kurve.Interface.Key.N; break;
			case Gdk.Key.l: key = Kurve.Interface.Key.L; break;
			case Gdk.Key.s: key = Kurve.Interface.Key.S; break;
			// workaround for neo keyboard layout bug			
			case Gdk.Key.Caps_Lock: key = Kurve.Interface.Key.Shift; break;
			default: return;
		}

		rootComponent.KeyUp(key);
	}

	void RootComponentChanged()
	{
		GdkWindow.InvalidateRegion(GdkWindow.VisibleRegion, true);
	}
}