package com.example.keymanager

import android.os.Build
import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import androidx.annotation.RequiresApi
import java.security.KeyPairGenerator
import java.security.KeyStore
import java.security.Signature

@RequiresApi(Build.VERSION_CODES.M)
class KeyManager(val alias: String) {

    private val ks: KeyStore = KeyStore.getInstance("AndroidKeyStore").apply {
        load(null)
    }
    private val aliases = ks.aliases().toList()
    private var entry: KeyStore.PrivateKeyEntry

    init {
        if (!aliases.contains(alias)) {
            createKey()
        }
        entry = getKey()!!
    }

    fun sign(msg: ByteArray): ByteArray {
        return Signature.getInstance("SHA256withECDSA").run {
            initSign(entry.privateKey)
            update(msg)
            sign()
        }
    }

    fun verify(signature: ByteArray, msg: ByteArray): Boolean {
        return Signature.getInstance("SHA256withECDSA").run {
            initVerify(entry.certificate)
            update(msg)
            verify(signature)
        }
    }

    fun getPublicKey(): ByteArray {
        return entry.certificate.publicKey.encoded
    }

    private fun createKey() {
        val kpg: KeyPairGenerator = KeyPairGenerator.getInstance(
            KeyProperties.KEY_ALGORITHM_EC,
            "AndroidKeyStore"
        )
        val parameterSpec: KeyGenParameterSpec = KeyGenParameterSpec.Builder(
            alias,
            KeyProperties.PURPOSE_SIGN or KeyProperties.PURPOSE_VERIFY
        ).run {
            setDigests(KeyProperties.DIGEST_SHA256, KeyProperties.DIGEST_SHA512)
            build()
        }

        kpg.initialize(parameterSpec)
        kpg.generateKeyPair()
    }

    private fun getKey(): KeyStore.PrivateKeyEntry? {
        val entry = ks.getEntry(alias, null)
        return if (entry !is KeyStore.PrivateKeyEntry) {
            null
        } else {
            entry
        }
    }
}