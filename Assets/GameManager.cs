using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    Camera mainCam;
    void Start()
    {
        mainCam = Camera.main;
    }
    [SerializeField] GameObject handPrefab;

    
    [Serializable] class MyFloatEvent : UnityEvent<float>{};
    [Serializable] class MyIntEvent : UnityEvent<int>{};
    [SerializeField] UnityEvent OnHeal, OnDamage;
    [SerializeField] MyFloatEvent OnEnergyUpdated;
    [SerializeField] MyIntEvent OnQuadrantQueued;

    // this queue exists to make sure inputs don't get overridden by others
    Queue<int> shotQuadrants = new Queue<int>();
    // but only up to a certain number of queued shots
    [SerializeField] int maxQueuedShots;

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
                    quadrant = 2;
                } else {
                    quadrant = 3;
                }
            }
            print(quadrant);
            if (shotQuadrants.Count < maxQueuedShots) {
                shotQuadrants.Enqueue(quadrant);
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.K))
        {
            if (shotQuadrants.Count < maxQueuedShots) {
                shotQuadrants.Enqueue(0);
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.H))
        {
            if (shotQuadrants.Count < maxQueuedShots) {
                shotQuadrants.Enqueue(1);
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.J))
        {
            if (shotQuadrants.Count < maxQueuedShots) {
                shotQuadrants.Enqueue(2);
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.L))
        {
            if (shotQuadrants.Count < maxQueuedShots) {
                shotQuadrants.Enqueue(3);
            }
        }

        while (energy > energyPerShot)
        {
            Shoot(shotQuadrants.Dequeue());
            energy -= energyPerShot;
        }
        OnEnergyUpdated.Invoke(energy);
    }
    void Shoot(int quadrant)
    {
        if (quadrant < 2)
        {
            Heal();
        } else { 
            Damage();
        }
    }
    void Heal()
    {
        // TODO: change face
        OnHeal.Invoke();
    }
    void Damage()
    {
        // TODO: change face
        OnDamage.Invoke();
    }
}