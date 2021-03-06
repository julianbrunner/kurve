using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Cairo;
using Kurve.Interface;

namespace Kurve.Component
{
	abstract class Component
	{
		readonly Component parent;

		protected virtual IEnumerable<Component> SubComponents { get { yield break; } }

		protected Component() { }
		protected Component(Component parent)
		{
			if (parent == null) throw new ArgumentNullException("parent");

			this.parent = parent;
		}

		public virtual void Draw(Context context)
		{
			foreach (Component component in SubComponents.ToArray()) component.Draw(context);
		}
		public virtual void MouseDown(Vector2Double mousePosition, MouseButton mouseButton)
		{
			foreach (Component component in SubComponents.ToArray()) component.MouseDown(mousePosition, mouseButton);
		}
		public virtual void MouseUp(Vector2Double mousePosition, MouseButton mouseButton)
		{
			foreach (Component component in SubComponents.ToArray()) component.MouseUp(mousePosition, mouseButton);
		}
		public virtual void MouseMove(Vector2Double mousePosition) 
		{
			foreach (Component component in SubComponents.ToArray()) component.MouseMove(mousePosition);
		}
		public virtual void Scroll(ScrollDirection scrollDirection)
		{
			foreach (Component component in SubComponents.ToArray()) component.Scroll(scrollDirection);
		}
		public virtual void KeyDown(Key key)
		{
			foreach (Component component in SubComponents.ToArray()) component.KeyDown(key);
		}
		public virtual void KeyUp(Key key)
		{
			foreach (Component component in SubComponents.ToArray()) component.KeyUp(key);
		}
		public virtual void Changed()
		{
			parent.Changed();
		}
	}
}

