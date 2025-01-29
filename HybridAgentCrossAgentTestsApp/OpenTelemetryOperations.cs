using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridAgentCrossAgentTestsApp
{
	public class OpenTelemetryOperations
	{
		public static ActivitySource TestAppActivitySource = new("TestApp activity source");

		public static void DoWorkInSpan(ActivityKind activityKind, Action work)
		{
			using var activity = TestAppActivitySource.StartActivity("DoWorkInSpan", activityKind);

			work();
		}

		public static void AddAttributeToCurrentSpan(string key, object value, Action work)
		{
			Activity.Current?.AddTag(key, value);
			work();
		}

		public static void AssertNotValidSpan()
		{
			if (Activity.Current != null)
			{
				throw new Exception("Expected no active span, but found one.");
			}
		}

		public static object GetCurrentTraceId()
		{
			return Activity.Current!.TraceId.ToString();
		}

		public static object GetCurrentSpanId()
		{
			return Activity.Current!.SpanId.ToString();
		}

		public static void RecordExceptionOnSpan(string errorMessage, Action work)
		{
			Activity.Current?.AddException(new Exception(errorMessage));

			work();
		}
	}
}
