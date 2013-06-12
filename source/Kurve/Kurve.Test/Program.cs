using System;
using Krach.Basics;
using System.Linq;
using System.Collections.Generic;
using Krach.Extensions;
using System.Linq.Expressions;
using Wrappers.Casadi;
using Kurve.Curves;
using System.Xml.Linq;

namespace Kurve.Test
{
	static class Program
	{
		static void Main(string[] parameters)
        {
			BasicSpecification basicSpecification = new BasicSpecification(1, 2, new PolynomialCurveTemplate(4), Enumerables.Create<CurveSpecification>());

			Optimizer optimizer = Optimizer.Create(basicSpecification);

			foreach (double x in Scalars.GetIntermediateValues(4, 5, 10))
			{
				double curveLength = x;
				int segmentCount = 10;
				CurveTemplate segmentTemplate = new PolynomialCurveTemplate(10);
				IEnumerable<CurveSpecification> curveSpecifications = Enumerables.Create<CurveSpecification>
				(
					new PointCurveSpecification(0.0, new Vector2Double(-1.0,  0.0)),
					new PointCurveSpecification(0.5, new Vector2Double( 0.0, -1.0)),
					new PointCurveSpecification(1.0, new Vector2Double(+1.0,  0.0))
				);

				basicSpecification = new BasicSpecification(curveLength, segmentCount, segmentTemplate, curveSpecifications);

				optimizer = optimizer.Modify(basicSpecification);

				Console.WriteLine(optimizer.GetCurves().Count());
			}
		}
	}
}
