using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    Camera mainCam;
    Color heartCol;
    void Start()
    {
        mainCam = Camera.main;
        heartCol = hearts[0].color;
    }
    public void Begin()
    {
        energy = 1;
        lives = 3;
        foreach (Image im in hearts) {
            im.color = heartCol;
        }
        score = 0;
        DisplayScore();
        skinnedMesh.materials[1].mainTexture = idle;
        StartCoroutine(SpawnConstantly());
    }
    [SerializeField] UnityEvent OnHeal, OnDamage, OnDead;

    float energy = 1f;
    [SerializeField] float energyPerShot, energyPerSec;
    [SerializeField] Image ammo, ammoBG;
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
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Spawn();
        }
        ammo.fillAmount = energy;
        ammoBG.color = energy>=energyPerShot? new Color(.3f,.3f,.7f) : new Color(.1f,.1f,.1f);
    }
    [SerializeField] Projectile handPrefab, saniPrefab;
    Queue<Projectile>[] projectiles = new Queue<Projectile>[4] { new Queue<Projectile>(), new Queue<Projectile>(), new Queue<Projectile>(), new Queue<Projectile>() };
    void Spawn()
    {
        bool good = UnityEngine.Random.Range(0,100) > 80;
        int quadrant = UnityEngine.Random.Range(0, 4);
        Projectile projectile;
        if (good)
        {
            projectile = Instantiate(saniPrefab);
        }
        else
        {
            projectile = Instantiate(handPrefab);
        }
        projectile.Init(quadrant);
        projectiles[quadrant].Enqueue(projectile);
    }
    IEnumerator SpawnConstantly()
    {
        float maxDelay = energyPerShot / energyPerSec;
        float tDelay = 2f;
        float tStart = Time.time;
        float tNext = tStart + tDelay;
        while (true)
        {
            if (Time.time > tNext)
            {
                Spawn();
                tNext += tDelay;
                Projectile.speed = 1;
            }
            yield return null;
        }
    }
    int lives = 3;
    [SerializeField] Image[] hearts;
    void OnTriggerEnter(Collider other)
    {
        string otherTag = other.tag;
        var projectile = projectiles[other.GetComponent<Projectile>().Quadrant].Dequeue();
        Destroy(projectile.gameObject);
        if (otherTag == "Hand") {
            Damage();
        } else if (other.tag == "Sani") {
            Heal();
        }
    }
    void Shoot(int quadrant)
    {
        if (energy < energyPerShot) {
            return;
        }
        energy -= energyPerShot;
        if (projectiles[quadrant].Count > 0)
        {
            var shot = projectiles[quadrant].Dequeue();
            string shotTag = shot.tag;
            Destroy(shot.gameObject);
            if (shotTag == "Hand") {
                Heal();
            } else if (shot.tag == "Sani") {
                Damage();
            }
        }
    }
    [SerializeField] Texture2D idle, happy, sad, dead;
    [SerializeField] SkinnedMeshRenderer skinnedMesh;
    int score;
    void Heal()
    {
        TemporarilySwapFace(happy, .85f);
        score += 10;
        score = (int)(score * 1.5f);
        DisplayScore();
        OnHeal.Invoke();
    }
    void Damage()
    {
        lives -= 1;
        hearts[lives].color = Color.grey;
        if (lives > 0) {
            TemporarilySwapFace(sad, .85f);
            OnDamage.Invoke();
        }
    }
    [SerializeField] Text scoreText;
    void DisplayScore()
    {
        scoreText.text = $"Score: {score}";
    }
    void GameOver()
    {
        StopAllCoroutines();
        foreach (var queue in projectiles) {
            while (queue.Count > 0) {
                Destroy(queue.Dequeue().gameObject);
            }
        }
        skinnedMesh.materials[1].mainTexture = dead;
        skinnedMesh.materials[0].color = new Color(.1f,.5f,.2f);
        OnDead.Invoke();
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