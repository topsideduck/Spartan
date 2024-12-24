using MessagePack;

namespace Spartan.Models;

[MessagePackObject]
public class SchemaInfo
{
    [Key(0)] public string TypeName { get; set; } // Fully qualified type name

    [Key(1)] public Dictionary<string, string> FieldSchema { get; set; } // Field names and types
}