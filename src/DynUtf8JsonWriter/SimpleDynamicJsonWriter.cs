using System.Text.Json;

namespace DynUtf8JsonWriter
{
    /// <summary>
    /// Wrap a <see cref="Utf8JsonWriter "/> and provide the <see cref="DynamicJsonWriter.WriteDynamic" /> method to write dynamically typed values.
    /// Does not facilitate any additional type support.
    /// </summary>
    public sealed class SimpleDynamicJsonWriter : DynamicJsonWriter
    {
        /// <summary>
        /// Construct an instance of a dynamic JSON writer wrapper.
        /// </summary>
        /// <param name="writer">The wrapped writer.</param>
        /// <exception cref="ArgumentNullException">Thrown if wrapped writer not supplied.</exception>
        public SimpleDynamicJsonWriter(Utf8JsonWriter writer) : base(writer)
        {
        }
    }
}
