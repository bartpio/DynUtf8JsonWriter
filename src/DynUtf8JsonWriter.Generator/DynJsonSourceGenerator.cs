using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DynUtf8JsonWriter.Generator
{
    [Generator]
    public class DynJsonSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = context.Compilation;
            var itsc = new ImplementationTypeSetCache(context);
            var symbolset = itsc.ForAssembly(context.Compilation.Assembly);

            INamedTypeSymbol writer = itsc.All.Where(x => x.Name.Equals("Utf8JsonWriter", StringComparison.Ordinal)).Single();
            List<IMethodSymbol> writersyms = writer
                .GetMembers()
                .Where(x => x.Name.StartsWith("Write") && x.Name.EndsWith("Value") && !x.Name.Contains("Comment") && !x.Name.Contains("Raw"))
                .OfType<IMethodSymbol>()
                .Where(x => x.Parameters.Length == 1 && x.Parameters[0].Type.GetTypeMembers().Length == 0)
                .ToList();

            var pairs = from reg in writersyms
                        let method = reg.Name
                        let type = reg.Parameters[0].Type.ToDisplayString()
                        where !type.Contains("Json")
                        where !type.Contains("char")
                        select new { method, type };

            INamedTypeSymbol reader = itsc.All.Where(x => x.Name.Equals("Utf8JsonReader", StringComparison.Ordinal)).Single();
            List<IMethodSymbol> readersyms = reader
                .GetMembers()
                .Where(x => x.Name.StartsWith("Get") && !x.Name.Contains("Comment"))
                .OfType<IMethodSymbol>()
                .Where(x => x.Parameters.Length == 0)
                .ToList();

            var idx = 0;
            foreach (var pair in pairs)
            {
                var readerMethodName = GetReaderMethod(readersyms, pair.type);
                var src = GenWriterPartialClass(pair.method, pair.type, readerMethodName);
                context.AddSource($"DynamicJsonWriter.{idx}.g.cs", src);
                idx++;
            }
        }

        internal static string GetReaderMethod(List<IMethodSymbol> readersyms, string type)
        {
            var sym = readersyms.Where(x => x.ReturnType.ToDisplayString() == type).Single();
            return sym.Name;
        }

        internal static string GenWriterPartialClass(string method, string type, string reader)
        {
            return $@"using System;

namespace DynUtf8JsonWriter
{{
    public partial class DynamicJsonWriter
    {{
        {GenWriterMethod(method, type, reader)}
    }}
}}";
        }

        internal static string GenWriterMethod(string method, string type, string reader)
        {
            return $@"
        /// <summary>
        /// Write value to the wrapped writer, and provide the name of the type that the value was interpreted as.
        /// </summary>
        /// <param name=""value"">Value to write.</param>
        /// <returns>The string ""{CleanType(type)}"".</returns>
        [DynamicJsonWriteValue(typeof({CleanType(type)}), ""{CleanType(type)}"", ""{method}"", ""{reader}"")]
        public string WriteValue({MakeNotNulla(type)} value)
        {{
            Writer.{method}(value);
            return ""{CleanType(type)}"";
        }}";
        }

        internal static string CleanType(string typeName) =>
            MakeNotNulla(typeName).Replace("System.", "");

        internal static string MakeNotNulla(string typeName) =>
            typeName.Replace("?", "");

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this generator
        }
    }
}
