using OsintCompanion.Views;

namespace OsintCompanion;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(SocialLookupPage), typeof(SocialLookupPage));
	}
}


