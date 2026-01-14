using MoonlightAppImport.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace MoonlightAppImport
{
    public enum ServerType
    {
        Sunshine,
        Apollo,
        Vibepollo
    }

    public class MoonlightAppImportSettings : ObservableObject
    {
        private string _moonlightPath = string.Empty;
        private string _sunshineHost = string.Empty;
        private string _sunshineUsername = string.Empty;
        private ServerType _serverType = ServerType.Sunshine;
        private bool _skipCertificateValidation = true;
        private bool _pingHost = false;
        private bool _isEnabled = false;
        private bool _addMetadata = false;

        private SecureString _sunshinePassword = new SecureString();
        private string _encryptedSunshinePassword = string.Empty;

        private SecureString _vibepolloApiKey = new SecureString();
        private string _encryptedVibepolloApiKey = string.Empty;

        public string MoonlightPath { get => _moonlightPath; set => SetValue(ref _moonlightPath, value); }
        public string SunshineHost { get => _sunshineHost; set => SetValue(ref _sunshineHost, value); }
        public string SunshineUsername { get => _sunshineUsername; set => SetValue(ref _sunshineUsername, value); }
        public ServerType ServerType { get => _serverType; set => SetValue(ref _serverType, value, nameof(ServerType), nameof(IsVibepollo), nameof(IsSunshine)); }
        public bool IsSunshine => (ServerType == ServerType.Sunshine || ServerType == ServerType.Apollo) && IsEnabled;
        public bool IsVibepollo => ServerType == ServerType.Vibepollo && IsEnabled;
        public bool SkipCertificateValidation { get => _skipCertificateValidation; set => SetValue(ref _skipCertificateValidation, value); }
        public bool PingHost { get => _pingHost; set => SetValue(ref _pingHost, value); }
        public bool IsEnabled { get => _isEnabled; set => SetValue(ref _isEnabled, value, nameof(IsEnabled), nameof(IsSunshine), nameof(IsVibepollo)); }
        public bool AddMetadata { get => _addMetadata; set => SetValue(ref _addMetadata, value); }

        [DontSerialize]
        public string VibepolloApiKey
        {
            get => SecureStringToString(_vibepolloApiKey);
            set
            {
                // Update the SecureString
                var secureString = new SecureString();
                if (!string.IsNullOrEmpty(value))
                {
                    foreach (char c in value)
                    {
                        secureString.AppendChar(c);
                    }
                }
                secureString.MakeReadOnly();

                // Store it securely in memory
                _vibepolloApiKey = secureString;

                // Update the encrypted version for serialization
                _encryptedVibepolloApiKey = EncryptPassword(value);

                // Notify property changes
                OnPropertyChanged(nameof(VibepolloApiKey));
            }
        }

        // Property for serialized, encrypted password
        public string EncryptedVibepolloApiKey
        {
            get => _encryptedVibepolloApiKey;
            private set
            {
                if (_encryptedVibepolloApiKey != value)
                {
                    _encryptedVibepolloApiKey = value;

                    // When loaded from serialization, restore the SecureString
                    string decrypted = DecryptPassword(value);
                    var secureString = new SecureString();
                    if (!string.IsNullOrEmpty(decrypted))
                    {
                        foreach (char c in decrypted)
                        {
                            secureString.AppendChar(c);
                        }
                    }
                    secureString.MakeReadOnly();
                    _vibepolloApiKey = secureString;

                    OnPropertyChanged(nameof(EncryptedVibepolloApiKey));
                }
            }
        }

        [DontSerialize]
        public string SunshinePassword
        {
            get => SecureStringToString(_sunshinePassword);
            set
            {
                // Update the SecureString
                var secureString = new SecureString();
                if (!string.IsNullOrEmpty(value))
                {
                    foreach (char c in value)
                    {
                        secureString.AppendChar(c);
                    }
                }
                secureString.MakeReadOnly();

                // Store it securely in memory
                _sunshinePassword = secureString;

                // Update the encrypted version for serialization
                _encryptedSunshinePassword = EncryptPassword(value);

                // Notify property changes
                OnPropertyChanged(nameof(SunshinePassword));
            }
        }

        // Property for serialized, encrypted password
        public string EncryptedSunshinePassword
        {
            get => _encryptedSunshinePassword;
            private set
            {
                if (_encryptedSunshinePassword != value)
                {
                    _encryptedSunshinePassword = value;

                    // When loaded from serialization, restore the SecureString
                    string decrypted = DecryptPassword(value);
                    var secureString = new SecureString();
                    if (!string.IsNullOrEmpty(decrypted))
                    {
                        foreach (char c in decrypted)
                        {
                            secureString.AppendChar(c);
                        }
                    }
                    secureString.MakeReadOnly();
                    _sunshinePassword = secureString;

                    OnPropertyChanged(nameof(EncryptedSunshinePassword));
                }
            }
        }

        #region Helper methods for secure operations
        private string SecureStringToString(SecureString secureString)
        {
            if (secureString == null || secureString.Length == 0)
                return string.Empty;

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        private string EncryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            byte[] plainBytes = Encoding.Unicode.GetBytes(password);
            byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }

        private string DecryptPassword(string encryptedPassword)
        {
            if (string.IsNullOrEmpty(encryptedPassword))
                return string.Empty;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedPassword);
                byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.Unicode.GetString(plainBytes);
            }
            catch
            {
                // Handle decryption errors gracefully
                return string.Empty;
            }
        }
        #endregion
    }

    public class MoonlightAppImportSettingsViewModel : ObservableObject, ISettings
    {
        private readonly MoonlightAppImport plugin;
        private MoonlightAppImportSettings editingClone { get; set; }

        private MoonlightAppImportSettings settings;
        public MoonlightAppImportSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public MoonlightAppImportSettingsViewModel(MoonlightAppImport plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<MoonlightAppImportSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new MoonlightAppImportSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            Settings.MoonlightPath = Settings.MoonlightPath.Trim();
            Settings.SunshinePassword = Settings.SunshinePassword.Trim();
            Settings.VibepolloApiKey = Settings.VibepolloApiKey.Trim();
            Settings.SunshineUsername = Settings.SunshineUsername.Trim();
            Settings.SunshineHost = Settings.SunshineHost.Trim();

            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();

            // If the Addon is not enabled, skip the validation of the settings
            if (!Settings.IsEnabled)
                return true;

            // Check if the moonlight path is valid
            Settings.MoonlightPath = Settings.MoonlightPath.Trim().Trim('"');
            bool result = File.Exists(Settings.MoonlightPath) && Path.GetFileName(Settings.MoonlightPath).Equals("Moonlight.exe", StringComparison.OrdinalIgnoreCase);
            if (!result)
                errors.Add("- The Moonlight path was invalid! Must point to a \"Moonlight.exe\".");

            // Check if the sunshine host is valid
            result = IPValidator.ValidateAndResolve(Settings.SunshineHost);
            if (!result)
                errors.Add("- The Sunshine host address was invalid! Could be \"192.168.1.69\" or \"localhost\".");

            // Check if there is a username/password for the server if it is sunshine or apollo
            if (settings.IsSunshine)
            {
                if (string.IsNullOrEmpty(Settings.SunshineUsername))
                    errors.Add($"- When you choose \"Sunshine\" or \"Apollo\" as server then you need to provide a username.");

                if (string.IsNullOrEmpty(Settings.SunshinePassword))
                    errors.Add($"- When you choose \"Sunshine\" or \"Apollo\" as server then you need to provide a password.");
            }
            // Check if there is an API key if it is vibepollo
            else if (settings.IsVibepollo)
            {
                if (string.IsNullOrEmpty(Settings.VibepolloApiKey))
                    errors.Add($"- When you choose \"Vibepollo\" as server then you need to provide an API Key.");
            }
            
            return errors.Count == 0;
        }
    }
}