using System;
using System.Collections.Generic;

public enum ButtonType {
    Normal,
    TwoDirectionRotary,
    MultiPositionRotary
}

[Serializable]
public class GridButton {
    public int x;
    public int y;
    public int width;
    public int height;
    public string text;
    public double fontSize;
    public ButtonType buttonType;
    public double rotaryAngle;
    public KeyCombo keypress;
    public KeyCombo cwKeypress;
    public KeyCombo ccwKeypress;
    public List<KeyCombo> multiKeypresses;
    public int defaultKeypress;
}
