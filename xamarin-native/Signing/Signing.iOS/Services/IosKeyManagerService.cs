    using System;
using Binding;
using Foundation;
using Signing.Services;

[assembly: Xamarin.Forms.Dependency(typeof(Signing.iOS.Services.IosKeyManagerService))]
namespace Signing.iOS.Services {
    public class IosKeyManagerService : IKeyManagerService {
        private readonly KeyManager key;

        public IosKeyManagerService() {
            key = new KeyManager();
        }

        byte[] IKeyManagerService.Sign(byte[] message) {
            return Array.ConvertAll(
                key.Sign(Array.ConvertAll(message, NSNumber.FromByte)),
                n => n.ByteValue);
        }

        bool IKeyManagerService.Verify(byte[] signature, byte[] message) {
            var sig = Array.ConvertAll(signature, NSNumber.FromByte);
            var msg = Array.ConvertAll(message, NSNumber.FromByte);
            return key.Verify(sig, msg);
        }
    }
}
