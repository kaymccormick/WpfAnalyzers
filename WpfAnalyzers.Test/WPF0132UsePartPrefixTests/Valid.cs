namespace WpfAnalyzers.Test.WPF0132UsePartPrefixTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTemplateChildAnalyzer();

        [Test]
        public static void WhenPartPrefix()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = ""PART_Bar"", Type = typeof(Border))]
    public class FooControl : Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)this.GetTemplateChild(""PART_Bar"");
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
