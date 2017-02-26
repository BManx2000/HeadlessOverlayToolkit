using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using Valve.VR;

public class HandBillboard : MonoBehaviour {
    public static float LaserAngle = 60;

    public Texture2D OverlayTexture;
    public Texture2D OverlayTexturePressed;
    public Texture2D BallTexture;
    public Texture2D LaserTexture;

    public GameObject ButtonOverlayObject;
    private HOTK_Overlay ButtonOverlay;

    private HOTK_Overlay OwnOverlay;

    public GameObject HandOverlayObject;
    private HOTK_Overlay HandOverlay;

    private GameObject Hand;
    private GameObject Head;

    private HOTK_TrackedDevice Device;

    private GridButton CurrentButton = null;

    private Vector3 RotaryDirection;
    private Vector3 RotaryOriginalUp;
    private float RotationSteps;
    private int RotaryPosition;

    private OverlayLoader MovingOverlay = null;
    private Vector3 MovingOriginalPosition;
    private Quaternion MovingOriginalRotation;
    private Vector3 MovingOriginalOverlayPosition;
    private Quaternion MovingOriginalOverlayRotation;
    private bool applyRollDeadzone;

    private int profileIndex;

    bool lastLaserMode;

    // Use this for initialization
    void Start () {
        lastLaserMode = DataLoader.Settings.Lasermode;

        this.Head = GameObject.Find("Camera (head)");

        HOTK_Overlay overlay = GetComponent<HOTK_Overlay>();
        OwnOverlay = overlay;
        if(overlay.AnchorDevice == HOTK_Overlay.AttachmentDevice.LeftController) {
            this.Hand = GameObject.Find("Controller (left)");
        }
        else if(overlay.AnchorDevice == HOTK_Overlay.AttachmentDevice.RightController) {
            this.Hand = GameObject.Find("Controller (right)");
        }
        else {
            Debug.LogError("Hand billboard on wrong object");
            Debug.Assert(false);
        }
        Device = Hand.GetComponent<HOTK_TrackedDevice>();

        ButtonOverlay = ButtonOverlayObject.GetComponent<HOTK_Overlay>();
        HandOverlay = HandOverlayObject.GetComponent<HOTK_Overlay>();

        HandComponents components = new HandComponents(this.Hand, this);
        DataLoader.HandComponents.Add(components);

        UpdateLaserMode();
	}

    private void UpdateLaserMode() {
        lastLaserMode = DataLoader.Settings.Lasermode;

        if (lastLaserMode) {
            OwnOverlay.OverlayTexture = LaserTexture;
            OwnOverlay.Alpha = 0.5f;
            OwnOverlay.Scale = 0.002f;
            OwnOverlay.AnchorPoint = HOTK_Overlay.AttachmentPoint.Center;
            SetLaserLength(0.1f);
        }
        else {
            OwnOverlay.OverlayTexture = BallTexture;
            OwnOverlay.Alpha = 1;
            OwnOverlay.Scale = 0.025f;
            OwnOverlay.AnchorPoint = HOTK_Overlay.AttachmentPoint.FlatAbove;
            OwnOverlay.UvOffset.z = 1;
            OwnOverlay.AnchorOffset = Vector3.zero;
        }
    }

    public Vector3 LaserDirection() {
        return Mathf.Cos(Mathf.Deg2Rad * LaserAngle) * Hand.transform.forward + Mathf.Sin(Mathf.Deg2Rad * LaserAngle) * -Hand.transform.up;
    }

    public Vector3 LaserUpDirection() {
        return Mathf.Sin(Mathf.Deg2Rad * LaserAngle) * Hand.transform.forward + Mathf.Cos(Mathf.Deg2Rad * LaserAngle) * Hand.transform.up;
    }

    public Vector3 PointingDirection() {
        return DataLoader.Settings.Lasermode ? LaserDirection() : Hand.transform.forward;
    }

    public Vector3 PointingUpDirection() {
        return DataLoader.Settings.Lasermode ? LaserUpDirection() : Hand.transform.up;
    }

    public float AngleBetween(Vector3 a, Vector3 b, Vector3 n) {
        a = Vector3.ProjectOnPlane(a, n);
        b = Vector3.ProjectOnPlane(b, n);
        return Mathf.Rad2Deg*Mathf.Atan2(Vector3.Dot(Vector3.Cross(b, a), n), Vector3.Dot(a, b));
    }

    void SetLaserLength(float length) {
        OwnOverlay.UvOffset.z = OwnOverlay.Scale / length;
        Quaternion laserRot = Quaternion.Euler(new Vector3(LaserAngle, 0, 0));
        Vector3 rotAxis = laserRot * Vector3.up;
        OwnOverlay.AnchorOffset = rotAxis * length/2;
    }

