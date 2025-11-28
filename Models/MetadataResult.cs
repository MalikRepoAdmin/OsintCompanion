using System.Collections.Generic;

namespace OsintCompanion.Models
{
    /// <summary>
    /// Represents metadata extracted from a webpage or file.
    /// </summary>
    public class MetadataResult
    {
        /// <summary> The source URL or file path. </summary>
        public string Source { get; set; }

        /// <summary> Detected content type (HTML, Image, Document, etc.). </summary>
        public string ContentType { get; set; }

        /// <summary> Page or file title. </summary>
        public string Title { get; set; }

        /// <summary> Meta description or summary. </summary>
        public string Description { get; set; }

        /// <summary> Meta keywords if found. </summary>
        public List<string> Keywords { get; set; } = new();

        /// <summary> Author name (meta or EXIF). </summary>
        public string Author { get; set; }

        /// <summary> Creation date if available. </summary>
        public string CreatedDate { get; set; }

        /// <summary> Last modified date. </summary>
        public string ModifiedDate { get; set; }

        /// <summary> For image files: EXIF info. </summary>
        public Dictionary<string, string> ExifData { get; set; } = new();

        /// <summary> Any extra discovered metadata fields. </summary>
        public Dictionary<string, string> Extra { get; set; } = new();

        /// <summary> Returns true if any metadata was extracted. </summary>
        public bool HasData =>
            !string.IsNullOrEmpty(Title) ||
            !string.IsNullOrEmpty(Description) ||
            Keywords.Count > 0 ||
            ExifData.Count > 0;
    }
}
