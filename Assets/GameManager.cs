using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    Camera mainCam;
    void Start()
    {
        mainCam = Camera.main;
    }
    [SerializeField] GameObject handPrefab, saniPrefab;
    
    [Serializable] class MyFloatEvent : UnityEvent<float>{};
    [Serializable] class MyIntEvent : UnityEvent<int>{};
    [SerializeField] UnityEvent OnHeal, OnDamage;
    [SerializeField] MyFloatEvent OnEnergyUpdated;
    [SerializeField] MyIntEvent OnQuadrantQueued;

    float energy = 1f;
    [SerializeField] float energyPerShot, energyPerSec;
    void Update()
    {
        // add new energy
        energy += Time.deltaTime * energyPerSec;
        energy = Mathf.Min(energy, 1);

        // find shots and queue them up
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 viewportPos = mainCam.ScreenToViewportPoint(Input.mousePosition);
            int quadrant;
            if (viewportPos.y >= viewportPos.x) {
                if (viewportPos.y >= 1-viewportPos.x) {
                    quadrant = 0;
                } else {
                    quadrant = 1;
                }
            } else {
                if (viewportPos.y >= 1-viewportPos.x) {
                    quadrant = 3;
                } else {
                    quadrant = 2;
                }
            }
            Shoot(quadrant);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.K))
        {
            Shoot(0);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.H))
        {
            Shoot(1);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.J))
        {
            Shoot(2);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.L))
        {
            Shoot(3);
        }

        // if (Input.GetKeyDown(KeyCode.Q))
        // {

        // }

        OnEnergyUpdated.Invoke(energy);
    }
    void Shoot(int quadrant)
    {
        if (energy < energyPerShot) {
            return;
        }

        if (quadrant < 2) {
            Heal();
        } else { 
            Damage();
        }
        energy -= energyPerShot;
    }
    [SerializeField] Texture2D idle, happy, sad;
    [SerializeField] SkinnedMeshRenderer skinnedMesh;
    void Heal()
    {
        TemporarilySwapFace(happy, .85f);
        OnHeal.Invoke();
    }
    void Damage()
    {
        TemporarilySwapFace(sad, .85f);
        OnDamage.Invoke();
    }
    IEnumerator faceSwapRoutine;
    void TemporarilySwapFace(Texture2D tempFace, float duration)
    {
        if (faceSwapRoutine != null) {
            StopCoroutine(faceSwapRoutine);
        }
        IEnumerator Swap()
        {
            float tEnd = Time.time + duration;
            skinnedMesh.materials[1].mainTexture = tempFace;
            while (Time.time < tEnd)
            {
                yield return null;
            }
            skinnedMesh.materials[1].mainTexture = idle;
            faceSwapRoutine = null;
        }
        StartCoroutine(faceSwapRoutine = Swap());
    }
}