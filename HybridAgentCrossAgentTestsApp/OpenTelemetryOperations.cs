﻿using System;
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

		public static void DoWorkInSpanWithRemoteParent(ActivityKind activityKind, Action work)
		{
			var parentContext = new ActivityContext(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded, isRemote: true);
			using var activity = TestAppActivitySource.StartActivity("DoWorkInSpan", activityKind, parentContext);

			work();
		}

		public static void DoWorkInSpanWithInboundContext(ActivityKind activityKind, InboundContext inboundContext, Action work)
		{
			var parentContext = GetActivityContextFromInboundContext(inboundContext);
			using var activity = TestAppActivitySource.StartActivity("DoWorkInSpan", activityKind, parentContext);

			work();
		}

		private static ActivityContext GetActivityContextFromInboundContext(InboundContext inboundContext)
		{
			var otelPropagator = DistributedContextPropagator.Current;
			otelPropagator.ExtractTraceIdAndState(inboundContext, (object? carrier, string fieldName, out string? fieldValue, out IEnumerable<string>? fieldValues) =>
			{
				if (carrier == null)
				{
					fieldValue = null;
					fieldValues = null;
					return;
				}

				InboundContext typedCarrier = (InboundContext)carrier;
				switch (fieldName.ToLower())
				{
					case "traceparent":
						fieldValue = typedCarrier.GetTraceParentHeader();
						break;
					case "tracestate":
						fieldValue = typedCarrier.GetTraceStateHeader();
						break;
					default:
						fieldValue = null;
						break;
				}

				fieldValues = null;
			}, out string? traceParent, out string? traceState);

			if (!ActivityContext.TryParse(traceParent, traceState, isRemote: true, out ActivityContext context))
			{
				throw new Exception("Failed to parse traceparent and tracestate from inbound context.");
			}
			return context;
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

		public static void InjectHeaders(Action work)
		{
			var externalCall = SimulatedOperations.GetCurrentExternalCall()!;

			var otelPropagator = DistributedContextPropagator.Current;

			otelPropagator.Inject(Activity.Current, externalCall, (object? call, string headerName, string headerValue) => ((ExternalCallLibrary)call!).Headers[headerName] = headerValue);

			work();
		}
	}
}
