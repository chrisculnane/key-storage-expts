using Foundation;

namespace Binding {
	// @interface KeyManager : NSObject
	[BaseType(typeof(NSObject))]
	[Protocol]
	interface KeyManager {
		// -(NSArray * _Nonnull)sign:(NSArray * _Nonnull)message __attribute__((warn_unused_result("")));
		[Export("sign:")]
		NSNumber[] Sign(NSNumber[] message);

		// -(BOOL)verify:(NSArray<NSNumber *> * _Nonnull)signature message:(NSArray<NSNumber *> * _Nonnull)message __attribute__((warn_unused_result("")));
		[Export("verify:message:")]
		bool Verify(NSNumber[] signature, NSNumber[] message);
	}
}