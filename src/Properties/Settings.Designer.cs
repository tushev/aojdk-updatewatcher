﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AJ_UpdateWatcher.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.8.1.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool isConfigured {
            get {
                return ((bool)(this["isConfigured"]));
            }
            set {
                this["isConfigured"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool FirstSilentRunMessageHasBeenDisplayed {
            get {
                return ((bool)(this["FirstSilentRunMessageHasBeenDisplayed"]));
            }
            set {
                this["FirstSilentRunMessageHasBeenDisplayed"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool CheckForSelfUpdates {
            get {
                return ((bool)(this["CheckForSelfUpdates"]));
            }
            set {
                this["CheckForSelfUpdates"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://api.github.com/repos/tushev/aojdk-updatewatcher/releases/latest")]
        public string SelfUpdatesAPI {
            get {
                return ((string)(this["SelfUpdatesAPI"]));
            }
            set {
                this["SelfUpdatesAPI"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2.0.1.0")]
        public string SettingsFormatVersion {
            get {
                return ((string)(this["SettingsFormatVersion"]));
            }
            set {
                this["SettingsFormatVersion"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool SettingsUpgradeComplete_from_V1 {
            get {
                return ((bool)(this["SettingsUpgradeComplete_from_V1"]));
            }
            set {
                this["SettingsUpgradeComplete_from_V1"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int UserConfigurableSetting_MaxConcurrentMSIDownloads {
            get {
                return ((int)(this["UserConfigurableSetting_MaxConcurrentMSIDownloads"]));
            }
            set {
                this["UserConfigurableSetting_MaxConcurrentMSIDownloads"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int ErrorsEncounteredSinceLastConfigurationWindowOpened {
            get {
                return ((int)(this["ErrorsEncounteredSinceLastConfigurationWindowOpened"]));
            }
            set {
                this["ErrorsEncounteredSinceLastConfigurationWindowOpened"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public int UserConfigurableSetting_WarnIfNUpdateChecksResultedInErrors {
            get {
                return ((int)(this["UserConfigurableSetting_WarnIfNUpdateChecksResultedInErrors"]));
            }
            set {
                this["UserConfigurableSetting_WarnIfNUpdateChecksResultedInErrors"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UserConfigurableSetting_UseRandomPrefixForDownloadedMSIs {
            get {
                return ((bool)(this["UserConfigurableSetting_UseRandomPrefixForDownloadedMSIs"]));
            }
            set {
                this["UserConfigurableSetting_UseRandomPrefixForDownloadedMSIs"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SettingsUpgradeRequired {
            get {
                return ((bool)(this["SettingsUpgradeRequired"]));
            }
            set {
                this["SettingsUpgradeRequired"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string UserConfigurableSetting_PostUpdateCommand {
            get {
                return ((string)(this["UserConfigurableSetting_PostUpdateCommand"]));
            }
            set {
                this["UserConfigurableSetting_PostUpdateCommand"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UserConfigurableSetting_UseTrayNotificationForBackgroundCheck {
            get {
                return ((bool)(this["UserConfigurableSetting_UseTrayNotificationForBackgroundCheck"]));
            }
            set {
                this["UserConfigurableSetting_UseTrayNotificationForBackgroundCheck"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool JavaHomeWarningHasBeenDisplayed {
            get {
                return ((bool)(this["JavaHomeWarningHasBeenDisplayed"]));
            }
            set {
                this["JavaHomeWarningHasBeenDisplayed"] = value;
            }
        }
    }
}
