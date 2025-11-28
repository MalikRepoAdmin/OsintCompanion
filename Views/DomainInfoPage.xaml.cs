using System.Net.Http.Json;
using System.Net;
using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;
using Microsoft.Maui.Graphics;

namespace OsintCompanion.Views
{
    public partial class DomainInfoPage : ContentPage
    {
        private bool _isDrawerOpen = false;
        private const uint AnimationDuration = 300; 
        private const double DrawerWidth = 250; 
        
        private string _activeTabId = string.Empty;
        private readonly Dictionary<string, (IView Content, Color Color)> _drawerConfigs;

        // This is the CONSTRUCTOR
        public DomainInfoPage()
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
        

        private readonly HttpClient _httpClient = new();
        private readonly Dictionary<string, IpApiResponse> _cache = new(); // local cache

        // Logic For Button "LookUp" and Enter Key Pressed
        private async void OnLookupClicked(object sender, EventArgs e)
        {
            await StartLookup(); // ✅ handler must be async void
        }

        private async void OnEnterPressed(object sender, EventArgs e)
        {
            await StartLookup(); // ✅ same here
        }

        // Action executed when LookUp Button clicked or Enter Key Pressed
        private async Task StartLookup()
        {
            string query = InputEntry.Text?.Trim().ToLower() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(query))
            {
                await DisplayAlert("Input Required", "Please enter a domain or IP address.", "OK");
                return;
            }

            LoadingIndicator.IsVisible = LoadingIndicator.IsRunning = true;
            ResultContainer.Children.Clear();

            try
            {
                if (_cache.ContainsKey(query))
                {
                    ShowResult(_cache[query]);
                    return;
                }

                IpApiResponse? result = null;

                // Detect if input is IP
                bool isIp = IPAddress.TryParse(query, out _);

                if (isIp)
                    result = await QueryIpFallbackAsync(query);
                else
                {
                    // resolve domain -> IP first
                    try
                    {
                        var addresses = await Dns.GetHostAddressesAsync(query);
                        if (addresses.Length > 0)
                            result = await QueryIpFallbackAsync(addresses[0].ToString());
                        else
                            Console.WriteLine("No addresses resolved.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"DNS resolution failed: {ex.Message}");
                        // Try querying directly with the domain name
                        result = await QueryIpFallbackAsync(query);
                    }

                }

                if (result != null)
                {
                    _cache[query] = result; // store in cache
                    ShowResult(result);
                }
                else
                    await DisplayAlert("Error", "No data found or API limit reached.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lookup Failed", ex.Message, "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = LoadingIndicator.IsRunning = false;
            }
        }

        // Tries ipapi.co first, then ip-api.com
        private async Task<IpApiResponse?> QueryIpFallbackAsync(string query)
        {
            try
            {
                string url = $"https://ipapi.co/{query}/json/";
                var result = await _httpClient.GetFromJsonAsync<IpApiResponse>(url);
                if (result != null && result.ip != null) return result;
                var json = await _httpClient.GetStringAsync(url);
                await DisplayAlert("DEBUG", json.Substring(0, Math.Min(json.Length, 500)), "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ipapi.co error: {ex.Message}");
            }

            try
            {
                string url = $"https://ip-api.com/json/{query}";
                var alt = await _httpClient.GetFromJsonAsync<IpApiResponse>(url);
                if (alt != null && alt.query != null)
                {
                    // Normalize property names for consistent UI
                    alt.ip = alt.query;
                    alt.country_name ??= alt.country;
                    alt.org ??= alt.isp;
                    return alt;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ip-api.com error: {ex.Message}");
            }


            return null;
        }

        private void ShowResult(IpApiResponse data)
        {
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

            AddLine("IP", data.ip);
            AddLine("City", data.city);
            AddLine("Region", data.region);
            AddLine("Country", data.country_name);
            AddLine("ISP / Org", data.org);
            AddLine("ASN", data.asn);
            AddLine("Latitude", data.latitude?.ToString());
            AddLine("Longitude", data.longitude?.ToString());
            AddLine("Timezone", data.timezone);
        }
    }

    public class IpApiResponse
    {
        public string? ip { get; set; }
        public string? query { get; set; }
        public string? city { get; set; }
        public string? region { get; set; }
        public string? country_name { get; set; }
        public string? country { get; set; }
        public string? org { get; set; }
        public string? isp { get; set; }
        public string? asn { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public string? timezone { get; set; }
    }
}
