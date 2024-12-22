using MessagePack;

namespace Spartan.Models;

[MessagePackObject]
public class ClientPublicKeysModel
{
    [Key("IKaPublicKey")] public required byte[] IKaPublicKey { get; set; }
    [Key("EKaPublicKey")] public required byte[] EKaPublicKey { get; set; }
}