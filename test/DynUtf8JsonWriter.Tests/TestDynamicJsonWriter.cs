using System.Text.Json;

namespace DynUtf8JsonWriter.Tests
{
    internal class TestDynamicJsonWriter : DynamicJsonWriter
    {
        public TestDynamicJsonWriter(Utf8JsonWriter writer) : base(writer)
        {
        }

        protected override string WriteFallback(object value)
        {
            Writer.WriteNumberValue(123);
            return "TEST FALLBACK";
        }
    }
}
