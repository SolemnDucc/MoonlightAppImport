using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace MoonlightAppImport
{
    /// <summary>
    /// Provides an attached property to enable MVVM-compatible, secure data binding
    /// for <see cref="PasswordBox"/> controls without exposing the password as plain text.
    /// </summary>
    public static class PasswordBoxHelper
    {
        /// <summary>
        /// Gets or sets the bindable <see cref="SecureString"/> password for a <see cref="PasswordBox"/>.
        /// Attach this property to a PasswordBox to enable two-way binding with a ViewModel property.
        /// </summary>
        public static readonly DependencyProperty SecurePasswordProperty =
            DependencyProperty.RegisterAttached(
                "SecurePassword",
                typeof(SecureString),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSecurePasswordChanged));

        /// <summary>
        /// Internal flag to prevent re-entrant updates when the PasswordBox itself triggers a change.
        /// </summary>
        private static readonly DependencyProperty _isUpdatingProperty =
            DependencyProperty.RegisterAttached(
                "_IsUpdating",
                typeof(bool),
                typeof(PasswordBoxHelper));

        /// <summary>Gets the attached <see cref="SecureString"/> value from a <see cref="PasswordBox"/>.</summary>
        public static SecureString GetSecurePassword(DependencyObject dp)
            => (SecureString)dp.GetValue(SecurePasswordProperty);

        /// <summary>Sets the attached <see cref="SecureString"/> value on a <see cref="PasswordBox"/>.</summary>
        public static void SetSecurePassword(DependencyObject dp, SecureString value)
            => dp.SetValue(SecurePasswordProperty, value);

        /// <summary>
        /// Called when the bound SecurePassword property changes (e.g. ViewModel resets the value).
        /// Updates the PasswordBox UI without triggering a re-entrant loop.
        /// </summary>
        private static void OnSecurePasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is PasswordBox passwordBox)
            {
                // Unsubscribe to avoid duplicate event handlers on re-attach
                passwordBox.PasswordChanged -= PasswordChanged;

                // Only update the UI if the change did not originate from the PasswordBox itself
                if (!(bool)passwordBox.GetValue(_isUpdatingProperty))
                {
                    var secureString = e.NewValue as SecureString;

                    // Clear current password and re-set only if a non-empty SecureString is provided
                    passwordBox.Password = secureString != null ? ConvertToUnsecureString(secureString) : string.Empty;
                }

                passwordBox.PasswordChanged += PasswordChanged;
            }
        }

        /// <summary>
        /// Handles the PasswordChanged event of the PasswordBox.
        /// Pushes the new <see cref="SecurePassword"/> back to the bound ViewModel property.
        /// </summary>
        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                // Set guard flag to prevent OnSecurePasswordChanged from overwriting during this update
                passwordBox.SetValue(_isUpdatingProperty, true);
                SetSecurePassword(passwordBox, passwordBox.SecurePassword.Copy());
                passwordBox.SetValue(_isUpdatingProperty, false);
            }
        }

        /// <summary>
        /// Converts a <see cref="SecureString"/> to a plain <see cref="string"/> for initializing
        /// the PasswordBox UI. The result is only kept in memory for the duration of the call.
        /// </summary>
        private static string ConvertToUnsecureString(SecureString secureString)
        {
            if (secureString == null || secureString.Length == 0)
                return string.Empty;

            var ptr = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secureString);
            try
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }
    }
}