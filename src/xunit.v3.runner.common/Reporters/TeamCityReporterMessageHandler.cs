using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="_IMessageSink" /> that supports <see cref="TeamCityReporter" />.
/// </summary>
public class TeamCityReporterMessageHandler : DefaultRunnerReporterMessageHandler
{
	readonly MessageMetadataCache metadataCache = new();
	readonly string? rootFlowId;

	/// <summary>
	/// Initializes a new instance of the <see cref="TeamCityReporterMessageHandler" /> class.
	/// </summary>
	/// <param name="logger">The logger used to report messages</param>
	/// <param name="rootFlowId">The root flow ID for reporting to TeamCity</param>
	public TeamCityReporterMessageHandler(
		IRunnerLogger logger,
		string? rootFlowId) :
			base(logger)
	{
		this.rootFlowId = rootFlowId;

	}

	/// <summary>
	/// Gets the current date &amp; time in UTC.
	/// </summary>
	protected virtual DateTimeOffset UtcNow =>
		DateTimeOffset.UtcNow;

	/// <summary>
	/// Handles instances of <see cref="_ErrorMessage" />.
	/// </summary>
	protected override void HandleErrorMessage(MessageHandlerArgs<_ErrorMessage> args)
	{
		base.HandleErrorMessage(args);

		var error = args.Message;

		LogError("FATAL ERROR", error);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestAssemblyCleanupFailure" />.
	/// </summary>
	protected override void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<_TestAssemblyCleanupFailure> args)
	{
		base.HandleTestAssemblyCleanupFailure(args);

		var cleanupFailure = args.Message;

		LogError($"Test Assembly Cleanup Failure ({ToEscapedAssemblyName(cleanupFailure)})", cleanupFailure, cleanupFailure.AssemblyUniqueID);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestAssemblyFinished" />.
	/// </summary>
	protected override void HandleTestAssemblyFinished(MessageHandlerArgs<_TestAssemblyFinished> args)
	{
		base.HandleTestAssemblyFinished(args);

		var assemblyFinished = args.Message;

		LogSuiteFinished(ToEscapedAssemblyName(assemblyFinished), assemblyFinished.AssemblyUniqueID);

		metadataCache.TryRemove(assemblyFinished);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestAssemblyStarting" />.
	/// </summary>
	protected override void HandleTestAssemblyStarting(MessageHandlerArgs<_TestAssemblyStarting> args)
	{
		base.HandleTestAssemblyStarting(args);

		var assemblyStarting = args.Message;

		metadataCache.Set(assemblyStarting);

		LogSuiteStarted(ToEscapedAssemblyName(assemblyStarting), assemblyStarting.AssemblyUniqueID, rootFlowId);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestCaseCleanupFailure" />.
	/// </summary>
	protected override void HandleTestCaseCleanupFailure(MessageHandlerArgs<_TestCaseCleanupFailure> args)
	{
		base.HandleTestCaseCleanupFailure(args);

		var cleanupFailure = args.Message;

		LogError($"Test Case Cleanup Failure ({ToEscapedTestCaseName(cleanupFailure)})", cleanupFailure, cleanupFailure.TestCollectionUniqueID);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestCaseFinished" />.
	/// </summary>
	protected override void HandleTestCaseFinished(MessageHandlerArgs<_TestCaseFinished> args)
	{
		base.HandleTestCaseFinished(args);

		metadataCache.TryRemove(args.Message);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestCaseStarting" />.
	/// </summary>
	protected override void HandleTestCaseStarting(MessageHandlerArgs<_TestCaseStarting> args)
	{
		base.HandleTestCaseStarting(args);

		metadataCache.Set(args.Message);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestClassCleanupFailure" />.
	/// </summary>
	protected override void HandleTestClassCleanupFailure(MessageHandlerArgs<_TestClassCleanupFailure> args)
	{
		base.HandleTestClassCleanupFailure(args);

		var cleanupFailure = args.Message;

		LogError($"Test Class Cleanup Failure ({ToEscapedTestClassName(cleanupFailure)})", cleanupFailure, cleanupFailure.TestCollectionUniqueID);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestClassFinished" />.
	/// </summary>
	protected override void HandleTestClassFinished(MessageHandlerArgs<_TestClassFinished> args)
	{
		base.HandleTestClassFinished(args);

		metadataCache.TryRemove(args.Message);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestClassStarting" />.
	/// </summary>
	protected override void HandleTestClassStarting(MessageHandlerArgs<_TestClassStarting> args)
	{
		base.HandleTestClassStarting(args);

		metadataCache.Set(args.Message);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestCollectionCleanupFailure" />.
	/// </summary>
	protected override void HandleTestCollectionCleanupFailure(MessageHandlerArgs<_TestCollectionCleanupFailure> args)
	{
		base.HandleTestCollectionCleanupFailure(args);

		var cleanupFailure = args.Message;

		LogError($"Test Collection Cleanup Failure ({ToEscapedTestCollectionName(cleanupFailure)})", cleanupFailure, cleanupFailure.TestCollectionUniqueID);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestCollectionFinished" />.
	/// </summary>
	protected override void HandleTestCollectionFinished(MessageHandlerArgs<_TestCollectionFinished> args)
	{
		base.HandleTestCollectionFinished(args);

		var testCollectionFinished = args.Message;

		LogSuiteFinished(ToEscapedTestCollectionName(testCollectionFinished), testCollectionFinished.TestCollectionUniqueID);

		metadataCache.TryRemove(testCollectionFinished);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestCollectionStarting" />.
	/// </summary>
	protected override void HandleTestCollectionStarting(MessageHandlerArgs<_TestCollectionStarting> args)
	{
		base.HandleTestCollectionStarting(args);

		var testCollectionStarting = args.Message;

		metadataCache.Set(testCollectionStarting);

		LogSuiteStarted(ToEscapedTestCollectionName(testCollectionStarting), testCollectionStarting.TestCollectionUniqueID, testCollectionStarting.AssemblyUniqueID);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestCleanupFailure" />.
	/// </summary>
	protected override void HandleTestCleanupFailure(MessageHandlerArgs<_TestCleanupFailure> args)
	{
		base.HandleTestCleanupFailure(args);

		var cleanupFailure = args.Message;

		LogError($"Test Cleanup Failure ({ToEscapedTestName(cleanupFailure)})", cleanupFailure, cleanupFailure.TestCollectionUniqueID);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestFailed" />.
	/// </summary>
	protected override void HandleTestFailed(MessageHandlerArgs<_TestFailed> args)
	{
		base.HandleTestFailed(args);

		var testFailed = args.Message;
		var details = $"{TeamCityEscape(ExceptionUtility.CombineMessages(testFailed))}|r|n{TeamCityEscape(ExceptionUtility.CombineStackTraces(testFailed))}";

		LogMessage("testFailed", $"name='{ToEscapedTestName(testFailed)}' details='{details}'", testFailed.TestCollectionUniqueID);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestFinished" />.
	/// </summary>
	protected override void HandleTestFinished(MessageHandlerArgs<_TestFinished> args)
	{
		base.HandleTestFinished(args);

		var testFinished = args.Message;

		var formattedName = ToEscapedTestName(testFinished);
		var flowId = testFinished.TestCollectionUniqueID;

		if (!string.IsNullOrWhiteSpace(testFinished.Output))
			LogMessage("testStdOut", $"name='{formattedName}' out='{TeamCityEscape(testFinished.Output)}' tc:tags='tc:parseServiceMessagesInside']", flowId);

		LogMessage("testFinished", $"name='{formattedName}' duration='{(int)(testFinished.ExecutionTime * 1000M)}'", flowId);

		metadataCache.TryRemove(testFinished);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestMethodCleanupFailure" />.
	/// </summary>
	protected override void HandleTestMethodCleanupFailure(MessageHandlerArgs<_TestMethodCleanupFailure> args)
	{
		base.HandleTestMethodCleanupFailure(args);

		var cleanupFailure = args.Message;

		LogError($"Test Method Cleanup Failure ({ToEscapedTestMethodName(cleanupFailure)})", cleanupFailure, cleanupFailure.TestCollectionUniqueID);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestMethodFinished" />.
	/// </summary>
	protected override void HandleTestMethodFinished(MessageHandlerArgs<_TestMethodFinished> args)
	{
		base.HandleTestMethodFinished(args);

		metadataCache.TryRemove(args.Message);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestMethodStarting" />.
	/// </summary>
	protected override void HandleTestMethodStarting(MessageHandlerArgs<_TestMethodStarting> args)
	{
		base.HandleTestMethodStarting(args);

		metadataCache.Set(args.Message);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestSkipped" />.
	/// </summary>
	protected override void HandleTestSkipped(MessageHandlerArgs<_TestSkipped> args)
	{
		base.HandleTestSkipped(args);

		var testSkipped = args.Message;

		LogMessage("testIgnored", $"name='{ToEscapedTestName(testSkipped)}' message='{TeamCityEscape(testSkipped.Reason)}'", testSkipped.TestCollectionUniqueID);
	}

	/// <summary>
	/// Handles instances of <see cref="_TestStarting" />.
	/// </summary>
	protected override void HandleTestStarting(MessageHandlerArgs<_TestStarting> args)
	{
		base.HandleTestStarting(args);

		var testStarting = args.Message;

		metadataCache.Set(testStarting);

		LogMessage("testStarted", $"name='{ToEscapedTestName(testStarting)}'", testStarting.TestCollectionUniqueID);
	}

	// Helpers

	void LogError(
		string messageType,
		_IErrorMetadata errorMetadata,
		string? flowId = null)
	{
		var message = $"[{messageType}] {errorMetadata.ExceptionTypes[0]}: {ExceptionUtility.CombineMessages(errorMetadata)}";
		var stackTrace = ExceptionUtility.CombineStackTraces(errorMetadata);

		LogMessage("message", $"status='ERROR' text='{TeamCityEscape(message)}' errorDetails='{TeamCityEscape(stackTrace)}'", flowId);
	}

	void LogMessage(
		string messageType,
		string? arguments = null,
		string? flowId = null) =>
			Logger.LogRaw($"##teamcity[{messageType} timestamp='{TeamCityEscape(UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff"))}+0000'{(flowId != null ? $" flowId='{TeamCityEscape(flowId)}'" : "")}{(arguments != null ? " " + arguments : "")}]");

	void LogSuiteFinished(
		string escapedName,
		string flowId)
	{
		LogMessage("testSuiteFinished", $"name='{escapedName}'", flowId);
		LogMessage("flowFinished", flowId: flowId);
	}

	void LogSuiteStarted(
		string escapedName,
		string flowId,
		string? parentFlowId = null)
	{
		LogMessage("flowStarted", parentFlowId != null ? $"parent='{TeamCityEscape(parentFlowId)}'" : null, flowId);
		LogMessage("testSuiteStarted", $"name='{escapedName}'", flowId);
	}

	string ToEscapedAssemblyName(_TestAssemblyMessage message)
	{
		var metadata = metadataCache.TryGetAssemblyMetadata(message);
		if (metadata == null)
			return "<unknown test assembly>";

		return TeamCityEscape(metadata.AssemblyPath ?? metadata.SimpleAssemblyName());
	}

	string ToEscapedTestCaseName(_TestCaseMessage message)
	{
		var metadata = metadataCache.TryGetTestCaseMetadata(message);
		if (metadata == null)
			return "<unknown test case>";

		return TeamCityEscape(metadata.TestCaseDisplayName);
	}

	string ToEscapedTestClassName(_TestClassMessage message)
	{
		var metadata = metadataCache.TryGetClassMetadata(message);
		if (metadata == null)
			return "<unknown test class>";

		return TeamCityEscape(metadata.TestClass);
	}

	string ToEscapedTestCollectionName(_TestCollectionMessage message)
	{
		var metadata = metadataCache.TryGetCollectionMetadata(message);
		if (metadata == null)
			return "<unknown test collection>";


		return TeamCityEscape($"{metadata.TestCollectionDisplayName} ({message.TestCollectionUniqueID})");
	}

	string ToEscapedTestMethodName(_TestMethodMessage message) =>
		TeamCityEscape(ToTestMethodName(message) ?? "<unknown test method>");

	string ToEscapedTestName(_TestMessage message)
	{
		var testMetadata = metadataCache.TryGetTestMetadata(message);
		if (testMetadata == null)
			return "<unknown test>";

		// TODO: Is there a way to get just the component pieces of the display name?
		// That way we could construct the method name and arguments separately.
		return TeamCityEscape(testMetadata.TestDisplayName);
	}

	string? ToTestClassName(_TestClassMessage message) =>
		metadataCache.TryGetClassMetadata(message)?.TestClass;

	string? ToTestMethodName(_TestMethodMessage message)
	{
		var testMethodMetadata = metadataCache.TryGetMethodMetadata(message);
		if (testMethodMetadata == null)
			return null;

		var testClassName = ToTestClassName(message);
		if (testClassName == null)
			return testMethodMetadata.TestMethod;

		return $"{testClassName}.{testMethodMetadata.TestMethod}";
	}

	[return: NotNullIfNotNull("value")]
	static string? TeamCityEscape(string? value)
	{
		if (value == null)
			return null;

		var sb = new StringBuilder(value.Length);

		for (var i = 0; i < value.Length; i++)
		{
			var ch = value[i];

			switch (ch)
			{
				case '\\':
					sb.Append("|0x005C");
					break;
				case '|':
					sb.Append("||");
					break;
				case '\'':
					sb.Append("|'");
					break;
				case '\n':
					sb.Append("|n");
					break;
				case '\r':
					sb.Append("|r");
					break;
				case '[':
					sb.Append("|[");
					break;
				case ']':
					sb.Append("|]");
					break;
				default:
					if (ch < '\x007f')
						sb.Append(ch);
					else
					{
						sb.Append("|0x");
						sb.Append(((int)ch).ToString("x4"));
					}
					break;
			}
		}

		return sb.ToString();
	}
}
