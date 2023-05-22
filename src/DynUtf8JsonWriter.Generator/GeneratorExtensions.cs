using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace DynUtf8JsonWriter.Generator
{
    internal static class GeneratorExtensions
    {
        public static IEnumerable<INamedTypeSymbol> AllNestedTypesAndSelf(this INamedTypeSymbol type)
        {
            yield return type;
            foreach (var typeMember in type.GetTypeMembers())
            {
                foreach (var nestedType in typeMember.AllNestedTypesAndSelf())
                {
                    yield return nestedType;
                }
            }
        }
    }
}
