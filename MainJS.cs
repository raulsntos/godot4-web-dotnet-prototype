// Building for the browser requires a Main entry-point before .NET 10.
// See https://github.com/dotnet/runtime/issues/110620.
#if !NET10_0_OR_GREATER
using System.Diagnostics;

internal class MainJS
{
	public static void Main()
	{
		// Godot won't call this method, we have our own entry-point.
		throw new UnreachableException();
	}
}
#endif
