using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullLight : MonoBehaviour {

    public Animator SkullAnimator;
    public GameObject SkullFX;
    public ParticleSystem BloodFX;
    public GameObject BloodPool;
    public float BloodAppearTime = 1f;

    float bloodTimer = 0f;
    bool isOn = false;

    private void Start() { 
        SkullFX.SetActive(false);
        ParticleSystem.EmissionModule bloodEmission = BloodFX.emission;
        bloodEmission.enabled = false;
        
        BloodPool.transform.localScale = Vector3.zero;
    }

    void Update() {
        if(isOn) {
            if(bloodTimer <= BloodAppearTime) {
                bloodTimer += Time.deltaTime;
                BloodPool.transform.localScale = Vector3.one*Mathf.Clamp01(bloodTimer/BloodAppearTime);
            }
        }
    }

    public void TurnOn() {
        SkullAnimator.SetBool("Awake", true);
        ParticleSystem.EmissionModule bloodEmission = BloodFX.emission;
        bloodEmission.enabled = true;
        SkullFX.SetActive(true);
        isOn = true;
    }

    public bool IsTurnedOn() {
        return isOn;
    }

    public void Reset() {
        SkullAnimator.SetBool("Awake",false);
        SkullAnimator.Play("Asleep");
        SkullFX.SetActive(false);
        ParticleSystem.EmissionModule bloodEmission = BloodFX.emission;
        bloodEmission.enabled = false;
        BloodPool.transform.localScale = Vector3.zero;
        isOn = false;

    }

}
