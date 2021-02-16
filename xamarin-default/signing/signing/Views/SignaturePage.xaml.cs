using System;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace signing.Views {
    public partial class SignaturePage : ContentPage {
        public AsymmetricCipherKeyPair KeyPair {
            get; private set;
        }

        public SignaturePage() {
            InitializeComponent();

            var progress = new Progress<string>(update => { ContentLabel.Text = update; });
            Task.Run(() => GetOrGenerateKey(progress));
        }

        async void GetOrGenerateKey(IProgress<string> progress) {
            try {
                var key = await SecureStorage.GetAsync("key");
                if (key is null) {
                    KeyPair = GenerateKeyPair();
                    var pubkeyBytes = ((ECPublicKeyParameters)KeyPair.Public).Q.GetEncoded();
                    var privkeyBytes = ((ECPrivateKeyParameters)KeyPair.Private).D.ToByteArray();
                    await SecureStorage.SetAsync("key",
                        $"{Convert.ToBase64String(pubkeyBytes)}:{Convert.ToBase64String(privkeyBytes)}");
                } else {
                    var pubkeyBytes = Convert.FromBase64String(key.Split(':')[0]);
                    var privkeyBytes = Convert.FromBase64String(key.Split(':')[1]);
                    var curve = ECNamedCurveTable.GetByName("P-256");
                    var pubKey = new ECPublicKeyParameters(curve.Curve.DecodePoint(pubkeyBytes), DomainParams);
                    var privKey = new ECPrivateKeyParameters(new BigInteger(privkeyBytes), DomainParams);
                    KeyPair = new AsymmetricCipherKeyPair(pubKey, privKey);
                }

                var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
                var msg = Encoding.ASCII.GetBytes("Hello World");
                signer.Init(true, KeyPair.Private);
                signer.BlockUpdate(msg, 0, msg.Length);
                var signature = signer.GenerateSignature();

                signer.Init(false, KeyPair.Public);
                signer.BlockUpdate(msg, 0, msg.Length);
                var verified = signer.VerifySignature(signature);

                var pubkey = Convert.ToBase64String(((ECPublicKeyParameters)KeyPair.Public).Q.GetEncoded());
                progress.Report($"Public key: {pubkey}"
                    + $"\nSignature: {Convert.ToBase64String(signature)}"
                    + $"\nVerified: {verified}");
            } catch (Exception e) {
                System.Console.WriteLine("Failed: " + e.Message + "\n" + e.StackTrace);
            }
        }

        static ECDomainParameters DomainParams {
            get {
                var curve = ECNamedCurveTable.GetByName("P-256");
                return new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
            }
        }

        static AsymmetricCipherKeyPair GenerateKeyPair() {
            var rng = new SecureRandom();
            var keyParams = new ECKeyGenerationParameters(DomainParams, rng);

            var generator = new ECKeyPairGenerator("ECDSA");
            generator.Init(keyParams);
            return generator.GenerateKeyPair();
        }
    }
}
