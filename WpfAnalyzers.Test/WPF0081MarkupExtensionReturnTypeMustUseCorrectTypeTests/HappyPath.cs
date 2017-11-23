﻿namespace WpfAnalyzers.Test.WPF0081MarkupExtensionReturnTypeMustUseCorrectTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly WPF0081MarkupExtensionReturnTypeMustUseCorrectType Analyzer = new WPF0081MarkupExtensionReturnTypeMustUseCorrectType();

        [Test]
        public void WhenHasAttribute()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(FooExtension))]
    public class FooExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}