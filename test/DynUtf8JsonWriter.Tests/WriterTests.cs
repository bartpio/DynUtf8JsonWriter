using System.Text;
using System.Text.Json;

namespace DynUtf8JsonWriter.Tests
{
    public class Tests
    {
        [Test]
        public void VerifyAllJsonWriteMethodsRunWithoutCrashing_UsingReflection()
        {
            var methods = typeof(DynamicJsonWriter).GetMethods().Where(x => x.Name == nameof(DynamicJsonWriter.WriteValue))
                .OrderBy(meth => meth.GetParameters().Single().ParameterType.Name)
                .ToList();

            Assert.That(methods.Count, Is.EqualTo(13));

            using var ms = new MemoryStream();
            using (var writer = new Utf8JsonWriter(ms))
            {
                DynamicJsonWriter dynwriter = new SimpleDynamicJsonWriter(writer);
                writer.WriteStartArray();

                var dyntypes = new List<string>();
                foreach (var meth in methods)
                {
                    var arg = meth.GetParameters().Single();
                    var type = arg.ParameterType;
                    var defaultValue = type.GetDefaultValue();
                    dynamic? dyn = defaultValue;

                    string? dyntype = dynwriter.WriteDynamic(dyn);
                    TestContext.WriteLine($"{type.Name} {dyntype}");
                    dyntypes.Add(dyntype ?? "(null)");
                }

                var dyntypesjoined = string.Join(";", dyntypes);
                Assert.That(dyntypesjoined, Is.EqualTo("bool;DateTime;DateTimeOffset;decimal;double;Guid;int;long;(null);float;(null);uint;ulong"));

                writer.WriteEndArray();
            }

            var longjson = Encoding.UTF8.GetString(ms.ToArray());
            Assert.That(longjson, Is.EqualTo(@"[false,""0001-01-01T00:00:00"",""0001-01-01T00:00:00+00:00"",0,0,""00000000-0000-0000-0000-000000000000"",0,0,null,0,null,0,0]"));
        }

        [Test]
        public void VerifyAdditionalTypes_UsingWriteValue()
        {
            using var ms = new MemoryStream();
            using (var writer = new Utf8JsonWriter(ms))
            {
                var dynwriter = new TestDynamicJsonWriter(writer);
                writer.WriteStartArray();

                dynamic? dyn = null;
                Assert.That(dynwriter.WriteDynamic(dyn), Is.Null);
                dyn = "somestring";
                Assert.That(dynwriter.WriteDynamic(dyn), Is.EqualTo("string"));
                dyn = ("some", 12345678);
                Assert.That(dynwriter.WriteDynamic(dyn), Is.EqualTo("TEST ADDITIONAL WRITEVALUE METHOD"));

                writer.WriteEndArray();
            }

            var longjson = Encoding.UTF8.GetString(ms.ToArray());
            Assert.That(longjson, Is.EqualTo(@"[null,""somestring"",[""some"",12345678]]"));
        }

        [Test]
        public void VerifyAdditionalTypes_UsingWriteFallback()
        {
            using var ms = new MemoryStream();
            using (var writer = new Utf8JsonWriter(ms))
            {
                var dynwriter = new TestDynamicJsonWriter(writer);
                writer.WriteStartArray();

                dynamic? dyn = null;
                Assert.That(dynwriter.WriteDynamic(dyn), Is.Null);
                dyn = "somestring";
                Assert.That(dynwriter.WriteDynamic(dyn), Is.EqualTo("string"));
                dyn = ("something", "needing", "fallback");
                Assert.That(dynwriter.WriteDynamic(dyn), Is.EqualTo("TEST FALLBACK"));

                writer.WriteEndArray();
            }

            var longjson = Encoding.UTF8.GetString(ms.ToArray());
            Assert.That(longjson, Is.EqualTo(@"[null,""somestring"",123]"));
        }
    }
}