using System.Text.Json;

namespace DynUtf8JsonWriter.Tests
{
    public sealed class TestDynamicJsonWriter : DynamicJsonWriter
    {
        public TestDynamicJsonWriter(Utf8JsonWriter writer) : base(writer)
        {
        }

        #region additional types: WriteValue approach

        // required override, as prescribed in DynamicJsonWriter documentation
        protected override string WriteNonNullDynamic(dynamic value) =>
            WriteValue(value);

        public string WriteValue((string str, int num) pair)
        {
            Writer.WriteStartArray();
            Writer.WriteStringValue(pair.str);
            Writer.WriteNumberValue(pair.num);
            Writer.WriteEndArray();

            return "TEST ADDITIONAL WRITEVALUE METHOD";
        }

        #endregion

        #region additional types: WriteFallback approach

        protected override string WriteFallback(object value)
        {
            Writer.WriteNumberValue(123);
            return "TEST FALLBACK";
        }

        #endregion
    }
}
