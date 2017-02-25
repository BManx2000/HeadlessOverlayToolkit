using System;
using System.Collections.Generic;

[Serializable]
public class ButtonGrid : UnityEngine.ISerializationCallbackReceiver {
    [NonSerialized]
    public Profile parent;
    public string name;
    public string filename;
    public int gridWidth;
    public int gridHeight;
    public double width;
    public double height;
    public double x;
    public double y;
    public double z;
    public double pitch;
    public double yaw;
    public double roll;
    public double alpha;
    public bool outline;
    public bool border;
    public bool locked;
    public List<GridButton> buttons;

    private GridButton[,] SpatialIndex;

    public void RebuildSpatialIndex() {
        this.SpatialIndex = new GridButton[this.gridWidth, this.gridHeight];
        foreach (GridButton button in this.buttons) {
            this.AddButtonToIndex(button);
        }
    }

    public void RemoveButtonFromIndex(GridButton button) {
        for (int x = button.x; x < button.x + button.width; x++) {
            for (int y = button.y; y < button.y + button.height; y++) {
                //Debug.Assert(this.SpatialIndex[x, y] == button);
                this.SpatialIndex[x, y] = null;
            }
        }
    }

    public void AddButtonToIndex(GridButton button) {
        for (int x = button.x; x < button.x + button.width; x++) {
            for (int y = button.y; y < button.y + button.height; y++) {
                //Debug.Assert(this.SpatialIndex[x, y] == null);
                this.SpatialIndex[x, y] = button;
            }
        }
    }

    public GridButton ButtonAtPosition(int x, int y) {
        return this.SpatialIndex[x, y];
    }

    public void OnBeforeSerialize() {
    }

    public void OnAfterDeserialize() {
        this.RebuildSpatialIndex();
    }
}
