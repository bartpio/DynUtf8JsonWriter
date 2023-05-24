using System.ComponentModel;
using System.Text.Json;

namespace DynUtf8JsonWriter
{
    /// <summary>
    /// Wrap a <see cref="Utf8JsonWriter "/> and provide the <see cref="WriteDynamic" /> method to write dynamically typed values.
    /// </summary>
    /// <remarks>
    /// Optionally, subclasses may provide additional type support, using one of the following approaches:
    ///  - Provide additional WriteValue signatures, and override <see cref="WriteNonNullDynamic"/> as prescribed.
    ///  - Override <see cref="WriteFallback(object)"/>.
    /// </remarks>
    public abstract partial class DynamicJsonWriter
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

            return WriteNonNullDynamic(value);
        }

        /// <summary>
        /// Write non-null dynamically typed value to the wrapped writer, and provide the name of the type that the value was interpreted as.
        /// If a subclass implements its own additional WriteValue signatures, in order for them to work, it needs to override this method as follows:
        /// protected override string WriteNonNullDynamic(dynamic value) => WriteValue(value);
        /// This override is the same as the base implementation, but is requried for dynamic dispatch to work as expected.
        /// </summary>
        /// <param name="value">Dynamically typed value that's never null.</param>
        /// <returns>The name of the type that the value was interpreted as.</returns>
        protected virtual string WriteNonNullDynamic(dynamic value) =>
            WriteValue(value);

        /// <summary>
        /// Calls <see cref="WriteFallback" />.
        /// To write dynamically typed values, don't call this method. Use <see cref="WriteDynamic(dynamic?)" /> instead.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string WriteValue(object value) =>
            WriteFallback(value);

        /// <summary>
        /// Subclasses may implement this method to handle writing values whose type is otherwise unsupported.
        /// Instead of implementing this, subclasses may introduce additional WriteValue signatures, and override <see cref="WriteNonNullDynamic"/>
        /// as prescribed.
        /// </summary>
        /// <param name="value">Dynamically typed value, of a type that <see cref="WriteDynamic(dynamic)" /> couldn't handle.</param>
        /// <returns>Implementation should return the name of the type that the value was interpreted as.</returns>
        /// <exception cref="NotImplementedException">Always thrown from the base <see cref="DynamicJsonWriter" /> implementation.</exception>
        protected virtual string WriteFallback(object value) =>
            throw new NotImplementedException($"{nameof(DynamicJsonWriter)}.{nameof(WriteFallback)} not implemented to handle type {value?.GetType().Name}");

        #region Auxiliary Types

        /// <summary>
        /// Write <see cref="DBNull"/> to the wrapped writer as a JSON null token.
        /// </summary>
        /// <param name="value">DBNull value.</param>
        /// <returns>The name of the type that the value was interpreted as, which in this case is "DBNull".</returns>
        public string WriteValue(DBNull value)
        {
            Writer.WriteNullValue();
            return nameof(DBNull);
        }

        /// <summary>
        /// Write <see cref="DateOnly"/> to the wrapped writer as an ISO 8601 calendar date string (in yyyy-MM-dd format).
        /// </summary>
        /// <param name="value">DateOnly value.</param>
        /// <returns>The name of the type that the value was interpreted as, which in this case is "DateOnly".</returns>
        public string WriteValue(DateOnly value)
        {
            Writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
            return nameof(DateOnly);
        }

        /// <summary>
        /// Write byte array to the wrapped writer as a Base64 encoded string.
        /// </summary>
        /// <param name="value">Byte array value.</param>
        /// <returns>The name of the type that the value was interpreted as, which in this case is "byte[]".</returns>
        public string WriteValue(byte[] value)
        {
            Writer.WriteBase64StringValue(value);
            return "byte[]";
        }

        #endregion
        }
}