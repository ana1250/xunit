using System.Threading.Tasks;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// The base test method runner for xUnit.net v3 tests (with overridable context).
/// </summary>
public class XunitTestMethodRunnerBase<TContext, TTestMethod, TTestCase> :
	TestMethodRunner<TContext, TTestMethod, TTestCase>
		where TContext : XunitTestMethodRunnerBaseContext<TTestMethod, TTestCase>
		where TTestMethod : class, IXunitTestMethod
		where TTestCase : class, IXunitTestCase
{
	/// <summary>
	/// Runs the test case.
	/// </summary>
	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestCase(
		TContext ctxt,
		TTestCase testCase)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(testCase);

		return
			testCase.Run(
				ctxt.ExplicitOption,
				ctxt.MessageBus,
				ctxt.ConstructorArguments,
				ctxt.Aggregator,
				ctxt.CancellationTokenSource
			);
	}
}
