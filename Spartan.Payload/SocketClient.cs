using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Spartan.Utils;

namespace Spartan.Payload;

public class SocketClient : IDisposable
{
    private readonly ClientRatchet _clientRatchet;
    private readonly BinaryReader _binaryReader;
    private readonly BinaryWriter _binaryWriter;

    public SocketClient(BinaryReader binaryReader, BinaryWriter binaryWriter)
    {
        _binaryReader = binaryReader;
        _binaryWriter = binaryWriter;

        _clientRatchet = new ClientRatchet();
        
        PerformX3dhHandshake();
        
        _clientRatchet.InitializeRatchet();
    }

    public void Dispose()
    {
        _binaryReader.Dispose();
        _binaryWriter.Dispose();
        GC.SuppressFinalize(this);
    }

    private void SendClientPublicKeys()
    {
        var clientPublicKeysDictionary = new Dictionary<string, byte[]>
        {
            { "IKaPublicKey", _clientRatchet.IKaPublicKey },
            { "EKaPublicKey", _clientRatchet.EKaPublicKey }
        };

        // Serialize the dictionary into json
        var serializedClientPublicKeysDictionary = JsonSerializer.Serialize(clientPublicKeysDictionary);

        // convert to byte array
        var serializedClientPublicKeysDictionaryBytes = Encoding.UTF8.GetBytes(serializedClientPublicKeysDictionary);

        // send the length of the json
        _binaryWriter.Write(serializedClientPublicKeysDictionaryBytes.Length);

        // send the json
        _binaryWriter.Write(serializedClientPublicKeysDictionaryBytes);
    }

    private Dictionary<string, byte[]>? ReceiveServerPublicKeys()
    {
        var serializedServerPublicKeysDictionaryBytesLength = _binaryReader.ReadInt32();
        var serializedServerPublicKeysDictionaryBytes =
            _binaryReader.ReadBytes(serializedServerPublicKeysDictionaryBytesLength);

        // convert to json
        var serializedServerPublicKeysDictionary = Encoding.UTF8.GetString(serializedServerPublicKeysDictionaryBytes);

        // deserialize the json
        var serverPublicKeysDictionary =
            JsonSerializer.Deserialize<Dictionary<string, byte[]>>(serializedServerPublicKeysDictionary);

        return serverPublicKeysDictionary;
    }

    private void PerformX3dhHandshake()
    {
        SendClientPublicKeys();

        var serverPublicKeysDictionary = ReceiveServerPublicKeys();
        _clientRatchet.X3dh(serverPublicKeysDictionary["SPKbPublicKey"], serverPublicKeysDictionary["IKbPublicKey"],
            serverPublicKeysDictionary["OPKbPublicKey"]);
    }

    public void SendData(byte[] rawData)
    {
        var (key, iv) = _clientRatchet.SendRotate();

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        _binaryWriter.Write(_clientRatchet.DhRatchetPublicKey.Length);
        _binaryWriter.Write(_clientRatchet.DhRatchetPublicKey);

        using var encryptor = aes.CreateEncryptor();
        using var dataStream = new MemoryStream(rawData);

        var buffer = new byte[8192];

        while (dataStream.Read(buffer, 0, buffer.Length) > 0)
        {
            var encryptedChunk = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);

            _binaryWriter.Write(encryptedChunk.Length);
            _binaryWriter.Write(encryptedChunk);
        }

        _binaryWriter.Write(0); // Signal end of file
    }

    public byte[] ReceiveData()
    {
        var dhRatchetPublicKeyLength = _binaryReader.ReadInt32();
        var dhRatchetPublicKeyBytes = _binaryReader.ReadBytes(dhRatchetPublicKeyLength);

        var (key, iv) = _clientRatchet.ReceiveRotate(dhRatchetPublicKeyBytes);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var dataStream = new MemoryStream();

        while (true)
        {
            var chunkSize = _binaryReader.ReadInt32();
            if (chunkSize == 0) break; // End of file

            var encryptedChunk = _binaryReader.ReadBytes(chunkSize);
            var decryptedChunk = decryptor.TransformFinalBlock(encryptedChunk, 0, encryptedChunk.Length);

            dataStream.Write(decryptedChunk, 0, decryptedChunk.Length);
        }

        return dataStream.ToArray();
    }
}