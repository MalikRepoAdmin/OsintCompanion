using System;
using System.Collections.Generic;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls;
using OsintCompanion.Services;
using OsintCompanion.Models;

namespace OsintCompanion.Views
{
    public partial class SocialLookupPage : ContentPage
    {
        // Single service instance for the page (stateless HttpClient inside service)
        private readonly SocialSearchService _socialSearchService = new();

        public SocialLookupPage()
        {
            InitializeComponent();
        }

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