    // Update is called once per frame
    void Update () {
        if(DataLoader.Settings.Lasermode != lastLaserMode) {
            UpdateLaserMode();
        }

        if(!Device.IsValid) {
            OwnOverlay.enabled = false;
            HandOverlay.enabled = false;
            ButtonOverlay.enabled = false;
            return;
        }

        OwnOverlay.enabled = ActivityMonitor.Active && !DataLoader.Profile.hidePointer;
        HandOverlay.enabled = true;

        if (DataLoader.Settings.Lasermode) {
            transform.position = Hand.transform.position;
            Quaternion laserRot = Quaternion.Euler(new Vector3(LaserAngle, 0, 0));
            Vector3 rotAxis = laserRot * Vector3.up;
            
            transform.localRotation = laserRot;
            Vector3 localHeadLaser = Hand.transform.InverseTransformPoint(Head.transform.position);
            localHeadLaser = Quaternion.Inverse(laserRot) * localHeadLaser;
            float angleLaser = Mathf.Atan2(localHeadLaser.y, localHeadLaser.x) * 180 / Mathf.PI - 90;
            laserRot = Quaternion.AngleAxis(angleLaser, rotAxis) * laserRot;
            transform.localRotation = laserRot;
        }
        else {
            transform.position = Hand.transform.position;
            transform.LookAt(Head.transform.position);
            Quaternion look = transform.rotation;
            transform.rotation = Quaternion.Inverse(Hand.transform.rotation);
            transform.rotation = transform.rotation * look * Quaternion.AngleAxis(180, Vector3.up);
        }

        Vector3 localHead = Hand.transform.InverseTransformPoint(Head.transform.position);
        float angle = Mathf.Atan2(localHead.y, localHead.x) * 180 / Mathf.PI - 90;

        //HandOverlayObject.transform.position = Hand.transform.position;
        Vector3 euler = HandOverlayObject.transform.localRotation.eulerAngles;
        euler.y = angle;
        HandOverlayObject.transform.localRotation = Quaternion.Euler(euler);

        if (ActivityMonitor.Active) {
            UpdateDraggingOverlay();
            UpdateOverlays();
        }
    }

    void UpdateDraggingOverlay() {
        if (MovingOverlay != null) {
            Quaternion rotation = Hand.transform.rotation * Quaternion.Inverse(MovingOriginalRotation);
            Vector3 unRolled = (rotation * MovingOriginalOverlayRotation).eulerAngles;
            float rollDeadzone = 15;
            if (!applyRollDeadzone && (unRolled.z <= rollDeadzone || unRolled.z >= 360 - rollDeadzone)) {
                applyRollDeadzone = true;
            }
            if (applyRollDeadzone) {
                if (unRolled.z < 180) {
                    if (unRolled.z <= rollDeadzone) {
                        unRolled.z = 0;
                    }
                    else {
                        unRolled.z -= rollDeadzone;
                    }
                }
                else {
                    if (unRolled.z >= 360 - rollDeadzone) {
                        unRolled.z = 0;
                    }
                    else {
                        unRolled.z += rollDeadzone;
                    }
                }
            }
            rotation = Quaternion.Euler(unRolled);
            MovingOverlay.transform.rotation = rotation;
            Vector3 originalOffset = MovingOriginalOverlayPosition - MovingOriginalPosition;
            Vector3 offset = (rotation * Quaternion.Inverse(MovingOriginalOverlayRotation)) * originalOffset;
            MovingOverlay.transform.position = MovingOriginalOverlayPosition + BallPosition() - MovingOriginalPosition - originalOffset + offset;

            if (PressedUp(SteamVR_Controller.ButtonMask.Grip)) {
                MovingOverlay.SavePosition();
                MovingOverlay = null;
                applyRollDeadzone = false;
            }
        }
    }

