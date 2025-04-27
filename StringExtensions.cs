namespace BrowserTest;

internal static class StringExtensions
{
	public static string TrimPrefix(this string text, string prefix)
	{
		if (text.StartsWith(prefix))
		{
			return text.Substring(prefix.Length);
		}

		return text;
	}
}
