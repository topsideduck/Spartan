using MessagePack;

namespace Spartan.Models;

[MessagePackObject]
public class MessageWrapperModel
{
    [Key("TypeName")] public required string TypeName { get; set; } // Fully qualified type name

    [Key("Data")] public required byte[] Data { get; set; } // Serialized object data
}