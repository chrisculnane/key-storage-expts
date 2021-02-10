package com.example.keystoretest

import android.app.AlertDialog
import android.os.Build
import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import android.util.Log
import android.widget.Toast
import androidx.annotation.RequiresApi
import java.security.KeyPairGenerator
import java.security.KeyStore
import java.security.Signature
import java.util.*

class MainActivity : AppCompatActivity() {
    val alias = "my-alias"

    @RequiresApi(Build.VERSION_CODES.O)
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)
        val ks: KeyStore = KeyStore.getInstance("AndroidKeyStore").apply {
            load(null)
        }
        val aliases: Enumeration<String> = ks.aliases()

        if (!aliases.toList().contains(alias)) {
            /*
         * Generate a new EC key pair entry in the Android Keystore by
         * using the KeyPairGenerator API. The private key can only be
         * used for signing or verification and only with SHA-256 or
         * SHA-512 as the message digest.
         */
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

            val kp = kpg.generateKeyPair()
            val builder = AlertDialog.Builder(this)
            builder.setTitle("Alert")
            builder.setMessage("Generated keypair: " + kp.public.toString())
            builder.show()
        }

        val entry: KeyStore.Entry = ks.getEntry(alias, null)
        if (entry !is KeyStore.PrivateKeyEntry) {
            Log.w("WARN", "Not an instance of a PrivateKeyEntry")
        } else {
            val signature: ByteArray = Signature.getInstance("SHA256withECDSA").run {
                initSign(entry.privateKey)
                update("hello world".encodeToByteArray())
                sign()
            }
            val valid: Boolean = Signature.getInstance("SHA256withECDSA").run {
                initVerify(entry.certificate)
                update("hello world".encodeToByteArray())
                verify(signature)
            }

            val builder = AlertDialog.Builder(this)
            builder.setTitle("Information")
            builder.setMessage("Public key: "
                + Base64.getEncoder().encodeToString(entry.certificate.publicKey.encoded)
                + "\nPrivate key: "
                + entry.privateKey.encoded // returns null because it doesn't actually have access
                + "\nSignature: " + Base64.getEncoder().encodeToString(signature)
                + "\n" + if (valid) { "Verified." } else { "Failed to verify." })
            builder.setPositiveButton("OK", null)
            builder.show()
        }
    }
}