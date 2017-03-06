using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Valve.VR;

public class ActivityMonitor : MonoBehaviour {
    private List<Vector3> Positions;
    private List<float> Speeds;

    private float alpha = 0.1f;

    public static bool Active;

	// Use this for initialization
	void Start () {
        //OpenVR.Compositor.SetTrackingSpace(ETrackingUniverseOrigin.TrackingUniverseSeated);
	}
	
	// Update is called once per frame
	void Update () {
        /*
        if(OpenVR.Compositor.GetTrackingSpace() != ETrackingUniverseOrigin.TrackingUniverseSeated) {
            OpenVR.Compositor.SetTrackingSpace(ETrackingUniverseOrigin.TrackingUniverseSeated);
        }
        */

        if (Positions == null || DataLoader.HandComponents.Count != Positions.Count) {
            Positions = new List<Vector3>(DataLoader.HandComponents.Count);
            Speeds = new List<float>(DataLoader.HandComponents.Count);
            foreach (HandComponents hand in DataLoader.HandComponents) {
                //Positions.Add(hand.Controller.transform.position);
                Positions.Add(hand.Controller.transform.rotation.eulerAngles);
                Speeds.Add(0f);
            }
        }

        float max = 0;
        for(int i=0; i<DataLoader.HandComponents.Count; i++) {
            /*Vector3 pos = DataLoader.HandComponents[i].Controller.transform.position;
            Vector3 vel = pos - Positions[i];
            Positions[i] = pos;
            float speed = vel.magnitude*60;
            */
            Vector3 ang = DataLoader.HandComponents[i].Controller.transform.rotation.eulerAngles;
            Vector3 angVel = ang - Positions[i];
            Positions[i] = ang;
            float speed = angVel.magnitude * 60;
            Speeds[i] = alpha * speed + (1 - alpha) * Speeds[i];
            if(Speeds[i] > max) {
                max = Speeds[i];
            }
        }

        if (!Active) {
            Active = max > 50;
        }
        else {
            Active = max > 1;
        }
    }
}
