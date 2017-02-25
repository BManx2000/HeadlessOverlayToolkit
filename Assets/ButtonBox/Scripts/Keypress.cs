using System;
using System.Collections.Generic;

[Serializable]
public class Keypress {
    public int Key;
    public int Scancode;
    public bool Extended;

    public Keypress(int key, int scancode, bool extended) {
        Key = key;
        Scancode = scancode;
        Extended = extended;
    }

    public override string ToString() {
        return Key.ToString();
    }
}
