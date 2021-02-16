using signing.Views;
using Xamarin.Forms;

namespace signing {
    public partial class AppShell : Shell {
        public AppShell() {
            InitializeComponent();
            Routing.RegisterRoute(nameof(SignaturePage), typeof(SignaturePage));
        }

    }
}
