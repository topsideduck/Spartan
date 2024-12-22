using MessagePack;

namespace Spartan.Models.Payload;

[MessagePackObject]
public class ServerPublicKeysModel
{
    [Key("IKbPublicKey")] public required byte[] IKbPublicKey { get; set; }
    [Key("SPKbPublicKey")] public required byte[] SPKbPublicKey { get; set; }
    [Key("OPKbPublicKey")] public required byte[] OPKbPublicKey { get; set; }
}