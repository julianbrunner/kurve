using System;
using Kurve.Interface;
using Cairo;
using Krach.Basics;
using Krach.Graphics;
using Kurve.Curves;

namespace Kurve.Component
{
	delegate void LengthInsertion(double length);

	class CurveLengthComponent : LengthControlComponent
	{
		public event LengthInsertion InsertLength;

		public CurveLengthComponent(Component parent) : base(parent) { }

		public override void OnInsertLength(double length)
		{
			if (InsertLength != null) InsertLength(length);
		}
	}
}
