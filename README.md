## Guide to files
This directory contains several projects demonstrating how to interact with the Secure Enclave/Android Keystore in Xamarin. A description follows:

* `xamarin-default`: a Xamarin project that uses the built-in Xamarin Secure Storage library (does NOT directly use SE/Keystore) to sign messages.
* `android/`: an Android Studio project containing a Kotlin library (`KeyStoreTest/KeyManager/`) and a demo application (`KeyStoreTest/`) that uses the Android Keystore to sign messages.
* `ios/`: an Xcode project containing a Swift library  (`KeyManager/`) that uses the iOS Secure Enclave to sign messages.
* `KeyManagerAndroid/`: a Xamarin project containing bindings for the Kotlin `KeyManager` library.
* `KeyManagerIos/`: a Xamarin project containing bindings for the Swift `KeyManager library`.
* `xamarin-native`: a Xamarin project that links to `KeyManager(Android|Ios)` to sign messages in Xamarin using the Secure Enclave/Android Keystore.

## Building
### Android
* Building `android/KeyStoreTest/KeyManager/` (e.g. open `KeyManager.kt` and click Build -> Make Module '...') produces an Android library at `android/KeystoreTest/KeyManager/build/outputs/aar`.
* Copying the Android library to `KeyManagerAndroid/KeyManagerAndroid/Jars` allows `KeyManagerAndroid` to build, producing a Xamarin Android DLL at `KeyManagerAndroid/KeyManagerAndroid/bin/Debug`.
* Adding this as a dependency to `xamarin-native` (right-click "Dependencies" under the Android project, click "Add Reference", and click "Add From...".) allows it to build and run on the Android emulator, using the Android Keystore.