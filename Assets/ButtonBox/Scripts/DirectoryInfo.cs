using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class DirectoryInfo {
    public static readonly string FolderName = "Virtual Button Box";
    public static readonly string SettingsName = "settings.json";
    public static readonly string ProfileName = "profile.json";
    public static readonly string ProfilesTextureName = "profiles.png";
    public static readonly string FolderPath;
    public static readonly string SettingsPath;
    public static readonly string ProfilesTexturePath;

    static DirectoryInfo() {
        FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FolderName);
        Directory.CreateDirectory(FolderPath);
        SettingsPath = Path.Combine(FolderPath, SettingsName);
        ProfilesTexturePath = Path.Combine(FolderPath, ProfilesTextureName);
    }
}
