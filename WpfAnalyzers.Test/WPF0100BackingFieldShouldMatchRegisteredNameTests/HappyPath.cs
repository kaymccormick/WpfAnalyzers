﻿namespace WpfAnalyzers.Test.WPF0100BackingFieldShouldMatchRegisteredNameTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly RoutedEventAnalyzer Analyzer = new RoutedEventAnalyzer();

        [TestCase("\"ValueChanged\"")]
        [TestCase("nameof(ValueChanged)")]
        [TestCase("nameof(FooControl.ValueChanged)")]
        public void DependencyPropertyRegisterBackingField(string nameof)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the ValueChanged event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler ValueChanged
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(ValueChangedEvent, value); }
        }
    }
}";
            testCode = testCode.AssertReplace("nameof(ValueChanged)", nameof);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("\"ValueChanged\"")]
        [TestCase("nameof(ValueChanged)")]
        [TestCase("nameof(FooControl.ValueChanged)")]
        public void DependencyPropertyRegisterBackingFieldExpressionBodies(string nameof)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the ValueChanged event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler ValueChanged
        {
            add => this.AddHandler(ValueChangedEvent, value);
            remove => this.RemoveHandler(ValueChangedEvent, value);
        }
    }
}";
            testCode = testCode.AssertReplace("nameof(ValueChanged)", nameof);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}