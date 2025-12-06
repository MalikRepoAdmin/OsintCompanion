using OsintCompanion.Views;

namespace OsintCompanion;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// 1. Register the target pages for the navigation to work
		// The argument here must match the targetPageId created in CreateMenuContent.
		Routing.RegisterRoute("domainlookup" /*This is the StyleId in A1MainMenu.xaml but it must be lowercase because of toLower()*/, typeof(OsintCompanion.Views.DomainInfoPage));
		Routing.RegisterRoute("sociallookup", typeof(SocialLookupPage));
		Routing.RegisterRoute("metadataextractor", typeof(MetadataPage));
		// ... and so on for all menu items

		Routing.RegisterRoute(nameof(SocialLookupPage), typeof(SocialLookupPage));
	
	}
}


