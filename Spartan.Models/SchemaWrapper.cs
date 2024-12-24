using MessagePack;

namespace Spartan.Models;

[MessagePackObject]
public class SchemaWrapper
{
    [Key(0)] public SchemaInfo Schema { get; set; }

    [Key(1)] public byte[] Data { get; set; }
}