using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using Basic.Reference.Assemblies;
using Godot;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace BrowserTest;

internal sealed partial class Main : Control
{
	private Button _toggleButton;

	private SplitContainer _splitContainer;

	private int _lastVerticalSplitOffset;
	private int _lastHorizontalSplitOffset;

	private CodeEdit _codeEdit;

	private OutputLabel _outputLabel;

	private OutputWriter _outputWriter;

	private Node3D _cube;

	public override void _Ready()
	{
		GetTree().Root.ContentScaleFactor = DisplayServer.ScreenGetScale();

		_cube = GetNode<Node3D>("%Cube");

		_splitContainer = GetNode<SplitContainer>("%SplitContainer");
		_codeEdit = GetNode<CodeEdit>("%CodeEdit");
		_outputLabel = GetNode<OutputLabel>("%OutputLabel");

		_toggleButton = GetNode<Button>("%ToggleLayoutButton");
		_toggleButton.Pressed += ToggleLayout;

		GetNode<Button>("%ResetButton").Pressed += ResetCode;
		GetNode<Button>("%RunButton").Pressed += RunCode;

		string godotVersion = Engine.GetVersionInfo()["string"].AsString();
		string dotnetVersion = RuntimeInformation.FrameworkDescription.TrimPrefix(".NET ");
		var poweredByLabel = GetNode<RichTextLabel>("%PoweredByLabel");
		poweredByLabel.Text = $"[font_size=12]Powered by [color=#9CDCFE][url=https://godotengine.org]Godot[/url][/color] {godotVersion} and [color=#9CDCFE][url=https://get.dot.net].NET[/url][/color] {dotnetVersion}[/font_size]";
		poweredByLabel.MetaClicked += GoToUrl;

		var sourceCodeLabel = GetNode<RichTextLabel>("%SourceCodeLabel");
		sourceCodeLabel.Text = $"[font_size=12][color=#9CDCFE][url=https://godotengine.org/article/live-from-godotcon-boston-web-dotnet-prototype/]Blog Announcement[/url][/color] • [color=#9CDCFE][url=https://github.com/raulsntos/godot4-web-dotnet-prototype]Source Code[/url][/color][/font_size]";
		sourceCodeLabel.MetaClicked += GoToUrl;

		Vector2 viewportSize = GetViewportRect().Size;
		(float viewportWidth, float viewportHeight) = (viewportSize.X, viewportSize.Y);

		_lastHorizontalSplitOffset = (int)(viewportWidth / 2);
		_lastVerticalSplitOffset = (int)(viewportHeight / 2);

		_splitContainer.SplitOffset = _lastHorizontalSplitOffset;

		_outputWriter = new OutputWriter(_outputLabel);

		_codeEdit.SyntaxHighlighter = new CSharpSyntaxHighlighter();

		if (viewportHeight > viewportWidth)
		{
			// The device screen is on portrait mode. Toggle the layout to vertical mode
			// to make better use of the space by default.
			ToggleLayout();
		}

		// Run the code immediately to preload the necessary assemblies
		// and avoid a long pause the first time the user runs the code.
		RunCode();
	}

	public override void _Process(double delta)
	{
		_cube.RotationDegrees += new Vector3(100, 100, 0) * (float)delta;
	}

	private static void GoToUrl(Variant variant)
	{
		string url = variant.AsString();

		if (!OperatingSystem.IsBrowser())
		{
			OS.ShellOpen(url);
			return;
		}

		JavaScriptBridge.Eval($"window.open('{url}', '_blank')");
	}

	private void ToggleLayout()
	{
		bool isVertical = _splitContainer.Vertical;
		_splitContainer.Vertical = !isVertical;

		if (isVertical)
		{
			_toggleButton.Icon = GD.Load<Texture2D>("res://Icons/VSplitContainer.svg");
			_toggleButton.TooltipText = "Switch to vertical layout";

			_lastVerticalSplitOffset = _splitContainer.SplitOffset;
			_splitContainer.SplitOffset = _lastHorizontalSplitOffset;
		}
		else
		{
			_toggleButton.Icon = GD.Load<Texture2D>("res://Icons/HSplitContainer.svg");
			_toggleButton.TooltipText = "Switch to horizontal layout";

			_lastHorizontalSplitOffset = _splitContainer.SplitOffset;
			_splitContainer.SplitOffset = _lastVerticalSplitOffset;
		}
	}

