using System;
using System.Collections.Generic;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls;
using OsintCompanion.Services;
using OsintCompanion.Models;

namespace OsintCompanion.Views
{
    public partial class SocialLookupPage : ContentPage, IQueryAttributable
    {
        // Single service instance for the page (stateless HttpClient inside service)
        private readonly SocialSearchService _socialSearchService = new();

        // This is the CONSTRUCTOR
        public SocialLookupPage()
        {
            InitializeComponent();

            //Add This line for every page that is child to Drawer
            PageTitleLabel.BindingContext = this;
            
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
                Title = "Social Lookup (Advanced Mode)";

                // 🚨 Make all Advanced elements visible 🚨
                // Register all the x:Name that is an Advanced Content
                AdvancedAPISocial.IsVisible = true;
                // ... (Add any other Advanced-only controls here)
            }
            else // Handles "Basic" mode, or if the parameter is missing/wrong.
            {
                Title = "Social Lookup (Basic Mode)";

                // 🚨 Ensure all Advanced elements are hidden 🚨
                AdvancedAPISocial.IsVisible = false;
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




        /***************************************/
        // Below is ViewModel(VM) logic to bridge between View and Model/Business Logic

        // Button click handler
        private async void OnSearchClicked(object sender, EventArgs e)
        {
            DebugLabel.Text = ""; // clear logs
            StatusLabel.Text = "";

            string username = InputEntry.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(username))
            {
                await DisplayAlert("Input Required", "Please enter a username.", "OK");
                return;
            }

            try
            {
                // Show spinner immediately and allow UI to render
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                StatusLabel.Text = "Looking up...";
                await Task.Yield(); // yield so UI can update

                // Run search with an inline logger callback
                DebugLabel.Text = string.Empty; // ✅ clear previous logs

                var results = await _socialSearchService.SearchUsernameAsync(username, Log);

                if (results == null || results.Count == 0)
                {
                    await DisplayAlert("No Results", "No sites responded or no matches found.", "OK");
                    ResultContainer.ItemsSource = new List<SocialResult>();
                    StatusLabel.Text = "No results";
                    return;
                }

                // Bind results (CollectionView handles scrolling)
                ResultContainer.ItemsSource = results;
                StatusLabel.Text = $"Found matches on {results.Count} sites";

                // Give focus to the CollectionView so mouse wheel scroll works on desktop
                ResultContainer.Focus();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lookup Failed", ex.ToString(), "OK");
                StatusLabel.Text = "Lookup failed";
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        // Reuse the search for Enter key
        private void OnEntryCompleted(object sender, EventArgs e)
            => OnSearchClicked(sender, e);

        // Append logs into the DebugLabel safely on main thread
        private void Log(string text)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // prepend timestamp for easier tracing
                var ts = DateTime.Now.ToString("HH:mm:ss");
                DebugLabel.Text += $"\n[{ts}] {text}";
            });
        }
    }
}
