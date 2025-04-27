using Godot;

namespace BrowserTest;

internal sealed partial class OutputLabel : RichTextLabel
{
	public void Write(string text)
	{
		AppendText(text);
	}

	public void WriteLine(string text)
	{
		Write($"{text}\n");
	}

	public void WriteInfo(string text)
	{
		WriteLine($"[color=#999999]{text}[/color]");
	}

	public void WriteWarning(string text)
	{
		WriteLine($"[color=#F1C232]{text}[/color]");
	}

	public void WriteError(string text)
	{
		WriteLine($"[color=#B00020]{text}[/color]");
	}
}
