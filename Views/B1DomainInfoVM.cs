// B1DomainInfoVM.cs

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using OsintCompanion.Models;
using OsintCompanion.Services;

// A minimal INotifyPropertyChanged base class (replace with your actual one)
public class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value)) return false;
        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

namespace OsintCompanion.Views
{
    // B1DomainInfoVM now handles the business logic
    public class B1DomainInfoVM : ObservableObject
    {
        private readonly IDomainInfoService _domainService;


        // To pass value for DisplayAlert() UI Element
        public Action<string, string, string>? DisplayAlertAction { get; set; }

        // View Properties
        private string _inputQuery = string.Empty;
        public string InputQuery
        {
            get => _inputQuery;
            set => SetProperty(ref _inputQuery, value);
        }

        private IpApiResponse? _lookupResult;
        public IpApiResponse? LookupResult
        {
            get => _lookupResult;
            set => SetProperty(ref _lookupResult, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }


        private string _userApiKey = string.Empty;
        // This property will be bound to the AdvancedAPI Entry in the XAML
        public string UserApiKey
        {
            get => _userApiKey;
            set => SetProperty(ref _userApiKey, value);
        }
        
        // Command
        public ICommand LookupCommand { get; }

        public B1DomainInfoVM(IDomainInfoService domainService)
        {
            _domainService = domainService;
            LookupCommand = new Command(async () => await ExecuteLookupAsync());
        }

        private async Task ExecuteLookupAsync()
        {
            if (IsBusy) return;

            string query = InputQuery.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                // In MVVM, you often use messaging or a dialog service for alerts
                // For simplicity here, we'll assume the View handles the alert if the result is null
                return; 
            }

            IsBusy = true;
            LookupResult = null; // Clear previous result

            try
            {
                // Service call replaces all the moved logic
                // 🚨 Pass the UserApiKey to the Service 🚨
                LookupResult = await _domainService.LookupDomainOrIpAsync(query, UserApiKey);

                if (LookupResult == null)
                {
                    // Call the Action instead for DisplayAlert
                    DisplayAlertAction?.Invoke("Error", "No data found or API limit reached.", "OK");
                }
            }
            catch (Exception ex)
            {
                // Real apps log this. For simplicity:
                Console.WriteLine($"Lookup failed: {ex.Message}");
                // Handle error state or set an error message property

                // Call the Action for the exception 
                DisplayAlertAction?.Invoke("Lookup Failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
            
        }
    }
}