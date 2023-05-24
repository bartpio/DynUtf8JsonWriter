# DynUtf8JsonWriter

## Overview

Serializes dynamically typed values to JSON using `Utf8JsonWriter`.

## Motivation

Explores use of source generation (hidden to library consumers), and dynamic method dispatch using the `dynamic` type.

## Basic Usage

Instantiate `SimpleDynamicJsonWriter` (an implementation of `DynamicJsonWriter` built in to the library), providing a `Utf8JsonWriter` to the constructor. Use the DynamicJsonWriter alongside the original `Utf8JsonWriter` to produce JSON output as desired. To write a dynamically typed value, use `DynamicJsonWriter.WriteDynamic`. Calls to `DynamicJsonWriter.WriteDynamic` will be dispatched to the applicable `DynamicJsonWriter.WriteValue` overload, and then to the applicable `Utf8JsonWriter` write-value method.

## Dynamically written value's type name

`DynamicJsonWriter.WriteDynamic` returns the name of the type that the value was interpreted as, or null if the value provided was null. The name of the type is provided as it would generally be written in C# source code (ex. `decimal` or `string`), not using reflection methods such as `Type.Name` or `Type.FullName`.

## Supported Types

### Native Utf8JsonWriter Types

The library provides support for the following types by calling the applicable "Write...Value" method on Utf8JsonWriter:

 - `string`
 - `bool`
 - `DateTime`
 - `DateTimeOffset`
 - `decimal`
 - `double`
 - `Guid`
 - `int`
 - `long`
 - `float`
 - `uint`
 - `ulong`

### Auxiliary Types

`DBNull` is serialized to JSON `null`.

`DateOnly` is serialized to an ISO 8601 calendar date string, in `yyyy-MM-dd` format.

## Adding support for Additional Types

To add support for additional types, derive a subclass from `DynamicJsonWriter`, and use one of the following approaches.

### Additional types using WriteValue methods

Provide additional `WriteValue` overloads in the subclass. For example:

```csharp
public string WriteValue((string str, int num) pair)
{
    Writer.WriteStartArray();
    Writer.WriteStringValue(pair.str);
    Writer.WriteNumberValue(pair.num);
    Writer.WriteEndArray();

    return "(string str, int num)";
}
```


 Also, override the `WriteNonNullDynamic` method as follows:

```csharp
protected override string WriteNonNullDynamic(dynamic value) =>
    WriteValue(value);
```

This override is the same as the base implementation, but is requried for dynamic dispatch to work as expected.

### Additional types using WriteFallback

Subclasses may implement `WriteFallback` to handle writing values whose type is otherwise unsupported. The default implementation always throws `NotImplementedException`.

## WriteValue method attributes

`DynamicJsonWriter`'s `WriteValue` methods are decorated with `DynamicJsonWriteValueAttribute`. For each overload, an attribute provides:
 - The `Type` that's handled.
 - The name of the type, as it would generally be written in C# source code (ex. `bool`).
 - The name of the `Utf8JsonWriter` write-value method used to write values (ex. `WriteBooleanValue`).
 - The name of the `Utf8JsonReader` get method that could be used to read values (ex. `GetBoolean`).

The library implementation does not look at `DynamicJsonWriteValueAttribute` decorations. These decorations are provided for optional use by the library consumer, when navigating library sources in an IDE, or via reflection on the `DynamicJsonWriter` type.
