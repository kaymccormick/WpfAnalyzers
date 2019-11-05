namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GetTemplateChildAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0130UseTemplatePartAttribute,
            Descriptors.WPF0131TemplatePartType);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is InvocationExpressionSyntax { ArgumentList: { Arguments: { Count: 1 } arguments } } invocation &&
                invocation.TryGetMethodName(out var name) &&
                name == "GetTemplateChild" &&
                arguments.TrySingle(out var argument) &&
                argument.Expression is { } expression &&
                context.SemanticModel.TryGetConstantValue<string>(expression, context.CancellationToken, out string? partName) &&
                context.ContainingSymbol is IMethodSymbol { Name: "OnApplyTemplate", IsOverride: true, Parameters: { Length: 0 } } containingMethod)
            {
                if (TryFindAttribute(containingMethod.ContainingType, partName, out var attribute))
                {
                    if (TryGetCastType(invocation, out var cast, out var castTypeSyntax))
                    {
                        if (TryFindTemplatePartType(attribute, out var partType))
                        {
                            if (partType != null &&
                                context.SemanticModel.TryGetType(castTypeSyntax, context.CancellationToken, out var castType) &&
                                !IsValidCast(partType, castType, cast, context.Compilation))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0131TemplatePartType, invocation.GetLocation()));
                            }
                        }
                        else
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0131TemplatePartType, invocation.GetLocation()));
                        }
                    }
                }
                else
                {
                    var partNameArg = argument.Expression is LiteralExpressionSyntax
                        ? $"\"{partName}\""
                        : argument.Expression.ToString();

                    if (TryGetCastType(invocation, out _, out var partType))
                    {
                        var attributeText = $"[System.Windows.TemplatePartAttribute(Name = {partNameArg}, Type = typeof({partType}))]";
                        context.ReportDiagnostic(Diagnostic.Create(
                                                     Descriptors.WPF0130UseTemplatePartAttribute,
                                                     invocation.GetLocation(),
                                                     ImmutableDictionary<string, string>.Empty.Add(nameof(AttributeListSyntax), attributeText),
                                                     attributeText));
                    }
                    else
                    {
                        var attributeText = $"[System.Windows.TemplatePartAttribute(Name = {partNameArg})]";
                        context.ReportDiagnostic(Diagnostic.Create(
                                                     Descriptors.WPF0130UseTemplatePartAttribute,
                                                     invocation.GetLocation(),
                                                     ImmutableDictionary<string, string>.Empty.Add(nameof(AttributeListSyntax), attributeText),
                                                     attributeText));
                    }
                }
            }
        }

        private static bool TryFindAttribute(INamedTypeSymbol type, string part, [NotNullWhen(true)] out AttributeData? attribute)
        {
            attribute = null;
            if (type == null ||
                type == KnownSymbols.Object)
            {
                return false;
            }

            foreach (var candidate in type.GetAttributes())
            {
                if (candidate.AttributeClass == KnownSymbols.TemplatePartAttribute &&
                    candidate.NamedArguments.TryFirst(x => IsMatch(x), out _))
                {
                    attribute = candidate;
                    return true;
                }
            }

            return TryFindAttribute(type.BaseType, part, out attribute);

            bool IsMatch(KeyValuePair<string, TypedConstant> a)
            {
                return a.Key == "Name" &&
                       a.Value.Value is string candidate &&
                       candidate == part;
            }
        }

        private static bool TryFindTemplatePartType(AttributeData attribute, [NotNullWhen(true)] out INamedTypeSymbol? type)
        {
            type = null;
            if (attribute.NamedArguments.TrySingle(x => x.Key == "Type", out var arg))
            {
                type = arg.Value.Value as INamedTypeSymbol;
            }

            return type != null;
        }

        private static bool TryGetCastType(InvocationExpressionSyntax invocation, [NotNullWhen(true)] out ExpressionSyntax? cast, [NotNullWhen(true)] out SyntaxNode? type)
        {
            switch (invocation.Parent)
            {
                case BinaryExpressionSyntax binary when
                    binary.IsKind(SyntaxKind.AsExpression):
                    {
                        cast = binary;
                        type = binary.Right as TypeSyntax;
                        return true;
                    }

                case CastExpressionSyntax castExpression:
                    cast = castExpression;
                    type = castExpression.Type;
                    return true;
                case IsPatternExpressionSyntax isPattern when
                    isPattern.Pattern is DeclarationPatternSyntax declarationPattern &&
                    !declarationPattern.Type.IsVar:
                    {
                        cast = isPattern;
                        type = declarationPattern.Type;
                        return true;
                    }

                default:
                    type = null;
                    cast = null;
                    return false;
            }
        }

        private static bool IsValidCast(INamedTypeSymbol partType, ITypeSymbol castType, ExpressionSyntax cast, Compilation compilation)
        {
            if (partType.IsAssignableTo(castType, compilation))
            {
                return true;
            }

            if (!(cast is CastExpressionSyntax))
            {
                return castType.IsAssignableTo(partType, compilation);
            }

            return false;
        }
    }
}
