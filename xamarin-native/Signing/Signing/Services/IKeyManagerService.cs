namespace Signing.Services {
    public interface IKeyManagerService {
        byte[] Sign(byte[] message);
        bool Verify(byte[] signature, byte[] message);
    }
}
