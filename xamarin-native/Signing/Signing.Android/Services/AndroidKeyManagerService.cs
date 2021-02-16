using System;
using Com.Example.Keymanager;
using Signing.Services;

namespace Signing.Droid.Services {
    public class AndroidKeyManagerService : IKeyManagerService {
        private readonly KeyManager key;

        public AndroidKeyManagerService() {
            key = new KeyManager("key-tag");
        }

        byte[] IKeyManagerService.Sign(byte[] message) {
            return key.Sign(message);
        }

        bool IKeyManagerService.Verify(byte[] signature, byte[] message) {
            return key.Verify(signature, message);
        }
    }
}
