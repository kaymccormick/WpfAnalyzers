﻿namespace WpfAnalyzers.Test.WPF0014SetValueMustUseRegisteredTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetCurrentValue(BarProperty, 1);")]
        public void DependencyProperty(string setValueCall)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public void Meh()
        {
            this.SetValue(BarProperty, 1);
        }
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, 1);", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetCurrentValue(BarProperty, 1);")]
        public void DependencyPropertyPartial(string setValueCall)
        {
            var part1 = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";

            var part2 = @"
namespace RoslynSandbox
{
    public partial class FooControl
    {
        public void Meh()
        {
            this.SetValue(BarProperty, 1);
        }
    }
}";
            part2 = part2.AssertReplace("this.SetValue(BarProperty, 1);", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(part1, part2);
        }

        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetValue(BarProperty, null);")]
        [TestCase("this.SetCurrentValue(BarProperty, 1);")]
        [TestCase("this.SetCurrentValue(BarProperty, null);")]
        public void DependencyPropertyOfTypeNullableInt(string setValueCall)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int?),
            typeof(FooControl),
            new PropertyMetadata(default(int?)));

        public int? Bar
        {
            get { return (int?)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public void Meh()
        {
            this.SetValue(BarProperty, 1);
        }
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, 1);", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("fooControl.SetValue(FooControl.BarProperty, 1);")]
        [TestCase("fooControl.SetValue(FooControl.BarProperty, null);")]
        [TestCase("fooControl.SetCurrentValue(FooControl.BarProperty, 1);")]
        [TestCase("fooControl.SetCurrentValue(FooControl.BarProperty, null);")]
        public void DependencyPropertyOfTypeNullableFromOutside(string setValueCall)
        {
            var fooControlCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int?),
            typeof(FooControl),
            new PropertyMetadata(default(int?)));

        public int? Bar
        {
            get { return (int?)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";

            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class Foo
    {
        public void Meh()
        {
            var fooControl = new FooControl();
            fooControl.SetValue(BarProperty, 1);
        }
    }
}";
            testCode = testCode.AssertReplace("fooControl.SetValue(BarProperty, 1);", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(fooControlCode, testCode);
        }

        [TestCase("this.SetValue(BarProperty, meh);")]
        [TestCase("this.SetCurrentValue(BarProperty, meh);")]
        public void DependencyPropertyOfTypeNullableIntParameter(string setValueCall)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int?),
            typeof(FooControl),
            new PropertyMetadata(default(int?)));

        public int? Bar
        {
            get { return (int?)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        public void Meh(int meh)
        {
            this.SetValue(BarProperty, meh);
        }
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, meh);", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetValue(BarProperty, null);")]
        [TestCase("this.SetCurrentValue(BarProperty, 1);")]
        [TestCase("this.SetCurrentValue(BarProperty, null);")]
        public void DependencyPropertyOfTypeObject(string setValueCall)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(object),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public object Bar
        {
            get { return (object)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public void Meh()
        {
            this.SetValue(BarProperty, 1);
        }
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, 1);", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("this.SetValue(BarProperty, new Foo());")]
        [TestCase("this.SetCurrentValue(BarProperty, new Foo());")]
        public void DependencyPropertyOfInterfaceType(string setValueCall)
        {
            var interfaceCode = @"
namespace RoslynSandbox
{
    public interface IFoo
    {
    }
}";

            var fooCode = @"
namespace RoslynSandbox
{
    public class Foo : IFoo
    {
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(IFoo),
            typeof(FooControl),
            new PropertyMetadata(default(IFoo)));

        public IFoo Bar
        {
            get { return (IFoo)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public void Meh()
        {
            this.SetValue(BarProperty, new Foo());
        }
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, new Foo());", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(interfaceCode, fooCode, testCode);
        }

        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetCurrentValue(BarProperty, 1);")]
        public void DependencyPropertyGeneric(string setValueCall)
        {
            var fooControlGeneric = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl<T> : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(T),
            typeof(FooControl),
            new PropertyMetadata(default(T)));

        public T Bar
        {
            get { return (T)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";

            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : FooControl<int>
    {
        public void Meh()
        {
            this.SetValue(BarProperty, 1);
        }
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, 1);", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(fooControlGeneric, testCode);
        }

        [TestCase("this.SetValue(BarProperty, (object)1);")]
        [TestCase("this.SetCurrentValue(BarProperty, (object)1);")]
        public void DependencyPropertySetValueOfTypeObject(string setValueCall)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public void Meh()
        {
            this.SetValue(BarProperty, (object)1);
        }
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, (object)1);", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("this.SetValue(BarProperty, value);")]
        [TestCase("this.SetCurrentValue(BarProperty, value);")]
        public void DependencyPropertySetValueOfTypeObject2(string setValueCall)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public void Meh()
        {
            var value = this.GetValue(BarProperty);
            this.SetValue(BarProperty, value);
        }
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, value);", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("this.SetValue(BarProperty, true);")]
        [TestCase("this.SetCurrentValue(BarProperty, true);")]
        public void DependencyPropertyAddOwner(string setValueCall)
        {
            var fooCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(bool),
            typeof(Foo),
            new PropertyMetadata(default(bool)));

        public static void SetBar(FrameworkElement element, bool value)
        {
            element.SetValue(BarProperty, value);
        }

        public static bool GetBar(FrameworkElement element)
        {
            return (bool)element.GetValue(BarProperty);
        }
    }
}";

            var fooControlPart1 = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(
            typeof(FooControl),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnVolumeChanged,
                OnVolumeCoerce));

        public bool Bar
        {
            get { return (bool)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        private static object OnVolumeCoerce(DependencyObject d, object basevalue)
        {
            return basevalue;
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";

            var fooControlPart2 = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl
    {
        public FooControl()
        {
            this.SetValue(BarProperty, false);
        }
    }
}";
            fooControlPart2 = fooControlPart2.AssertReplace("this.SetValue(BarProperty, false);", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(fooCode, fooControlPart1, fooControlPart2);
        }

        [TestCase("this.SetValue(VolumeProperty, 1.0);")]
        [TestCase("this.SetCurrentValue(VolumeProperty, 1.0);")]
        public void DependencyPropertyAddOwnerMediaElementVolume(string setValueCall)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class MediaElementWrapper : Control
    {
        public static readonly DependencyProperty VolumeProperty = MediaElement.VolumeProperty.AddOwner(
            typeof(MediaElementWrapper),
            new FrameworkPropertyMetadata(
                MediaElement.VolumeProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnVolumeChanged,
                OnVolumeCoerce));

        public MediaElementWrapper()
        {
            this.SetValue(VolumeProperty, 2.0);
        }

        public double Volume
        {
            get { return (double)this.GetValue(VolumeProperty); }
            set { this.SetValue(VolumeProperty, value); }
        }

        private static object OnVolumeCoerce(DependencyObject d, object basevalue)
        {
            return basevalue;
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(VolumeProperty, 2.0);", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("textBox.SetValue(TextBox.TextProperty, \"abc\");")]
        [TestCase("textBox.SetCurrentValue(TextBox.TextProperty, \"abc\");")]
        public void TextBoxText(string setValueCall)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public static class Foo
    {
        public static void Bar()
        {
            var textBox = new TextBox();
            textBox.SetValue(TextBox.TextProperty, ""abc"");
        }
    }
}";
            testCode = testCode.AssertReplace("textBox.SetValue(TextBox.TextProperty, \"abc\");", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [Test]
        public void SetCurrentValueInLambda()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        public void Meh()
        {
            this.Loaded += (sender, args) =>
            {
                this.SetCurrentValue(BarProperty, 1);
            };
        }
    }
}";
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("SetValue")]
        [TestCase("SetCurrentValue")]
        public void IgnoredPropertyAsParameter(string setValueCall)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        public void Meh(DependencyProperty property, object value)
        {
            this.SetCurrentValue(property, value);
        }
    }
}";
            testCode = testCode.AssertReplace("SetCurrentValue", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("SetValue")]
        [TestCase("SetCurrentValue")]
        public void IgnoresFreezable(string setValueCall)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(Brush),
            typeof(FooControl),
            new PropertyMetadata(default(Brush)));

        public Brush Brush
        {
            get { return (Brush)this.GetValue(BrushProperty); }
            set { this.SetValue(BrushProperty, value); }
        }

        public void UpdateBrush(Brush brush)
        {
            this.SetCurrentValue(BrushProperty, brush?.GetAsFrozen());
        }
    }
}";
            testCode = testCode.AssertReplace("SetCurrentValue", setValueCall);
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [Test]
        public void PropertyKeyInOtherClass()
        {
            var linkCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    public class Link : ButtonBase
    {
    }
}";

            var modernLinksCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class ModernLinks : ItemsControl
    {
        /// <summary>
        /// Identifies the SelectedSource dependency property.
        /// </summary>
        internal static readonly DependencyPropertyKey SelectedLinkPropertyKey = DependencyProperty.RegisterReadOnly(
            ""SelectedLink"",
            typeof(Link),
            typeof(ModernLinks),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedLinkProperty = SelectedLinkPropertyKey.DependencyProperty;
    }
}";

            var linkGroupCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    public class LinkGroup : ButtonBase
    {
        public static readonly DependencyProperty SelectedLinkProperty = ModernLinks.SelectedLinkProperty.AddOwner(typeof(LinkGroup));

        public Link SelectedLink
        {
            get { return (Link)this.GetValue(SelectedLinkProperty); }
            protected set { this.SetValue(ModernLinks.SelectedLinkPropertyKey, value); }
        }
    }
}";
            AnalyzerAssert.Valid<WPF0014SetValueMustUseRegisteredType>(linkCode, modernLinksCode, linkGroupCode);
        }
    }
}