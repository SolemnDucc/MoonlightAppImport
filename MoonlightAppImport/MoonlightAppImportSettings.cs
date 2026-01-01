using MoonlightAppImport.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MoonlightAppImport
{
    public class MoonlightAppImportSettings : ObservableObject
    {
        private string _moonlightPath = string.Empty;
        private string _sunshineHost = string.Empty;
        private string _sunshineUsername = string.Empty;
        private bool _isApollo = false;
        private bool _skipCertificateValidation = false;
        private bool _pingHost = true;

        private SecureString _sunshinePassword = new SecureString();
        private string _encryptedSunshinePassword = string.Empty;

        private SecureString _vibepolloApiKey = new SecureString();
        private string _encryptedVibepolloApiKey = string.Empty;

        public string MoonlightPath { get => _moonlightPath; set => SetValue(ref _moonlightPath, value); }
        public string SunshineHost { get => _sunshineHost; set => SetValue(ref _sunshineHost, value); }
        public string SunshineUsername { get => _sunshineUsername; set => SetValue(ref _sunshineUsername, value); }
        public bool IsApollo { get => _isApollo; set => SetValue(ref _isApollo, value); }
        public bool SkipCertificateValidation { get => _skipCertificateValidation; set => SetValue(ref _skipCertificateValidation, value); }
        public bool PingHost { get => _pingHost; set => SetValue(ref _pingHost, value); }

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
            set
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
            set
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

            // Check if the sunshine host is valid
            bool result = IPValidator.ValidateAndResolve(Settings.SunshineHost);
            if (!result)
                errors.Add("- The Sunshine host address was invalid! Could be \"192.168.1.69\" or \"localhost\".");

            // Check if the moonlight path is valid
            Settings.MoonlightPath = Settings.MoonlightPath.Trim().Trim('"');
            result = File.Exists(Settings.MoonlightPath) && Path.GetFileName(Settings.MoonlightPath).Equals("Moonlight.exe", StringComparison.OrdinalIgnoreCase);
            if (!result)
                errors.Add("- The Moonlight path was invalid! Must point to a \"Moonlight.exe\".");

            return errors.Count == 0;
        }
    }
}