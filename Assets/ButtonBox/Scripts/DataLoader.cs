using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Valve.VR;

public class DataLoader {
    public static Settings Settings;
    public static Profile Profile;
    public static List<Profile> profiles;

    public static List<HandComponents> HandComponents = new List<HandComponents>();
    public static List<OverlayLoader> Overlays = new List<OverlayLoader>();

    public static OverlayLoader ProfileOverlay;
    public static List<OverlayLoader> ProfileOverlayList;
    public static bool SelectingProfile = false;

    private static int PixelDensity = 1500;

	static DataLoader () {
        StreamReader r = new StreamReader(DirectoryInfo.SettingsPath);
        Settings = JsonUtility.FromJson<Settings>(r.ReadToEnd());
        r.Close();

        profiles = new List<Profile>();
        foreach (string directory in Directory.GetDirectories(DirectoryInfo.FolderPath)) {
            r = new StreamReader(Path.Combine(directory, DirectoryInfo.ProfileName));
            Profile profile = JsonUtility.FromJson<Profile>(r.ReadToEnd());
            r.Close();
            profile.directory = directory;
            profiles.Add(profile);
        }

        profiles.Sort();
        Profile defaultProfile = profiles[0];
        foreach(Profile profile in profiles) {
            if(profile.isDefault) {
                defaultProfile = profile;
                break;
            }
        }
        SetProfile(defaultProfile);

        CreateProfileOverlay();
	}

    public static void SetProfile(Profile profile) {
        Profile = profile;
        foreach(OverlayLoader overlay in Overlays) {
            GameObject.Destroy(overlay.GetComponent<HOTK_Overlay>().OverlayTexture);
            GameObject.Destroy(overlay.gameObject);
        }
        Overlays.Clear();
        foreach (ButtonGrid grid in Profile.grids) {
            GameObject overlay = (GameObject)GameObject.Instantiate(Resources.Load("Overlay"));
            overlay.GetComponent<OverlayLoader>().ButtonGrid = grid;
            overlay.GetComponent<HOTK_Overlay>().OverlayTexture = new Texture2D(16, 16);
            foreach(GridButton button in grid.buttons) {
                button.currentKeypress = button.defaultKeypress;
            }
        }
        foreach(Profile p in profiles) {
            p.isDefault = p == profile;
            SaveProfile(p);
        }
    }

    private static void CreateProfileOverlay() {
        Profile profile = new Profile();
        profile.name = "Profiles";
        profile.directory = DirectoryInfo.FolderPath;
        profile.grids = new List<ButtonGrid>(1);
        profile.index = -1;

        ButtonGrid grid = new ButtonGrid();
        grid.parent = profile;
        profile.grids.Add(grid);
        grid.name = "Profiles";
        grid.filename = DirectoryInfo.ProfilesTextureName;

        float buttonSize = 0.1f;
        int gridWidth = 4;
        int gridHeight = 2;
        while ((gridWidth * (gridHeight - 1)) < profiles.Count) {
            if (gridWidth == gridHeight - 1) {
                gridWidth++;
            }
            else {
                gridHeight++;
            }
        }

        grid.gridWidth = gridWidth;
        grid.gridHeight = gridHeight;
        grid.width = buttonSize * gridWidth;
        grid.height = buttonSize * gridHeight;
        grid.buttons = new List<GridButton>(profiles.Count);
        grid.x = 0;
        grid.y = 1;
        grid.z = 0;
        grid.pitch = 0;
        grid.yaw = 0;
        grid.roll = 0;
        grid.alpha = 1;

        GridButton resetButton = new GridButton();
        resetButton.x = 0;
        resetButton.y = 0;
        resetButton.width = gridWidth/2;
        resetButton.height = 1;
        resetButton.text = "Reset Seated Position";
        resetButton.fontSize = 0.0125;
        resetButton.keypress = null;
        grid.buttons.Add(resetButton);

        GridButton laserButton = new GridButton();
        laserButton.x = 2;
        laserButton.y = 0;
        laserButton.width = gridWidth - (gridWidth/2);
        laserButton.height = 1;
        laserButton.text = "Reset Seated Position";
        laserButton.fontSize = 0.0125;
        laserButton.keypress = null;
        grid.buttons.Add(laserButton);

        int x = 0;
        int y = 1;
        foreach(Profile p in profiles) {
            GridButton button = new GridButton();
            button.x = x;
            button.y = y;
            button.width = 1;
            button.height = 1;
            button.text = p.name;
            button.fontSize = 0.01;
            button.keypress = null;
            grid.buttons.Add(button);

            x++;
            if(x >= gridWidth) {
                x = 0;
                y++;
            }
        }
        grid.RebuildSpatialIndex();

        GameObject overlay = (GameObject)GameObject.Instantiate(Resources.Load("Overlay"));
        OverlayLoader loader = overlay.GetComponent<OverlayLoader>();
        overlay.GetComponent<HOTK_Overlay>().OverlayTexture = new Texture2D(16, 16);
        loader.ButtonGrid = grid;
        ProfileOverlay = loader;
        ProfileOverlayList = new List<OverlayLoader>(1);
        ProfileOverlayList.Add(ProfileOverlay);
    }

    public static void SavePositions() {
        SaveProfile(Profile);
    }

    private static void SaveProfile(Profile p) {
        StreamWriter w = new StreamWriter(Path.Combine(p.directory, DirectoryInfo.ProfileName), false);
        w.Write(JsonUtility.ToJson(p));
        w.Close();
    }

    public static void StartSelectingProfile(Vector3 overlayPosition, Vector3 headPosition) {
        SelectingProfile = true;
        ProfileOverlay.transform.position = overlayPosition;
        ProfileOverlay.transform.LookAt(ProfileOverlay.transform.position - (headPosition - ProfileOverlay.transform.position));
    }

    public static void StopSelectingProfile(int index) {
        SelectingProfile = false;
        if(index == 0) {
            OpenVR.System.ResetSeatedZeroPose();
        }
        else if(index == 1) {
            Settings.Lasermode = !Settings.Lasermode;
            StreamWriter w = new StreamWriter(DirectoryInfo.SettingsPath, false);
            w.Write(JsonUtility.ToJson(Settings));
            w.Close();
        }
        else if(index > 0) {
            SetProfile(profiles[index-2]);
        }
    }
}
