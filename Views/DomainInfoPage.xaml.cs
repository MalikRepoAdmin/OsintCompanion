using System.Net.Http.Json;
using System.Net;
using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;
using Microsoft.Maui.Graphics;
using OsintCompanion.Models;
using System.ComponentModel;


namespace OsintCompanion.Views
{
    public partial class DomainInfoPage : ContentPage, IQueryAttributable
    {
        private bool _isDrawerOpen = false;
        private const uint AnimationDuration = 300; 
        private const double DrawerWidth = 250; 
        
        private string _activeTabId = string.Empty;
        private readonly Dictionary<string, (IView Content, Color Color)> _drawerConfigs;

        // This is variable to hold B1DomainInfoVM
        private readonly B1DomainInfoVM _viewModel; // Hold the ViewModel

        // This is the CONSTRUCTOR : Also Inject B1DomainInfoVM as ViewModel
        public DomainInfoPage(B1DomainInfoVM viewModel)
        {
            InitializeComponent();

            // any Dependency Injection and Subscribe to Action from B1DomainInfoVM must be placed here

            // Inject the B1DomainInfoVM and set as flag _viewModel
            _viewModel = viewModel;
            this.BindingContext = _viewModel; // Set the ViewModel as the BindingContext

            // Subscribe to property changes in the ViewModel
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            // Subscribe the View's DisplayAlert method to the ViewModel's Action 
            _viewModel.DisplayAlertAction = async (title, message, cancel) =>
            {
                await DisplayAlert(title, message, cancel);
            };


            // Add This line for every page that is child to Drawer
            PageTitleLabel.BindingContext = this;



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

        // Variable / flag for Advance Mode Bool Switcher
        private bool _isAdvancedModeActive = false;

        /// <summary>
        /// This method is called automatically when the page is navigated to.
        /// From the Main Page drawer item
        /// </summary>
        /// <param name="query"></param>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Default state: Advanced components are hidden.
            bool showAdvanced = false;

            if (query.TryGetValue("mode", out object modeObject) && modeObject.ToString() == "Advanced")
            {
                showAdvanced = true;
            }

            SetAdvancedVisibility(showAdvanced);
        }

        /// <summary>
        /// This Method is for Showing "Advanced Content" Logic 
        /// And All x:Name that is advanced content being registered here
        /// </summary>
        public void SetAdvancedVisibility(bool showAdvanced)
        {
            _isAdvancedModeActive = showAdvanced;

            // All basic elements are visible by default because we didn't set IsVisible="False"
            // on them in XAML. We only control the elements that were marked as advanced.

            if (showAdvanced)
            {
                Title = "Domain Info (Advanced Mode)";

                // 🚨 Make all Advanced elements visible 🚨
                // Register all the x:Name that is an Advanced Content
                AdvancedAPI.IsVisible = true;
                // ... (Add any other Advanced-only controls here)
            }
            else // Handles "Basic" mode, or if the parameter is missing/wrong.
            {
                Title = "Domain Info (Basic Mode)";

                // 🚨 Ensure all Advanced elements are hidden 🚨
                AdvancedAPI.IsVisible = false;
                // ... (Add any other Advanced-only controls here)
            }
        }


        // 🚨 REMOVED: The problematic OnBackButtonPressed override is gone. 🚨

        /// <summary>
        /// Handles the click event for the custom back button.
        /// This relies on the clean stack created by the aggressive navigation in A1MainMenu.
        /// </summary>
        private async void CustomBackButton_Clicked(object sender, EventArgs e)
        {
            // 1. If currently Advanced, switch state to Basic.
            if (_isAdvancedModeActive)
            {
                // Action: Switch to Basic Mode (updates flag to false)
                SetAdvancedVisibility(false); 
                // Return, do not navigate away from the page.
                return; 
            } 
            
            // 2. If already Basic, navigate back one level (to A1MainMenu).
            try
            {
                // This must now be a single click because the stack has been reset.
                
                await Shell.Current.GoToAsync(".."); 
            }
            catch (Exception ex)
            {
                await DisplayAlert("Navigation Error", "Could not return to the previous page.", "OK");
            }
        }

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
    



        //////////////////////////////////////////////////////////////////////////
        // Below is the actual code to bind with ViewModel


        // New handler to watch for when the LookupResult changes
        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(B1DomainInfoVM.LookupResult))
            {
                // This is purely UI manipulation, so it's acceptable in the code-behind
                ShowResult(_viewModel.LookupResult);

                // Also, handle the IsBusy state here to manually control the LoadingIndicator visibility 
                // if you didn't use XAML binding for it (though XAML binding is cleaner).
                if (e.PropertyName == nameof(B1DomainInfoVM.IsBusy))
                {
                    LoadingIndicator.IsRunning = _viewModel.IsBusy;
                    LoadingIndicator.IsVisible = _viewModel.IsBusy;
                }
            }
        }
        
        // This method is moved from the old backend section, now triggered by ViewModel_PropertyChanged
        private void ShowResult(IpApiResponse? data)
        {
            // Clear the container first
            ResultContainer.Children.Clear();
            
            if (data == null) return; // Nothing to show

            void AddLine(string label, string? value)
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                ResultContainer.Children.Add(new Label
                {
                    Text = $"{label}: {value}",
                    FontSize = 14,
                    TextColor = Colors.Black,
                    LineBreakMode = LineBreakMode.WordWrap,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start
                });
            }

            // Map the data model to the UI labels
            AddLine("IP", data.Ip);
            AddLine("City", data.City);
            AddLine("Region", data.Region);
            AddLine("Country", data.CountryName);
            AddLine("ISP / Org", data.Org);
            AddLine("ASN", data.Asn);
            AddLine("Latitude", data.Latitude?.ToString());
            AddLine("Longitude", data.Longitude?.ToString());
            AddLine("Timezone", data.Timezone);
        }
    }
}
