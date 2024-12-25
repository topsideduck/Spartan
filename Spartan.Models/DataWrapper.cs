using MessagePack;

namespace Spartan.Models;

[MessagePackObject]
public class DataWrapper
{
    [Key(0)] public required string TypeName { get; set; }

    [Key(1)] public required byte[] Data { get; set; }
}