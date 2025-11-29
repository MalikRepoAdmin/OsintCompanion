using Microsoft.Maui.Controls;
using OsintCompanion.Services;
using System;
using System.Text;
using System.IO;


namespace OsintCompanion.Views
{
    /// <summary>
    /// Handles UI logic(View) and ViewModel for metadata extractor page.
    /// </summary>
    public partial class MetadataPage : ContentPage
    {
        private readonly MetadataService _metadataService;

        public MetadataPage()
        {
            InitializeComponent();

            //Add This line for every page that is child to Drawer
            PageTitleLabel.BindingContext = this;

            //Flag for MetadataService Method from Model(Bussines Logic)
            _metadataService = new MetadataService();
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
                AdvancedAPI.IsVisible = true;
                // ... (Add any other Advanced-only controls here)
            }
            else // Handles "Basic" mode, or if the parameter is missing/wrong.
            {
                Title = "Social Lookup (Basic Mode)";

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



        /***************************************/
        // Below is ViewModel(VM) logic to bridge between View and Model/Business Logic


        /// <summary>
        /// Event handler when user clicks "Extract Metadata" button.
        /// </summary>
        
        private void OnEntryCompleted(object sender, EventArgs e)
            => OnExtractClicked(sender, e);

        private async void OnExtractClicked(object sender, EventArgs e)
        {
            OutputLabel.Text = "Extracting metadata, please wait...";
            var input = InputEntry.Text?.Trim();
            input = input.Replace("\"", "");
            var normalizedInput = Path.GetFullPath(input);

            if (string.IsNullOrEmpty(normalizedInput))
            {
                OutputLabel.Text = @"Please enter a valid URL or file path (e.g ""C:\Users\user\Pictures\Untitled59_20240201105840.png"" )";
                return;
            }

            try
            {
                var result = await _metadataService.ExtractAsync(normalizedInput);
                OutputLabel.Text = FormatResult(result);
            }
            catch (Exception ex)
            {
                OutputLabel.Text = $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Builds a formatted string from the metadata result.
        /// </summary>
        private string FormatResult(OsintCompanion.Models.MetadataResult result)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Source: {result.Source}");
            sb.AppendLine($"Content Type: {result.ContentType}");
            sb.AppendLine();

            if (result.HasData)
            {
                if (!string.IsNullOrEmpty(result.Title))
                    sb.AppendLine($"Title: {result.Title}");
                if (!string.IsNullOrEmpty(result.Description))
                    sb.AppendLine($"Description: {result.Description}");
                if (result.Keywords.Count > 0)
                    sb.AppendLine($"Keywords: {string.Join(", ", result.Keywords)}");
                if (!string.IsNullOrEmpty(result.Author))
                    sb.AppendLine($"Author: {result.Author}");
                if (!string.IsNullOrEmpty(result.CreatedDate))
                    sb.AppendLine($"Created: {result.CreatedDate}");
                if (!string.IsNullOrEmpty(result.ModifiedDate))
                    sb.AppendLine($"Modified: {result.ModifiedDate}");

                if (result.ExifData.Count > 0)
                {
                    sb.AppendLine("\n[EXIF Metadata]");
                    foreach (var kv in result.ExifData)
                        sb.AppendLine($"{kv.Key}: {kv.Value}");
                }

                if (result.Extra.Count > 0)
                {
                    sb.AppendLine("\n[Extra Fields]");
                    foreach (var kv in result.Extra)
                        sb.AppendLine($"{kv.Key}: {kv.Value}");
                }
            }
            else
            {
                sb.AppendLine("No metadata found.");
            }

            return sb.ToString();
        }
    }
}
