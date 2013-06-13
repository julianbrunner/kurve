using System;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using System.Linq.Expressions;
using Wrappers.Casadi;
using Kurve.Curves;
using System.Xml.Linq;
using Kurve.Curves.Optimization;

namespace Kurve.Test
{
	static class Program
	{
		static void Main(string[] parameters)
        {
			Optimizer optimizer = new Optimizer();

			double curveLength = 4;
			int segmentCount = 10;
			CurveTemplate segmentTemplate = new PolynomialCurveTemplate(10);
			IEnumerable<CurveSpecification> curveSpecifications = Enumerables.Create<CurveSpecification>
			(
				new PointCurveSpecification(0.0, new Vector2Double(-1.0,  0.0)),
				new PointCurveSpecification(0.5, new Vector2Double( 0.0, -1.0)),
				new PointCurveSpecification(1.0, new Vector2Double(+1.0,  0.0))
			);
			BasicSpecification basicSpecification = new BasicSpecification(curveLength, segmentCount, segmentTemplate, curveSpecifications);

			Specification specification = new Specification(basicSpecification);

			foreach (double x in Scalars.GetIntermediateValues(4, 5, 10))
			{
				specification = optimizer.Normalize
				(
					new Specification
					(
						new BasicSpecification(x, specification.BasicSpecification.SegmentCount, specification.BasicSpecification.SegmentTemplate, specification.BasicSpecification.CurveSpecifications),
						specification.Position
					)
				);

				Console.WriteLine(optimizer.GetCurves(specification));
			}
		}
	}
}
