using NewRelic.Api.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridAgentCrossAgentTestsApp
{
	public class NewRelicAgentOperations
	{
		[Transaction]
		public static void DoWorkInTransaction(string transactionName, Action work)
		{
			NewRelic.Api.Agent.NewRelic.SetTransactionName("Custom", transactionName);
			work();
		}

		[Trace]
		public static void DoWorkInSegment(string segmentName, Action work)
		{
			NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction.CurrentSpan.SetName(segmentName);
			work();
		}

		public static void AssertNotValidTransaction()
		{
			// The .net agent public API does not provide a way to check if a transaction is active or valid.
			// This is a no-op for now.
		}

		public static object GetCurrentTraceId()
		{
			return NewRelic.Api.Agent.NewRelic.GetAgent().TraceMetadata.TraceId;
		}

		public static object GetCurrentSpanId()
		{
			return NewRelic.Api.Agent.NewRelic.GetAgent().TraceMetadata.SpanId;
		}

		public static bool GetCurrentIsSampledFlag()
		{
			return NewRelic.Api.Agent.NewRelic.GetAgent().TraceMetadata.IsSampled;
		}

		public static void InjectHeaders(Action work)
		{
			var externalCall = SimulatedOperations.GetCurrentExternalCall()!;

			var transactionApi = NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction;

			transactionApi.InsertDistributedTraceHeaders(externalCall, (ExternalCallLibrary call, string headerName, string headerValue) => call.Headers[headerName] = headerValue);

			work();
		}
	}
}
