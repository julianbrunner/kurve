using System;
using System.Linq;
using System.Collections.Generic;
using Krach.Basics;
using Krach.Extensions;
using Krach.Graphics;
using Kurve.Curves;
using Kurve.Curves.Optimization;
using Krach.Maps.Abstract;
using System.Diagnostics;
using Krach.Maps.Scalar;
using Krach.Maps;
using Kurve.Interface;
using Cairo;
using System.IO;

namespace Kurve.Component
{
	class BackgroundComponent : Component
	{
		readonly ImageSurface background;

		protected override IEnumerable<Component> SubComponents
		{
			get
			{
				yield break;
			}
		}

		public BackgroundComponent(Component parent, string fileName) : base(parent)
		{
			if (fileName == null) throw new ArgumentNullException("fileName");
			if (!File.Exists(fileName)) throw new ArgumentException("parameter 'fileName', points to a file that does not exist");

			this.background = new ImageSurface(fileName);
		}

		public override void Draw(Context context)
		{
			Drawing.DrawSurface(context, background);

			base.Draw(context);
		}
	}
}

