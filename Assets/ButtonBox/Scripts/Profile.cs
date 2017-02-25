using System;
using System.Collections.Generic;

[Serializable]
public class Profile : UnityEngine.ISerializationCallbackReceiver, IComparable<Profile> {
    public string name;
    public string directory;
    public List<ButtonGrid> grids;
    public int index;
    public bool isDefault;

    public Profile() {

    }

    public override string ToString() {
        return this.name;
    }

    public void OnBeforeSerialize() {
    }

    public void OnAfterDeserialize() {
        foreach(ButtonGrid grid in grids) {
            grid.parent = this;
        }
    }

    public int CompareTo(Profile other) {
        return index - other.index;
    }
}
