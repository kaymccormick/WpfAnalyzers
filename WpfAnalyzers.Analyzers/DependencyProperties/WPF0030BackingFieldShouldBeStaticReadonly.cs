﻿namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0030BackingFieldShouldBeStaticReadonly : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0030";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing field for a DependencyProperty should be static and readonly.",
            messageFormat: "Field '{0}' is backing field for a DependencyProperty and should be static and readonly.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Backing field for a DependencyProperty should be static and readonly.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleFieldDeclaration, SyntaxKind.FieldDeclaration);
        }

        private static void HandleFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var fieldDeclaration = context.Node as FieldDeclarationSyntax;
            if (fieldDeclaration == null || fieldDeclaration.IsMissing)
            {
                return;
            }

            var field = (IFieldSymbol)context.ContainingSymbol;
            if (field == null ||
                !(field.Type.Is(KnownSymbol.DependencyProperty) ||
                  field.Type.Is(KnownSymbol.DependencyPropertyKey)))
            {
                return;
            }

            if (DependencyProperty.TryGetRegisterInvocationRecursive(
    field,
    context.SemanticModel,
    context.CancellationToken,
    out InvocationExpressionSyntax _))
            {
                if (!field.IsReadOnly || !field.IsStatic)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptor,
                            context.Node.GetLocation(),
                            field.Name,
                            field.Type.Name));
                }
            }
        }
    }
}