namespace OsintCompanion.Views
{
    public partial class A1MainMenu : ContentPage
    {
        //Variable for Drawer
        private bool _isDrawerOpen = false;
        private const uint AnimationDuration = 300; 
        private const double DrawerWidth = 250; 
        
        private string _activeTabId = string.Empty;
        private readonly Dictionary<string, (IView Content, Color Color)> _drawerConfigs;

        //Constructor
        public A1MainMenu()
        {
            InitializeComponent();

            _drawerConfigs = new Dictionary<string, (IView Content, Color Color)>
            {
                { "DomainLookup", (CreateMenuContent("IP/Domain Options", "Basic", "Advanced"), (Color)Resources["DomainColor"]) },
                { "SocialLookup", (CreateMenuContent("Social Options", "Basic", "Advanced"), (Color)Resources["SocialColor"]) },
                { "MetadataExtractor", (CreateMenuContent("Metadata Options", "Basic", "Advanced"), (Color)Resources["MetadataColor"]) }
            };
            
            // Set a default active tab on load
            _activeTabId = "DomainLookup"; 
            UpdateDrawerContentAndColor(_activeTabId);
            UpdateHandleVisuals(DomainHandle);
        }



        //Code for Drawer

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // "Closed" State:
            // Hides the DrawerFrame (250 width) but leaves the
            // HandleColumn (50 width) visible at X=0.
            DrawerAssembly.TranslationX = -DrawerWidth; 
            _isDrawerOpen = false;
        }

        private async void Handle_Clicked(object sender, EventArgs e)
        {
            var clickedButton = (Button)sender;
            string tabId = clickedButton.StyleId;

            if (tabId == _activeTabId)
            {
                // Clicked the same tab: Toggle open/closed
                _isDrawerOpen = !_isDrawerOpen;
                await AnimateDrawer(_isDrawerOpen);
            }
            else
            {
                // Clicked a new tab: Switch content and open
                _activeTabId = tabId;

                UpdateDrawerContentAndColor(tabId);
                UpdateHandleVisuals(clickedButton);

                if (!_isDrawerOpen)
                {
                    _isDrawerOpen = true;
                    await AnimateDrawer(isOpening: true);
                }
            }
        }

        private void UpdateDrawerContentAndColor(string tabId)
        {
            if (_drawerConfigs.TryGetValue(tabId, out var config))
            {
                // Update Drawer Content
                DrawerContentLayout.Children.Clear();
                DrawerContentLayout.Children.Add(config.Content);

                // Update Drawer Color to match the button
                DrawerFrame.BackgroundColor = config.Color;
            }
        }

        // **** UPDATED METHOD ****
        // Now highlights the active bookmark instead of changing its color
        private void UpdateHandleVisuals(Button activeHandle)
        {
            foreach (var handle in new List<Button> { DomainHandle, SocialHandle, MetadataHandle })
            {
                if (handle == activeHandle)
                {
                    // Highlight the active button with a border
                    handle.BorderColor = Color.FromHex("#E0B6E4"); 
                    handle.BorderWidth = 3;
                }
                else
                {
                    // Remove border from inactive buttons
                    handle.BorderWidth = 0; 
                }
            }
        }

        private Task AnimateDrawer(bool isOpening)
        {
            // OPEN:   TranslationX = 0 (The whole assembly is visible)
            // CLOSED: TranslationX = -DrawerWidth (Hides the frame, shows the handles)
            double targetTranslationX = isOpening ? 0 : -DrawerWidth;
            return DrawerAssembly.TranslateTo(targetTranslationX, DrawerAssembly.Y, AnimationDuration, Easing.CubicOut);
        }
        
        

        //********************************************************//
        

        //Code for UI Logic
        

       

        // MODIFIED: Attach the click handler to the menu buttons (REPLACEMENT for the original)
        private IView CreateMenuContent(string title, string btn1, string btn2)
        {
            // Helper function to create a clickable button.
            // NOTE: This assumes btn1 will always be 'Basic' and btn2 will always be 'Advanced'
            Button CreateMenuItemButton(string text)
            {
                var button = new Button 
                { 
                    Text = text, 
                    BackgroundColor = Colors.White, 
                    TextColor = Colors.Black,
                    // Use StyleId to store the target page name AND the mode, separated by |
                    // Example: "DomainInfoPage|Basic"
                    StyleId = $"{_activeTabId}|{text}" // Use a consistent target page name here
                };
                // Attach the new click handler
                button.Clicked += MenuItemClicked;
                return button;
            }

            return new VerticalStackLayout
            {
                Spacing = 10,
                Children = 
                {
                    new Label { Text = title, FontAttributes = FontAttributes.Bold, FontSize = 18, TextColor = Colors.White },
                    CreateMenuItemButton("Basic"), // Use static text here if you want consistency
                    CreateMenuItemButton("Advanced"), // Use static text here
                }
            };
        }
        
        // NEW METHOD: Handles the redirection when a menu item is clicked
        private async void MenuItemClicked(object sender, EventArgs e)
        {
            var clickedButton = (Button)sender;
            string styleId = clickedButton.StyleId; // Example: "DomainInfoPage|Basic"

            // 1. Split the StyleId into TargetPageName and ModeParameter
            string[] parts = styleId.Split('|');
            if (parts.Length != 2)
            {
                await DisplayAlert("Error", "Invalid menu item configuration.", "OK");
                return;
            }

            string targetPageName = parts[0].ToLower(); // "domaininfopage"
            string mode = parts[1]; // "Basic" or "Advanced"

            // 2. Close the drawer immediately
            if (_isDrawerOpen)
            {
                _isDrawerOpen = false;
                await AnimateDrawer(isOpening: false);
            }

            // 🚨 FINAL STACK CLEARING FIX: Use single, aggressive triple-slash (///) route 🚨
            try
            {
                // Route example: ///main/domainlookup/domaininfopage?mode=Advanced
                // This command should forcibly clear all existing navigation history.
                string rootRoute = $"//mainroute/{_activeTabId.ToLower()}/{targetPageName}?mode={mode}";
                
                await Shell.Current.GoToAsync(rootRoute);


                // Temporary Debug Code
                await DisplayAlert("Debug DomainLookup",$"//mainroute/{_activeTabId.ToLower()}/{targetPageName}?mode={mode}","OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Navigation Error", $"Could not navigate. Error: {ex.Message}", "OK");
            }
        }

        private async void OnHelpClicked(object sender, EventArgs e)
        {
            await Help();
        }
        
        private async Task Help()
        {
            await DisplayAlert("Help","Go To Our Repository","OK");
        }
    }
}