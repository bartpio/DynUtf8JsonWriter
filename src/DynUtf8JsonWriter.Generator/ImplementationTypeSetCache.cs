using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DynUtf8JsonWriter.Generator
{
    /// <summary>
    /// Cache type information from a particular assembly and its references.
    /// </remarks>
    /// <summary>
    /// thanks to: <seealso href="https://stackoverflow.com/a/74163439" />
    /// </remarks>
    internal class ImplementationTypeSetCache
    {
        private readonly GeneratorExecutionContext _context;
        private readonly Lazy<IImmutableSet<INamedTypeSymbol>> _all;
        private IImmutableDictionary<IAssemblySymbol, IImmutableSet<INamedTypeSymbol>> _assemblyCache =
            ImmutableDictionary<IAssemblySymbol, IImmutableSet<INamedTypeSymbol>>.Empty;

        public ImplementationTypeSetCache(
            GeneratorExecutionContext context
            )
        {
            _context = context;
            _all = new Lazy<IImmutableSet<INamedTypeSymbol>>(
                () => context
                    .Compilation
                    .SourceModule
                    .ReferencedAssemblySymbols
                    .Prepend(_context.Compilation.Assembly)
                    .SelectMany(ForAssembly)
                    .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default));
        }

        public IImmutableSet<INamedTypeSymbol> All => _all.Value;

        public IImmutableSet<INamedTypeSymbol> ForAssembly(IAssemblySymbol assembly)
        {
            if (_assemblyCache.TryGetValue(assembly, out var set)) return set;

            var freshSet = GetImplementationsFrom(assembly);
            _assemblyCache = _assemblyCache.Add(assembly, freshSet);
            return freshSet;
        }

        private IImmutableSet<INamedTypeSymbol> GetImplementationsFrom(IAssemblySymbol assemblySymbol)
        {
            return GetAllNamespaces(assemblySymbol.GlobalNamespace)
                .SelectMany(ns => ns.GetTypeMembers())
                .SelectMany(t => t.AllNestedTypesAndSelf())
                .Where(nts => nts is
                {
                    IsAbstract: false,
                    IsStatic: false,
                    IsImplicitClass: false,
                    IsScriptClass: false,
                    TypeKind: TypeKind.Class or TypeKind.Struct or TypeKind.Structure,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                })
                .Where(nts =>
                    !nts.Name.StartsWith("<")
                    && true)
                .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        }

        private static IEnumerable<INamespaceSymbol> GetAllNamespaces(INamespaceSymbol root)
        {
            yield return root;
            foreach (var child in root.GetNamespaceMembers())
                foreach (var next in GetAllNamespaces(child))
                    yield return next;
        }
    }
}
