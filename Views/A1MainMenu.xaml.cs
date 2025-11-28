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
                { "Folder", (CreateMenuContent("Folder Menu", "Documents", "Archives"), (Color)Resources["FolderColor"]) },
                { "Settings", (CreateMenuContent("Settings Menu", "Profile", "Notifications"), (Color)Resources["SettingsColor"]) },
                { "Home", (CreateMenuContent("Home Menu", "Dashboard", "Summary"), (Color)Resources["HomeColor"]) }
            };
            
            // Set a default active tab on load
            _activeTabId = "Folder"; 
            UpdateDrawerContentAndColor(_activeTabId);
            UpdateHandleVisuals(FolderHandle);
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
            foreach (var handle in new List<Button> { FolderHandle, SettingsHandle, HomeHandle })
            {
                if (handle == activeHandle)
                {
                    // Highlight the active button with a border
                    handle.BorderColor = Colors.White; 
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
        
        // **** UPDATED METHOD ****
        // Added TextColor to look good on the dark drawer backgrounds
        private IView CreateMenuContent(string title, string btn1, string btn2)
        {
            return new VerticalStackLayout
            {
                Spacing = 10,
                Children = {
                    new Label { Text = title, FontAttributes = FontAttributes.Bold, FontSize = 18, TextColor = Colors.White },
                    new Button { Text = btn1, BackgroundColor=Colors.White, TextColor=Colors.Black },
                    new Button { Text = btn2, BackgroundColor=Colors.White, TextColor=Colors.Black },
                }
            };
        }

        //********************************************************//
        

        //Code for UI Logic
        

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