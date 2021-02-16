package com.example.keystoretest

import android.app.AlertDialog
import android.os.Build
import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import androidx.annotation.RequiresApi
import java.util.*

class MainActivity : AppCompatActivity() {
    val alias = "my-alias"

    // Version O (25) is required for base64. We only actually need version M (23).
    @RequiresApi(Build.VERSION_CODES.O)
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)
        val keyManager = com.example.keymanager.KeyManager(alias)

        val signature = keyManager.sign("hello world".encodeToByteArray())
        val verified = keyManager.verify(signature, "hello world".encodeToByteArray())

        val builder = AlertDialog.Builder(this)
        builder.setTitle("Information")
        builder.setMessage("Public key: "
            + Base64.getEncoder().encodeToString(keyManager.getPublicKey())
            + "\nSignature: " + Base64.getEncoder().encodeToString(signature)
            + "\n" + if (verified) { "Verified." } else { "Failed to verify." })
        builder.setPositiveButton("OK", null)
        builder.show()
    }
}