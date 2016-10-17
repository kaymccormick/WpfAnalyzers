﻿namespace WpfAnalyzers.DependencyProperties
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyMetaData
    {
        internal static bool TryGetConstructor(
            ObjectCreationExpressionSyntax objectCreation,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            out IMethodSymbol constructor)
        {
            constructor = null;
            var createdType = semanticModel.GetTypeInfo(objectCreation, cancellationToken)
                                           .Type;
            if (createdType == null)
            {
                return false;
            }

            if (!createdType.Is(KnownSymbol.PropertyMetadata) ||
                objectCreation?.ArgumentList.Arguments.FirstOrDefault() == null)
            {
                return false;
            }

            if (createdType.ContainingNamespace != KnownSymbol.PropertyMetadata.Namespace)
            {
                // don't think there is a way to handle custom subclassed.
                // should not be common
                return false;
            }

            constructor = semanticModel.GetSymbolInfo(objectCreation, cancellationToken)
                                       .Symbol as IMethodSymbol;
            return constructor != null;
        }

        internal static bool TryGetDefaultValue(
            ObjectCreationExpressionSyntax objectCreation,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            out ArgumentSyntax defaultValueArg)
        {
            defaultValueArg = null;
            if (objectCreation == null ||
                objectCreation.ArgumentList == null ||
                objectCreation.ArgumentList.Arguments.Count == 0)
            {
                return false;
            }

            IMethodSymbol constructor;
            if (!TryGetConstructor(objectCreation, semanticModel, cancellationToken, out constructor))
            {
                return false;
            }

            IParameterSymbol parameter;
            if (constructor == null ||
                !constructor.Parameters.TryGetFirst(out parameter) ||
                !parameter.Type.IsSameType(semanticModel.Compilation.GetTypeByMetadataName("System.Object")))
            {
                return false;
            }

            return objectCreation.ArgumentList.Arguments.TryGetFirst(out defaultValueArg);
        }

        internal static bool TryGetDependencyProperty(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol dependencyProperty)
        {
            dependencyProperty = null;
            var declarator = objectCreation.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
            if (declarator == null)
            {
                return false;
            }

            dependencyProperty = semanticModel.SemanticModelFor(declarator)
                                              .GetDeclaredSymbol(declarator, cancellationToken) as IFieldSymbol;
            return dependencyProperty != null;
        }
    }
}