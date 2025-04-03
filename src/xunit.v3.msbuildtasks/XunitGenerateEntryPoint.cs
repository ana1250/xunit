#pragma warning disable CS8618 // Properties are set by MSBuild

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit.Internal;

namespace Xunit.v3.MSBuildTasks;

// Adapted from https://github.com/microsoft/testfx/blob/a047150c9fcedb16d29d5da35a459352d80c7818/src/Platform/Microsoft.Testing.Platform.MSBuild/Tasks/TestingPlatformEntryPointTask.cs

public sealed class XunitGenerateEntryPoint : Task
{
	const string LanguageCS = "C#";
	const string LanguageFS = "F#";
	const string LanguageVB = "VB";

	readonly HashSet<string> languages = new(StringComparer.OrdinalIgnoreCase) { LanguageCS, LanguageFS, LanguageVB };
	string? rootNamespace;

	[Required]
	public ITaskItem Language { get; set; }

	public string RootNamespace
	{
		get => rootNamespace ?? string.Empty;
		set => rootNamespace = NamespaceHelpers.ToSafeNamespace(value ?? string.Empty);
	}

	[Required]
	public ITaskItem SourcePath { get; set; }

	public string? UseMicrosoftTestingPlatformRunner { get; set; }

	public override bool Execute()
	{
		Log.LogMessage(MessageImportance.Normal, $"SourcePath: '{SourcePath.ItemSpec}'");
		Log.LogMessage(MessageImportance.Normal, $"Language: '{Language.ItemSpec}'");
		Log.LogMessage(MessageImportance.Normal, $"RootNamespace: '{RootNamespace}'");

#pragma warning disable CA1806 // Do not ignore method results
		bool.TryParse(UseMicrosoftTestingPlatformRunner, out var useMicrosoftTestingPlatformRunner);
#pragma warning restore CA1806 // Do not ignore method results

		if (!languages.Contains(Language.ItemSpec))
			Log.LogError($"Language '{Language.ItemSpec}' is not supported.");
		else
			GenerateEntryPoint(Language.ItemSpec, RootNamespace, useMicrosoftTestingPlatformRunner, SourcePath, Log);

		return !Log.HasLoggedErrors;
	}

	static void GenerateEntryPoint(
		string language,
		string rootNamespace,
		bool useMicrosoftTestingPlatformRunner,
		ITaskItem sourcePath,
		TaskLoggingHelper log)
	{
		var @namespace =
			rootNamespace.Length == 0
				? "AutoGenerated"
				: rootNamespace + ".AutoGenerated";

		var entryPointSource = (language.ToUpperInvariant(), useMicrosoftTestingPlatformRunner) switch
		{
			(LanguageCS, false) => XunitEntryPointGenerator.GetXunitEntryPointCSharp(@namespace),
			(LanguageCS, true) => XunitEntryPointGenerator.GetMTPEntryPointCSharp(@namespace),
			(LanguageFS, false) => XunitEntryPointGenerator.GetXunitEntryPointFSharp(@namespace),
			(LanguageFS, true) => XunitEntryPointGenerator.GetMTPEntryPointFSharp(@namespace),
			(LanguageVB, false) => XunitEntryPointGenerator.GetXunitEntryPointVB(),
			(LanguageVB, true) => XunitEntryPointGenerator.GetMTPEntryPointVB(),
			_ => throw new InvalidOperationException($"Unsupported language '{language}'"),
		};

		log.LogMessage(MessageImportance.Normal, $"Entrypoint source:{Environment.NewLine}'{entryPointSource}'");

		File.WriteAllText(sourcePath.ItemSpec, entryPointSource);
	}
}
