using System;
using System.Text;
using Signing.Services;
using Xamarin.Forms;

namespace Signing {
    public partial class MainPage : ContentPage {
        public MainPage() {
            InitializeComponent();
            var key = DependencyService.Get<IKeyManagerService>();
            var message = Encoding.ASCII.GetBytes("Hello World");
            var signature = key.Sign(message);
            var verified = key.Verify(signature, message);
            ContentLabel.Text = $"Signature: {Convert.ToBase64String(signature)}"
                + $"\nVerified: {verified}";
        }
    }
}
