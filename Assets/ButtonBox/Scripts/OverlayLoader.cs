using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Valve.VR;

public class OverlayLoader : MonoBehaviour {
    public ButtonGrid ButtonGrid;
    private float Width;
    private float Height;

    private HOTK_Overlay Overlay;

    // Use this for initialization
    void Start() {
        if (this != DataLoader.ProfileOverlay) {
            DataLoader.Overlays.Add(this);
        }

        if (ButtonGrid.alpha > 0) {
            WWW file = new WWW("file:///" + Path.Combine(ButtonGrid.parent.directory, ButtonGrid.filename));
            file.LoadImageIntoTexture((Texture2D)this.GetComponent<HOTK_Overlay>().OverlayTexture);
        }

        transform.position = new Vector3((float)ButtonGrid.x, (float)ButtonGrid.y, (float)ButtonGrid.z);
        transform.rotation = Quaternion.Euler((float)ButtonGrid.pitch, (float)ButtonGrid.yaw, (float)ButtonGrid.roll);

        Overlay = GetComponent<HOTK_Overlay>();
        Overlay.Scale = (float)ButtonGrid.width;
        Overlay.Alpha = (float)ButtonGrid.alpha;
        this.Width = (float)ButtonGrid.width;
        this.Height = (float)ButtonGrid.height;
    }

    public Vector3 RelativePosition(Vector3 point, Vector3 direction, out float dist) {
        dist = 0;
        if (DataLoader.Settings.Lasermode) {
            float d = Vector3.Dot((transform.position - point), transform.forward) / (Vector3.Dot(direction, transform.forward));
            dist = d;
            if (d < 0) {
                return new Vector3(-1, -1, -1);
            }
            point = point + direction * d;
        }
        Vector3 relPos = point - transform.position;
        relPos = Quaternion.Inverse(transform.rotation) * relPos;
        if(!DataLoader.Settings.Lasermode) {
            dist = relPos.z;
        }
        relPos = new Vector3((relPos.x + this.Width / 2) / this.Width, (-relPos.y + this.Height / 2) / this.Height, relPos.z);
        return relPos;
    }

    public bool PositionInRange(Vector3 relPos, float limit) {
        return relPos.x >= 0 && relPos.x < 1 && relPos.y >= 0 && relPos.y < 1 && Mathf.Abs(relPos.z) <= limit;
    }

    public GridButton PointedButton(Vector3 relPos) {
        return this.ButtonGrid.ButtonAtPosition((int)(relPos.x * ButtonGrid.gridWidth), (int)(relPos.y * ButtonGrid.gridHeight));
    }

    public void ShowButtonOverlay(GridButton button, HOTK_Overlay overlay) {
        overlay.Scale = Mathf.Min(this.Width * button.width / this.ButtonGrid.gridWidth, this.Height * button.height / this.ButtonGrid.gridHeight);
        overlay.transform.position = transform.position + transform.rotation * new Vector3(-this.Width / 2 + (this.Width / this.ButtonGrid.gridWidth) * (button.x + button.width * 0.5f), -(-this.Height / 2 + (this.Height / this.ButtonGrid.gridHeight) * (button.y + button.height * 0.5f)), 0);
        overlay.transform.rotation = transform.rotation;
        overlay.enabled = true;
    }

    // Update is called once per frame
    void Update() {
        if(ButtonGrid.alpha == 0) {
            Overlay.enabled = false;
            return;
        }
        if (this == DataLoader.ProfileOverlay) {
            Overlay.enabled = DataLoader.SelectingProfile;
        }
        else {
            Overlay.enabled = ActivityMonitor.Active && !DataLoader.SelectingProfile;
        }
    }

    public void SavePosition() {
        ButtonGrid.x = transform.position.x;
        ButtonGrid.y = transform.position.y;
        ButtonGrid.z = transform.position.z;
        Vector3 rotation = transform.rotation.eulerAngles;
        ButtonGrid.pitch = rotation.x;
        ButtonGrid.yaw = rotation.y;
        ButtonGrid.roll = rotation.z;
        DataLoader.SavePositions();
    }
}
