using OsintCompanion.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.Xmp;
using MimeDetective;
using MimeDetective.Definitions;
using Microsoft.AspNetCore.Authorization.Infrastructure;


namespace OsintCompanion.Services
{
    /// <summary>
    /// Handles extraction of metadata from both URLs and local files.
    /// Supports HTML meta tags and image EXIF (cross-platform using MetadataExtractor).
    /// </summary>
    public class MetadataService
    {
        private readonly HttpClient _httpClient;

        public MetadataService()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Automatically detects input type (URL or file) and extracts metadata.
        /// </summary>
        public async Task<MetadataResult> ExtractAsync(string input)
        {
            if (Uri.IsWellFormedUriString(input, UriKind.Absolute))
                return await ExtractFromUrlAsync(input);
            else if (File.Exists(input))
                return await ExtractFromFileAsync(input);
            else
                throw new ArgumentException("Invalid URL or file path.");
        }

        /// <summary>
        /// Extract metadata from a webpage via HTTP.
        /// </summary>
        private async Task<MetadataResult> ExtractFromUrlAsync(string url)
        {
            var result = new MetadataResult { Source = url };

            try
            {
                var html = await _httpClient.GetStringAsync(url);
                result.ContentType = "HTML";

                // Extract <title>
                var titleMatch = Regex.Match(html, @"<title>(.*?)<\/title>", RegexOptions.IgnoreCase);
                if (titleMatch.Success)
                    result.Title = titleMatch.Groups[1].Value.Trim();

                // Extract meta description
                var descMatch = Regex.Match(html,
                    @"<meta\s+name=[""']description[""']\s+content=[""'](.*?)[""']",
                    RegexOptions.IgnoreCase);
                if (descMatch.Success)
                    result.Description = descMatch.Groups[1].Value.Trim();

                // Extract meta keywords
                var keyMatch = Regex.Match(html,
                    @"<meta\s+name=[""']keywords[""']\s+content=[""'](.*?)[""']",
                    RegexOptions.IgnoreCase);
                if (keyMatch.Success)
                {
                    var keywords = keyMatch.Groups[1].Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var k in keywords)
                        result.Keywords.Add(k.Trim());
                }

                // Extract author meta
                var authorMatch = Regex.Match(html,
                    @"<meta\s+name=[""']author[""']\s+content=[""'](.*?)[""']",
                    RegexOptions.IgnoreCase);
                if (authorMatch.Success)
                    result.Author = authorMatch.Groups[1].Value.Trim();

                // Extract Open Graph tags (extra info)
                var ogTags = Regex.Matches(html, @"<meta\s+property=[""']og:(.*?)[""']\s+content=[""'](.*?)[""']", RegexOptions.IgnoreCase);
                foreach (Match og in ogTags)
                    result.Extra[$"og:{og.Groups[1].Value}"] = og.Groups[2].Value;
            }
            catch (Exception ex)
            {
                result.Description = $"Error reading metadata: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Extract metadata from local files (images, HTML/XML, etc.).
        /// </summary>
        private async Task<MetadataResult> ExtractFromFileAsync(string path)
        {
            var result = new MetadataResult { Source = path };

            try
            {
                var ext = Path.GetExtension(path).ToLower();
                result.ContentType = ext switch
                {
                    ".jpg" or ".jpeg" or ".png" or ".tiff" => "Image",
                    ".xml" or ".html" or ".htm" => "Document",
                    _ => "Unknown"
                };

                if (result.ContentType == "Image")
                {
                    // ✅ Cross-platform EXIF extraction (MetadataExtractor)
                    await Task.Run(() =>
                    {
                        try
                        {
                            var directories = ImageMetadataReader.ReadMetadata(path);
                            foreach (var directory in directories)
                            {
                                foreach (var tag in directory.Tags)
                                {
                                    string key = $"{directory.Name} - {tag.Name}";
                                    result.ExifData[key] = tag.Description ?? "N/A";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Description = $"EXIF read error: {ex.Message}";
                        }
                    });
                }
                else if (result.ContentType == "Document")
                {
                    var text = await File.ReadAllTextAsync(path);
                    var titleMatch = Regex.Match(text, @"<title>(.*?)<\/title>", RegexOptions.IgnoreCase);
                    if (titleMatch.Success)
                        result.Title = titleMatch.Groups[1].Value.Trim();
                }
            }
            catch (Exception ex)
            {
                result.Description = $"File read error: {ex.Message}";
            }

            return result;
        }


        /// <summary>
        ///         This is a Portable Method to Detect Data type With MIME-Detective
        ///         as the Inspector of Extended Data Type/Extensions
        /// </summary>
        /// <returns>
        ///         This will returns "DataTypeResult" variable 
        /// </returns>
        private static IEnumerable<object> DetectFileTypeMime(string path)
        {
            /// <summary> 
            /// 		Default ContentInspector Builder for MIME-Detective 
            /// </summary>
            /// <returns> 
            /// 		This will returns a Type or Variable called "Inspector" that can be
            /// 		Used Accross the Project
            /// 
            /// 		Use it like this:
            /// 		var Results = Inspector.Inspect(ContentByteArray);
            /// 		var Results = Inspector.Inspect(ContentStream);
            /// 		var Results = Inspector.Inspect(ContentFileName);
            /// 		
            /// 		And Group Results Based on File Extension or Mime Type:
            /// 		var ResultsByFileExtension = Results.ByFileExtensions();
            /// 		var ResultsByFileExtension = Results.ByMimeType();
            /// </returns>
            var Inspector = new ContentInspectorBuilder()
            {
                Definitions = MimeDetective.Definitions.DefaultDefinitions.All()
            }.Build();

            // Logic to Inspect Data
            var DataTypeResult = Inspector.Inspect(path);
            return DataTypeResult;
        }
    }
}
