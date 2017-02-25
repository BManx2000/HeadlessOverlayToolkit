using System;
using System.Collections.Generic;

[Serializable]
public class KeyCombo {
    public List<Keypress> keypresses;

    public KeyCombo(List<Keypress> presses) {
        System.Diagnostics.Debug.Assert(presses.Count > 0);
        keypresses = presses;
    }

    public override string ToString() {
        string result = "";
        foreach (Keypress k in keypresses) {
            result = result + k.ToString() + " + ";
        }
        result = result.Substring(0, result.Length - 3);
        return result;
    }
}
