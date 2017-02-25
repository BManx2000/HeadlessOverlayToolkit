using UnityEngine;
using System.Collections;

public class HandComponents {
    public GameObject Controller;
    public HandBillboard Billboard;

    public HandComponents(GameObject controller, HandBillboard billboard) {
        this.Controller = controller;
        this.Billboard = billboard;
    }
}