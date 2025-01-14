﻿using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static INamedTypeSymbol GetArgumentType(this INamedTypeSymbol type, INamedTypeSymbol baseGenericDefinition, Compilation compilation)
        {
            INamedTypeSymbol resolvedBaseGenericType = null;
            for (var currentType = type.BaseType; currentType != null; currentType = currentType.BaseType)
            {
                if (currentType.OriginalDefinition.Equals(baseGenericDefinition))
                {
                    resolvedBaseGenericType = currentType;
                    break;
                }
            }

            if (resolvedBaseGenericType == null)
                return null;
            return resolvedBaseGenericType.TypeArguments[0] as INamedTypeSymbol;
        }
    }
}
