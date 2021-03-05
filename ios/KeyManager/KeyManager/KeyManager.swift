//
//  KeyManager.swift
//  KeyManager
//
//  Created by Eleanor McMurtry on 15/2/21.
//

import CryptoKit
import LocalAuthentication

@objc(KeyManager)
public class KeyManager: NSObject {
    var key: AgnosticP256Key
    
    @objc
    public override init() {
        key = AgnosticP256Key.getOrGenerate()!
    }
    
    @objc
    public func sign(_ message: [UInt8]) -> [UInt8] {
        return Array(key.sign(Data(message)).rawRepresentation)
    }
    
    @objc
    public func verify(_ signature: [UInt8], message: [UInt8]) -> Bool {
        let sig = try! P256.Signing.ECDSASignature(rawRepresentation: Data(signature))
        return key.verify(sig, data: Data(message))
    }
}

// Wrapper class to act as a generic P256 signing key, backed by either a SecureEnclave key (if available)
// or a Keychain key.
enum AgnosticP256Key {
    case SecEnclaveKey(SecureEnclave.P256.Signing.PrivateKey)
    case KeychainKey(P256.Signing.PrivateKey)
    
    func sign(_ data: Data) -> P256.Signing.ECDSASignature {
        switch self {
        case let .SecEnclaveKey(key):
            return try! key.signature(for: data)
        case let .KeychainKey(key):
            return try! key.signature(for: data)
        }
    }
    
    func verify(_ signature: P256.Signing.ECDSASignature, data: Data) -> Bool {
        switch self {
        case let .SecEnclaveKey(key):
            return key.publicKey.isValidSignature(signature, for: data)
        case let .KeychainKey(key):
            return key.publicKey.isValidSignature(signature, for: data)
        }
    }
    
    func publicKeyBytes() -> Data {
        switch self {
        case let .SecEnclaveKey(key):
            return key.publicKey.rawRepresentation
        case let .KeychainKey(key):
            return key.publicKey.rawRepresentation
        }
    }
    
    static func getOrGenerate() -> Self? {
        // For some reason, SecureEnclave.isAvailable returns true on the simulator
        // but it doesn't actually work.
        #if targetEnvironment(simulator)
        let isSimulator = true
        #else
        let isSimulator = false
        #endif
        
        if (!isSimulator && SecureEnclave.isAvailable) {
            print("using Secure Enclave")
            return getSecureEnclaveKey().map({
                SecEnclaveKey($0)
            })
        } else {
            print("using Keychain")
            return getKeychainKey().map({
                KeychainKey($0)
            })
        }
    }
    
    private static func getSecureEnclaveKey() -> SecureEnclave.P256.Signing.PrivateKey? {
        // look up encrypted key from Keychain
        let query: [CFString: Any] = [
            kSecClass: kSecClassGenericPassword,
            kSecReturnData: true,
        ]
        var result: AnyObject?
        
        switch SecItemCopyMatching(query as CFDictionary, &result) {
        case errSecSuccess:
            // use encrypted key to load the real key from SecureEnclave
            let keyData = result as! Data
            let context = LAContext()
            return try! SecureEnclave.P256.Signing.PrivateKey(dataRepresentation: keyData, authenticationContext: context)
        case errSecItemNotFound:
            // generate key
            let context = LAContext()
            guard let accessControl = SecAccessControlCreateWithFlags(
               nil,
               kSecAttrAccessibleWhenUnlockedThisDeviceOnly,
               [.privateKeyUsage],
               nil
            ) else {
                print("failed to create access control")
                return nil
            }
            guard let key = try? SecureEnclave.P256.Signing.PrivateKey(accessControl: accessControl, authenticationContext: context) else {
                print("failed to generate key")
                return nil
            }
            // save key
            let query: [CFString: Any] = [
                kSecClass: kSecClassGenericPassword,
                kSecUseDataProtectionKeychain: true,
                kSecValueData: key.dataRepresentation
            ]
            let status = SecItemAdd(query as CFDictionary, nil)
            guard status == errSecSuccess else {
                print("failed saving encrypted key in keychain: \(status)")
                return nil
            }
            return key
        case let status:
            print("error accessing keychain: \(status)")
            return nil
        }
    }
    
    private static func getKeychainKey() -> P256.Signing.PrivateKey? {
        // look up key from Keychain
        let query: [CFString: Any ] = [
            kSecClass: kSecClassKey,
            kSecAttrAccessible: kSecAttrAccessibleWhenUnlocked,
            kSecUseDataProtectionKeychain: true,
            kSecReturnRef: true,
        ]
        var result: CFTypeRef?
        var secKey: SecKey
        
        switch SecItemCopyMatching(query as CFDictionary, &result) {
        case errSecSuccess:
            secKey = result as! SecKey
            // load the key from the native representation
            guard let data = SecKeyCopyExternalRepresentation(secKey, nil) as Data? else {
                return nil
            }
            return try? P256.Signing.PrivateKey(x963Representation: data)
        case errSecItemNotFound:
            // generate key
            let key = P256.Signing.PrivateKey()
            
            // get a SecKey representation
            let attributes = [kSecAttrKeyType: kSecAttrKeyTypeECSECPrimeRandom,
                              kSecAttrKeyClass: kSecAttrKeyClassPrivate] as [String: Any]

            guard let secKey = SecKeyCreateWithData(key.x963Representation as CFData,
                                                    attributes as CFDictionary,
                                                    nil) else {
                print("Unable to create SecKey representation.")
                return nil
            }
            let query: [CFString: Any] = [kSecClass: kSecClassKey,
                         kSecAttrAccessible: kSecAttrAccessibleWhenUnlocked,
                         kSecUseDataProtectionKeychain: true,
                         kSecValueRef: secKey]

            // add the key to the keychain
            let status = SecItemAdd(query as CFDictionary, nil)
            guard status == errSecSuccess else {
                print("Unable to store item: \(status)")
                return nil
            }
            
            return key
        case let status:
            print("error accessing keychain: \(status)")
            return nil
        }
    }
}
