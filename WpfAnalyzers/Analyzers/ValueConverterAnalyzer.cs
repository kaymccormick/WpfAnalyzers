namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class ValueConverterAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0070ConverterDoesNotHaveDefaultField,
            Descriptors.WPF0071ConverterDoesNotHaveAttribute,
            Descriptors.WPF0072ValueConversionMustUseCorrectTypes,
            Descriptors.WPF0073ConverterDoesNotHaveAttributeUnknownTypes,
            Descriptors.WPF0074DefaultMemberOfWrongType);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.ClassDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.ContainingSymbol is INamedTypeSymbol type &&
                type.IsAssignableToEither(KnownSymbols.IValueConverter, KnownSymbols.IMultiValueConverter, context.Compilation) &&
                context.Node is ClassDeclarationSyntax classDeclaration &&
                !type.IsAbstract &&
                type.DeclaredAccessibility != Accessibility.Private &&
                type.DeclaredAccessibility != Accessibility.Protected)
            {
                if (!type.IsAssignableTo(KnownSymbols.MarkupExtension, context.Compilation))
                {
                    if (ValueConverter.TryGetDefaultFieldsOrProperties(type, context.Compilation, out var defaults))
                    {
                        foreach (var fieldOrProperty in defaults)
                        {
                            if (fieldOrProperty.TryGetAssignedValue(context.CancellationToken, out var assignedValue) &&
                                context.SemanticModel.TryGetType(assignedValue, context.CancellationToken, out var assignedType) &&
                                !Equals(assignedType, type))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0074DefaultMemberOfWrongType, assignedValue.GetLocation()));
                            }
                        }
                    }
                    else if (!Virtual.HasVirtualOrAbstractOrProtectedMembers(type) &&
                             !type.Constructors.TryFirst(x => x.Parameters.Length > 0, out _) &&
                             !Mutable.HasMutableInstanceMembers(type))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0070ConverterDoesNotHaveDefaultField, classDeclaration.Identifier.GetLocation()));
                    }
                }

                if (type.IsAssignableTo(KnownSymbols.IValueConverter, context.Compilation))
                {
                    if (Attribute.TryFind(classDeclaration, KnownSymbols.ValueConversionAttribute, context.SemanticModel, context.CancellationToken, out var attribute))
                    {
                        if (ValueConverter.TryGetConversionTypes(classDeclaration, context.SemanticModel, context.CancellationToken, out var sourceType, out var targetType))
                        {
                            if (sourceType != null &&
                                sourceType != QualifiedType.System.Object &&
                                Attribute.TryFindArgument(attribute, 0, "sourceType", out var arg) &&
                                arg.Expression is TypeOfExpressionSyntax sourceTypeOf &&
                                TypeOf.TryGetType(sourceTypeOf, type, context.SemanticModel, context.CancellationToken, out var argType) &&
                                !Equals(argType, sourceType))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0072ValueConversionMustUseCorrectTypes, arg.GetLocation(), sourceType));
                            }

                            if (Attribute.TryFindArgument(attribute, 1, "targetType", out arg) &&
                                arg.Expression is TypeOfExpressionSyntax targetTypeOf &&
                                TypeOf.TryGetType(targetTypeOf, type, context.SemanticModel, context.CancellationToken, out argType) &&
                                !Equals(argType, targetType))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0072ValueConversionMustUseCorrectTypes, arg.GetLocation(), targetType));
                            }
                        }
                    }
                    else
                    {
                        if (ValueConverter.TryGetConversionTypes(classDeclaration, context.SemanticModel, context.CancellationToken, out _, out _))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0071ConverterDoesNotHaveAttribute, classDeclaration.Identifier.GetLocation()));
                        }
                        else
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0073ConverterDoesNotHaveAttributeUnknownTypes, classDeclaration.Identifier.GetLocation()));
                        }
                    }
                }
            }
        }
    }
}
