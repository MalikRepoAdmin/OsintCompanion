using Microsoft.Maui.Controls;
using OsintCompanion.Services;
using System;
using System.Text;
using System.IO;


namespace OsintCompanion.Views
{
    /// <summary>
    /// Handles UI logic for metadata lookup page.
    /// </summary>
    public partial class MetadataPage : ContentPage
    {
        private readonly MetadataService _metadataService;

        public MetadataPage()
        {
            InitializeComponent();
            _metadataService = new MetadataService();
        }

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
