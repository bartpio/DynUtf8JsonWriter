using System.Reflection;
using System.Text.Json;

namespace DynUtf8JsonWriter
{
    /// <summary>
    /// Decorates a <see cref="DynamicJsonWriter"/> WriteValue overload with metadata.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DynamicJsonWriteValueAttribute : Attribute
    {
        /// <summary>
        /// The type handled by this WriteValue overload.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The name of the type handled by this overload, as it would generally be written in C# source code.
        /// </summary>
        /// <remarks>
        /// For example, "decimal" or "string".
        /// </remarks>
        public string TypeName { get; }

        /// <summary>
        /// Name of the <see cref="Utf8JsonWriter"/> "Write...Value(value)" method used to write values handled by this overload.
        /// </summary>
        public string WriterMethodName { get; }

        /// <summary>
        /// Name of the <see cref="Utf8JsonReader"/> "Get...()" method that could be used to read values written by this overload.
        /// </summary>
        public string ReaderMethodName { get; }

        /// <summary>
        /// Construct an instance of <see cref="DynamicJsonWriteValueAttribute" />.
        /// </summary>
        /// <param name="type"><inheritdoc cref="Type"/></param>
        /// <param name="typeName"><inheritdoc cref="TypeName"/></param>
        /// <param name="writerMethodName"><inheritdoc cref="WriterMethodName"/></param>
        /// <param name="readerMethodName"><inheritdoc cref="ReaderMethodName"/></param>
        public DynamicJsonWriteValueAttribute(Type type, string typeName, string writerMethodName, string readerMethodName)
        {
            Type = type;
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            WriterMethodName = writerMethodName ?? throw new ArgumentNullException(nameof(writerMethodName));
            ReaderMethodName = readerMethodName ?? throw new ArgumentNullException(nameof(readerMethodName));
        }

        /// <summary>
        /// Get the <see cref="Utf8JsonReader"/> Get method corresponding to the type handled by this overload.
        /// </summary>
        public MethodInfo GetReaderMethod() =>
            typeof(Utf8JsonReader).GetMethod(ReaderMethodName) ??
            throw new InvalidOperationException($"could not get {nameof(Utf8JsonReader)} method");
    }
}