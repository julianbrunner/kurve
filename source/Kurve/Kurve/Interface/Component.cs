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

		public virtual void Draw(Context context) { }
		public virtual void MouseDown(Vector2Double mousePosition, MouseButton mouseButton) { }
		public virtual void MouseUp(Vector2Double mousePosition, MouseButton mouseButton) { }
		public virtual void MouseMove(Vector2Double mousePosition) { }
		public virtual void Scroll(ScrollDirection scrollDirection) { }

		protected void OnUpdate()
		{
			if (Update != null) Update();
		}
	}
}

