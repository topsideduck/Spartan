using MessagePack;

namespace Spartan.Models;

[MessagePackObject]
public class ServerPublicKeys
{
    [Key("IKbPublicKey")] public required byte[] IKbPublicKey { get; set; }
    [Key("SPKbPublicKey")] public required byte[] SPKbPublicKey { get; set; }
    [Key("OPKbPublicKey")] public required byte[] OPKbPublicKey { get; set; }
    
    
}