﻿namespace WpfAnalyzers.Helpers
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DocComment
    {
        internal static bool TryMatch(this XmlElementSyntax e, [NotNullWhen(true)] out XmlTextSyntax? prefix, [NotNullWhen(true)] out XmlElementSyntax? cref, [NotNullWhen(true)] out XmlTextSyntax? suffix)
        {
            prefix = default;
            cref = default;
            suffix = default;
            return e is { Content: { Count: 3 } content } &&
                   Element(0, out prefix) &&
                   Element(1, out cref) &&
                   Element(2, out suffix);

            bool Element<T>(int index, out T? result)
                 where T : class
            {
                return (result = content[index] as T) is { };
            }
        }
    }
}