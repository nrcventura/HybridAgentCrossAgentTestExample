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

		internal static object GetCurrentTraceId()
		{
			// The .net agent public API does not provide a way to get the current trace id.
			return "placeholder";
		}

		internal static object GetCurrentSpanId()
		{
			// The .net agent public API does not provide a way to get the current span id.
			return "placeholder";
		}
	}
}
