using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Wrappers.Casadi;
using Cairo;

namespace Kurve.Interface
{
	abstract class Component
	{
		public event Action Update;

		public abstract void Draw(Context context);
		public abstract void MouseDown(Vector2Double mousePosition, MouseButton mouseButton);
		public abstract void MouseUp(Vector2Double mousePosition, MouseButton mouseButton);
		public abstract void MouseMove(Vector2Double mousePosition);
		public abstract void Scroll(ScrollDirection scrollDirection);

		protected void OnUpdate()
		{
			if (Update != null) Update();
		}
	}
}

