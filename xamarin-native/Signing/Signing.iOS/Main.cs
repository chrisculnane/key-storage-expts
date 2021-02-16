using Signing.iOS.Services;
using Signing.Services;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(IosKeyManagerService))]
namespace Signing.iOS {
    public class Application {
        // This is the main entry point of the application.
        static void Main(string[] args) {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");
            DependencyService.Register<IKeyManagerService, IosKeyManagerService>();
        }
    }
}
