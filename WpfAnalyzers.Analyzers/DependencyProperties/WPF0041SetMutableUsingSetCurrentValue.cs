﻿namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0041SetMutableUsingSetCurrentValue : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0041";
        private const string Title = "Set mutable dependency properties using SetCurrentValue.";
        private const string MessageFormat = "Use SetCurrentValue({0}, {1})";
        private const string Description = "Prefer setting mutable dependency properties using SetCurrentValue.";
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
                                                                      DiagnosticId,
                                                                      Title,
                                                                      MessageFormat,
                                                                      AnalyzerCategory.DependencyProperties,
                                                                      DiagnosticSeverity.Error,
                                                                      AnalyzerConstants.EnabledByDefault,
                                                                      Description,
                                                                      HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleAssignment, SyntaxKind.SimpleAssignmentExpression);
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.InvocationExpression);
        }

        private static void HandleAssignment(SyntaxNodeAnalysisContext context)
        {
            var assignment = context.Node as AssignmentExpressionSyntax;
            if (assignment == null || context.SemanticModel == null)
            {
                return;
            }

            var property = context.SemanticModel.GetSymbolInfo(assignment.Left, context.CancellationToken).Symbol as IPropertySymbol;
            if (!property.IsPotentialDependencyPropertyAccessor())
            {
                return;
            }

            AccessorDeclarationSyntax setter;
            if (property.TryGetSetterSyntax(out setter))
            {
                FieldDeclarationSyntax dependencyProperty;
                if (setter.TryGetDependencyPropertyFromSetter(out dependencyProperty))
                {
                    if (dependencyProperty.IsDependencyPropertyKeyField())
                    {
                        return;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, assignment.GetLocation(), dependencyProperty.Name(), assignment.Right));
                }

                return;
            }

            IFieldSymbol field;
            if (property.TryGetMutableDependencyPropertyField(out field))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, assignment.GetLocation(), field.Name, assignment.Right));
            }
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null || context.SemanticModel == null)
            {
                return;
            }

            ArgumentSyntax property;
            ArgumentSyntax value;
            if (!invocation.TryGetSetValueArguments(context.SemanticModel, context.CancellationToken, out property, out value))
            {
                return;
            }

            var clrProperty = invocation.Ancestors()
                                        .OfType<PropertyDeclarationSyntax>()
                                        .FirstOrDefault();
            if (clrProperty.IsDependencyPropertyAccessor())
            {
                return;
            }

            var clrMethod = invocation.Ancestors()
                                      .OfType<MethodDeclarationSyntax>()
                                      .FirstOrDefault();
            if (clrMethod.IsAttachedSetAccessor(context.SemanticModel, context.CancellationToken))
            {
                return;
            }

            var typeInfo = context.SemanticModel.GetTypeInfo(property.Expression, context.CancellationToken);
            if (typeInfo.Type?.Name == Names.DependencyProperty)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation(), property, value));
            }
        }
    }
}