    void UpdateOverlays() {
        if(PressedDown(SteamVR_Controller.ButtonMask.ApplicationMenu)) {
            DataLoader.StartSelectingProfile(DataLoader.Settings.Lasermode ? Hand.transform.position + 0.2f*LaserDirection() : BallPosition(), Head.transform.position);
        }
        if(PressedUp(SteamVR_Controller.ButtonMask.ApplicationMenu) && DataLoader.SelectingProfile) {
            DataLoader.StopSelectingProfile(profileIndex);
        }
        bool profileSet = false;

        if (CurrentButton == null) {
            bool showOverlay = false;
            OverlayLoader closestOverlay = null;
            Vector3 closestPos = new Vector3(0, 0, 1000);
            float closestDist = 1000;
            List<OverlayLoader> overlays = DataLoader.SelectingProfile ? DataLoader.ProfileOverlayList : DataLoader.Overlays;
            foreach (OverlayLoader overlay in overlays) {
                float dist;
                Vector3 relPos = overlay.RelativePosition(DataLoader.Settings.Lasermode ? Hand.transform.position : BallPosition(),
                                                          DataLoader.Settings.Lasermode ? LaserDirection() : Vector3.zero,
                                                          out dist);
                if (overlay.PositionInRange(relPos, 0.1f) && dist < closestDist) {
                    closestOverlay = overlay;
                    closestPos = relPos;
                    closestDist = dist;
                }
            }

            if (closestOverlay != null) {
                if (DataLoader.Settings.Lasermode) {
                    SetLaserLength(closestDist);
                }
                GridButton button = closestOverlay.PointedButton(closestPos);
                if (button != null) {
                    showOverlay = true;
                    closestOverlay.ShowButtonOverlay(button, ButtonOverlay);
                    if(!DataLoader.SelectingProfile && PressedDown(SteamVR_Controller.ButtonMask.Trigger)) {
                        CurrentButton = button;
                        if(button.buttonType == ButtonType.Normal) {
                            KeyboardInput.Down(button.keypress);
                        }
                        else {
                            RotaryDirection = PointingDirection();
                            RotaryOriginalUp = PointingUpDirection();
                        }
                        ButtonOverlay.OverlayTexture = OverlayTexturePressed;
                    }
                    if(DataLoader.SelectingProfile) {
                        profileIndex = closestOverlay.ButtonGrid.buttons.IndexOf(button);
                        profileSet = true;
                    }
                }
                if (!DataLoader.SelectingProfile && !closestOverlay.ButtonGrid.locked && PressedDown(SteamVR_Controller.ButtonMask.Grip)) {
                    MovingOverlay = closestOverlay;
                    MovingOriginalPosition = BallPosition();
                    MovingOriginalRotation = Hand.transform.rotation;
                    MovingOriginalOverlayPosition = closestOverlay.transform.position;
                    MovingOriginalOverlayRotation = closestOverlay.transform.rotation;
                }
            }
            else if (DataLoader.Settings.Lasermode) {
                SetLaserLength(0.2f);
            }

            if(!showOverlay) {
                ButtonOverlay.enabled = false;
            }
        }
        else {
            if(PressedUp(SteamVR_Controller.ButtonMask.Trigger)) {
                if(CurrentButton.buttonType == ButtonType.Normal) {
                    KeyboardInput.Up(CurrentButton.keypress);
                }

                CurrentButton = null;
                ButtonOverlay.OverlayTexture = OverlayTexture;
            }
            else if(CurrentButton.buttonType != ButtonType.Normal) {
                float angle = AngleBetween(this.RotaryOriginalUp, PointingUpDirection(), this.RotaryDirection);
                if(Mathf.Abs(angle) >= CurrentButton.rotaryAngle) {
                    float sign = Mathf.Sign(angle);
                    this.RotaryOriginalUp = Quaternion.AngleAxis((float)CurrentButton.rotaryAngle * (-sign), this.RotaryDirection) * this.RotaryOriginalUp;
                    if(CurrentButton.buttonType == ButtonType.TwoDirectionRotary) {
                        KeyCombo keypress = sign > 0 ? CurrentButton.cwKeypress : CurrentButton.ccwKeypress;
                        KeyboardInput.Down(keypress);
                        KeyboardInput.Up(keypress);
                    }
                    else if(CurrentButton.buttonType == ButtonType.MultiPositionRotary) {
                        int nextPosition = CurrentButton.currentKeypress + (int)sign;
                        if(nextPosition >= 0 && nextPosition < CurrentButton.multiKeypresses.Count) {
                            CurrentButton.currentKeypress = nextPosition;
                            KeyCombo keypress = CurrentButton.multiKeypresses[nextPosition];
                            KeyboardInput.Down(keypress);
                            KeyboardInput.Up(keypress);
                        }
                    }
                }
            }
        }

        if(!profileSet && DataLoader.SelectingProfile) {
            profileIndex = -1;
        }
    }

    public bool Pressed(ulong button) {
        return SteamVR_Controller.Input((int)Device.Index).GetPress(button);
    }

    public bool PressedDown(ulong button) {
        return SteamVR_Controller.Input((int)Device.Index).GetPressDown(button);
    }

    public bool PressedUp(ulong button) {
        return SteamVR_Controller.Input((int)Device.Index).GetPressUp(button);
    }

    public Vector3 BallPosition() {
        return transform.position + Hand.transform.forward * 0.05f;
    }
}