	private void ResetCode()
	{
		_codeEdit.Text = """
			using System;

			Console.WriteLine("🌄");
			""";

		_outputLabel.Clear();
		_outputLabel.WriteLine("🌄");
	}

	private void RunCode()
	{
		_outputLabel.Clear();

		string csharpCode = _codeEdit.Text;

		CSharpParseOptions parseOptions = new CSharpParseOptions
		(
			languageVersion: LanguageVersion.Latest,
			documentationMode: DocumentationMode.None,
			kind: SourceCodeKind.Script
		);

		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(csharpCode, parseOptions);

		CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions
		(
			outputKind: OutputKind.ConsoleApplication,
			optimizationLevel: OptimizationLevel.Debug,
			allowUnsafe: false
		);

		CSharpCompilation compilation = CSharpCompilation.Create
		(
			assemblyName: "DynamicCode",
			options: compilationOptions,
			syntaxTrees: [syntaxTree],
			references: Net90.References.All
		);

		ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();
		foreach (Diagnostic diagnostic in diagnostics)
		{
			switch (diagnostic.Severity)
			{
				case DiagnosticSeverity.Error:
					_outputLabel.WriteError(diagnostic.ToString());
					break;

				case DiagnosticSeverity.Warning:
					_outputLabel.WriteWarning(diagnostic.ToString());
					break;

				case DiagnosticSeverity.Info:
					_outputLabel.WriteInfo(diagnostic.ToString());
					break;
			}
		}

		using MemoryStream memoryStream = new MemoryStream();
		EmitResult result = compilation.Emit(memoryStream);
		if (!result.Success)
		{
			// Diagnostics already written to the output label.
			return;
		}

		TextWriter originalConsoleOut = Console.Out;
		TextWriter originalConsoleError = Console.Error;

		var alc = new AssemblyLoadContext("DynamicCodeContext", isCollectible: true);
		try
		{
			memoryStream.Position = 0;
			Assembly assembly = alc.LoadFromStream(memoryStream);

			MethodInfo entryPoint = assembly.EntryPoint;
			if (entryPoint is null)
			{
				_outputLabel.WriteError("Failed to find entry-point in the C# code. Make sure there the code is top-level or there's a suitable Main method.");
				return;
			}

			Console.SetOut(_outputWriter);
			Console.SetError(_outputWriter);

			object instance = assembly.CreateInstance(entryPoint.DeclaringType.FullName);

			var entryPointParameters = entryPoint.GetParameters();

			if (entryPointParameters.Length is not (0 or 1))
			{
				_outputLabel.WriteError("Found entry-point in the C# code is not suitable. Make sure the code is top-level or the Main method has 0 or 1 parameters.");
				return;
			}

			object[] parameters = entryPointParameters.Length == 0 ? null : [Array.Empty<string>()];
			entryPoint.Invoke(instance, parameters);
		}
		catch (TargetInvocationException e)
		{
			_outputLabel.WriteError($"Exception thrown: {e.InnerException}");
		}
		catch (Exception e)
		{
			_outputLabel.WriteError($"Failed to run C# code. Exception thrown: {e}");
		}
		finally
		{
			Console.SetOut(originalConsoleOut);
			Console.SetError(originalConsoleError);
			alc.Unload();
		}
	}

	private sealed class OutputWriter : TextWriter
	{
		private readonly OutputLabel _outputLabel;

		public override Encoding Encoding => Encoding.Default;

		public OutputWriter(OutputLabel outputLabel)
		{
			_outputLabel = outputLabel;
		}

		public override void Write(string value)
		{
			_outputLabel.Write(value);
		}
	}
}
