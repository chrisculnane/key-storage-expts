using System;
using System.Text;
using System.Threading.Tasks;
using Signing.Services;
using Xamarin.Forms;

namespace Signing {
    public partial class MainPage : ContentPage {
        public MainPage() {
            InitializeComponent();
        }

        async void OnButtonClicked(object sender, EventArgs args)
        {
            var key = DependencyService.Get<IKeyManagerService>();
            var message = Encoding.ASCII.GetBytes("Hello World");
            var signature = key.Sign(message);
            var verified = key.Verify(signature, message);
            ContentLabel.Text = $"Signature: {Convert.ToBase64String(signature)}"
                + $"\nVerified: {verified}";
            Console.WriteLine(ContentLabel.Text);
        }
    }
}
