namespace DynUtf8JsonWriter.Tests
{
    internal static class TestExtensions
    {
        public static object? GetDefaultValue(this Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }
    }
}
