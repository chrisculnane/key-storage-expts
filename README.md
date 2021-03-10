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
(Based on [Xamarin documentation](https://docs.microsoft.com/en-us/xamarin/android/platform/binding-kotlin-library/walkthrough).)
* Use Android Studio to open the `android/KeystoreTest` project and build `KeyManager` (e.g. open `KeyManager.kt` and click Build -> Make Module '...') to produce an Android library at `android/KeystoreTest/KeyManager/build/outputs/aar`.
* Copy the Android library `KeyManager-debug.aar` to `KeyManagerAndroid/KeyManagerAndroid/Jars`. Open the `KeyManagerAndroid` solution (e.g. in Jetbrains Rider or Visual Studio) and build it, producing a Xamarin Android DLL at `KeyManagerAndroid/KeyManagerAndroid/bin/Debug`.
    * **Make sure you open the solution file** (ending in `.sln`), not the directory.
    * Before building, right-click the copied `KeyManager-debug.aar` file and click Properties. Make sure "Build action:" is set to "LibraryProjectZip". (See [here](https://docs.microsoft.com/en-us/xamarin/android/platform/binding-java-library/binding-an-aar) for more information.)
* Open the `xamarin-native` solution (e.g. in Jetbrains Rider or Visual Studio) and add the Xamarin Android DLL as a dependency to `xamarin-native`: right-click "Dependencies" under the Android project (here called `Signing.Android`), click "Add Reference", and click "Add From...". This allows it to build and run on the Android emulator, using the Android Keystore.
    * Again **make sure you open the solution file** (ending in `.sln`), not the directory.

### iOS
(Based on [Xamarin documentation](https://docs.microsoft.com/en-us/xamarin/ios/platform/binding-swift/walkthrough).)
* Build `ios/KeyManager/KeyManager.xcodeproj`:
    * Open the project in Xcode and check that it compiles (Cmd+B).
        * This project has a number of important build settings (see the documentation link above). "Always Embed Swift Standard Libraries" and "Enable Bitcode" are set to `No`, and "Excluded Architectures" -> "Release" -> "Any iOS Simulator SDK" includes `arm64`.
    * Navigate to the directory in a terminal and run `xcodebuild -showsdks` to show available SDKs.
    * Choose an iOS and Simulator SDK; here we will use 14.4. Build by running:
    ```
    xcodebuild -sdk iphonesimulator14.4 -project KeyManager.xcodeproj -configuration Release
    xcodebuild -sdk iphoneos14.4 -project KeyManager.xcodeproj -configuration Release
    ```
    * Compose the builds into a fat library:
    ```
    cp -R Release-iphoneos Release-fat
    cp -R Release-iphonesimulator/KeyManager.framework/Modules/KeyManager.swiftmodule Release-fat/KeyManager.framework/Modules/KeyManager.swiftmodule
    lipo -create -output Release-fat/KeyManager.framework/KeyManager Release-iphoneos/KeyManager.framework/KeyManager Release-iphonesimulator/KeyManager.framework/KeyManager
    ```
    Note: in theory this shouldn't be necessary since the iOS simulator and a real iOS device both use ARM, but due to a quirk/bug in how Visual Studio for Mac handles Xamarin builds for the iOS simulator on Apple silicon, we have to build an x86 library for the simulator, and therefore build a fat library.
* The linked documentation explains how to use Sharpie to generate C# bindings; this has already been done, so open `KeyManagerIos/KeyManagerIos.sln` in Visual Studio and build it.
    * **Note:** I had trouble getting this to work with Debug configuration. You may need to just use Release.
    * If building this project does not work immediately, remove the native reference from the solution, right-click "Native References" -> "Add Native References", and click `ios/KeyManager/build/Release-fat/KeyManager.framework/KeyManager` (i.e. the executable file).
    * Right-click to open the added native reference's Properties and make sure "Smart Link" is checked.
    * Also under Properties, add `Foundation CryptoKit LocalAuthentication` to the Frameworks field.
* Open `xamarin-native/Signing/Signing.sln` in Visual Studio, and add the reference `KeyManagerIos/KeyManagerIos/bin/Debug/NativeLibrary.dll` (as a ".NET Assembly") to the iOS project (here). You can now build and deploy to iOS using the Secure Enclave.
    * In theory this should also work for the iOS Simulator, but I had a lot of trouble doing this and unfortunately can't really figure out why.

**Troubleshooting:**
* Xcode gives `umbrella header ... not found`: open the KeyManager project, and under "Build Phases" -> "Headers", move the header from "Project" to "Public"
* `xcode-select: error: tool 'xcodebuild' requires Xcode, but active developer directory '/Library/Developer/CommandLineTools' is a command line tools instance`: Run this command:
```
sudo xcode-select -s /Applications/Xcode.app/Contents/Developer
```
* Simulator app crashes, debug log shows `error accessing keychain: -34018`:
    * In the `Signing.iOS` project, open `Entitlements.plist` and add the `Keychain Access Groups` property if it is not present.
    * Open `Signing.iOS` project options, and check that "iOS Bundle Signing" has "Custom Entitlements" set to `Entitlements.plist`.
* If you get weird errors in the `xamarin-native` solution, you can try removing the Xamarin.Essentials NuGet package and adding it again.


## Troubleshooting
* If you can't build the KeyManager Android Studio project, try opening the `build.gradle` file in the `KeyManager` directory and clicking any "run" buttons visible in it.
* Try updating Android Studio and/or the Kotlin version.
* Try opening the `KeyManager` project directly, instead of the `KeyStoreTest` parent project.
* Try running the "app" target of the `KeystoreTest` project.