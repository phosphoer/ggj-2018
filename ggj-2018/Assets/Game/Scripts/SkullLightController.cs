using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullLightController : MonoBehaviour {

    public List<SkullLight> Lights;

    int current = 0;

    public void EnableNext() {
        current++;
        if(current < Lights.Count) {
            Lights[current].TurnOn();
        }
        else Debug.LogWarning("Trying to enable more lights (" + current + ") than there are (" + Lights.Count + ").");
    }

    public void EnableNumberOf(int num) {
        for(int i=0; i<num && current<Lights.Count;i++) {
            EnableNext();
        }
    }

    public void ResetAll() {
        current = 0;
        for(int i = 0;i < Lights.Count;i++) {
            Lights[i].Reset();
        }
    }
}
