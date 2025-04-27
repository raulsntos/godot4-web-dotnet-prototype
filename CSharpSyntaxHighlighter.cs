using System;
using System.Collections.Generic;
using Godot;
using Microsoft.CodeAnalysis.CSharp;

namespace BrowserTest;

internal sealed partial class CSharpSyntaxHighlighter : CodeHighlighter
{
	public CSharpSyntaxHighlighter()
	{
		KeywordColors = [];

		SymbolColor = Color.FromHtml("#D4D4D4");
		NumberColor = Color.FromHtml("#B5CEA8");
		FunctionColor = Color.FromHtml("#DCDCAA");
		MemberVariableColor = Color.FromHtml("#9CDCFE");

		ColorRegions["//"] = Color.FromHtml("#6A9955");
		AddColorRegion("\"", "\"", Color.FromHtml("#CE9178"));
		AddColorRegion("/*", "*/", Color.FromHtml("#6A9955"));
		AddColorRegion("//", "", Color.FromHtml("#6A9955"));

		foreach (SyntaxKind syntaxKind in SyntaxFacts.GetKeywordKinds())
		{
			string keyword = SyntaxFacts.GetText(syntaxKind);
			KeywordColors[keyword] = Color.FromHtml("#569CD6");
		}

		AddTypesColor(typeof(object).Assembly.GetTypes());

		void AddTypesColor(IEnumerable<Type> types)
		{
			foreach (Type type in types)
			{
				AddTypeColor(type);
			}
		}

		void AddTypeColor(Type type)
		{
			if (KeywordColors.ContainsKey(type.Name))
			{
				return;
			}

			KeywordColors[type.Name] = GetTypeColor(type);

			ReadOnlySpan<char> fullNamespace = type.Namespace;
			if (fullNamespace.IsWhiteSpace())
			{
				return;
			}

			foreach (System.Range range in fullNamespace.Split('.'))
			{
				string name = fullNamespace[range].ToString();
				if (!KeywordColors.ContainsKey(name))
				{
					KeywordColors[name] = Color.FromHtml("#4EC9B0");
				}
			}
		}

		static Color GetTypeColor(Type type)
		{
			return type switch
			{
				{ } when type.IsInterface => Color.FromHtml("#B8D7A3"),
				{ } when type.IsEnum => Color.FromHtml("#B8D7A3"),
				{ } when type.IsValueType => Color.FromHtml("#86C691"),
				{ } when type.IsClass => Color.FromHtml("#4EC9B0"),
				_ => Color.FromHtml("#4EC9B0"),
			};
		}

#if false
		if (scr_lang != nullptr) {
			/* Comments */
			const Color comment_color = EDITOR_GET("text_editor/theme/highlighting/comment_color");
			List<String> comments;
			scr_lang->get_comment_delimiters(&comments);
			for (const String &comment : comments) {
				String beg = comment.get_slice(" ", 0);
				String end = comment.get_slice_count(" ") > 1 ? comment.get_slice(" ", 1) : String();
				highlighter->add_color_region(beg, end, comment_color, end.is_empty());
			}

			/* Doc comments */
			const Color doc_comment_color = EDITOR_GET("text_editor/theme/highlighting/doc_comment_color");
			List<String> doc_comments;
			scr_lang->get_doc_comment_delimiters(&doc_comments);
			for (const String &doc_comment : doc_comments) {
				String beg = doc_comment.get_slice(" ", 0);
				String end = doc_comment.get_slice_count(" ") > 1 ? doc_comment.get_slice(" ", 1) : String();
				highlighter->add_color_region(beg, end, doc_comment_color, end.is_empty());
			}

			/* Strings */
			const Color string_color = EDITOR_GET("text_editor/theme/highlighting/string_color");
			List<String> strings;
			scr_lang->get_string_delimiters(&strings);
			for (const String &string : strings) {
				String beg = string.get_slice(" ", 0);
				String end = string.get_slice_count(" ") > 1 ? string.get_slice(" ", 1) : String();
				highlighter->add_color_region(beg, end, string_color, end.is_empty());
			}
		}
#endif
	}
}
