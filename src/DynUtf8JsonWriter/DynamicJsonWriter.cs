using System.Text.Json;

namespace DynUtf8JsonWriter
{
    /// <summary>
    /// Wrap a <see cref="Utf8JsonWriter "/> and provide the <see cref="WriteDynamic" /> method to write dynamically typed values.
    /// </summary>
    public partial class DynamicJsonWriter
    {
        /// <summary>
        /// The wrapped writer.
        /// </summary>
        public Utf8JsonWriter Writer { get; }

        /// <summary>
        /// Construct an instance of a dynamic JSON writer wrapper.
        /// </summary>
        /// <param name="writer">The wrapped writer.</param>
        /// <exception cref="ArgumentNullException">Thrown if wrapped writer not supplied.</exception>
        public DynamicJsonWriter(Utf8JsonWriter writer)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        /// <summary>
        /// Write dynamically typed value to the wrapped writer, and provide the name of the type that the value was interpreted as.
        /// </summary>
        /// <param name="value">Dynamically typed value.</param>
        /// <returns>The name of the type that the value was interpreted as, or null if the value provided was null.</returns>
        public string? WriteDynamic(dynamic? value)
        {
            if (value is null)
            {
                Writer.WriteNullValue();
                return null;
            }

            return WriteValue(value);
        }

        /// <inheritdoc cref="WriteDynamic(dynamic)" />
        public string WriteValue(object value) =>
            WriteFallback(value);

        /// <summary>
        /// Subclasses may implement this method to handle writing values whose type is otherwise unsupported.
        /// </summary>
        /// <param name="value">Dynamically typed value, of a type that <see cref="WriteDynamic(dynamic)" /> couldn't handle.</param>
        /// <returns>Implementation should return the name of the type that the value was interpreted as.</returns>
        /// <exception cref="NotImplementedException">Always thrown from the base <see cref="DynamicJsonWriter" /> implementation.</exception>
        protected virtual string WriteFallback(object value) =>
            throw new NotImplementedException($"{nameof(DynamicJsonWriter)}.{nameof(WriteFallback)} not implemented to handle type {value?.GetType().Name}");
    }
}