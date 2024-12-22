using MessagePack;

namespace Spartan.Models;

[MessagePackObject]
public class SchemaWrapper
{
    [Key(0)] public SchemaInfo Schema { get; set; }

    [Key(1)] public byte[] Data { get; set; }
}

[MessagePackObject]
public class SchemaInfo
{
    [Key(0)] public string TypeName { get; set; } // Fully qualified type name

    [Key(1)] public Dictionary<string, string> FieldSchema { get; set; } // Field names and types
}