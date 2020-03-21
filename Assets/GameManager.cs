using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    Camera mainCam;
    Color heartCol, skinCol;
    void Awake()
    {
        mainCam = Camera.main;
        heartCol = hearts[0].color;
        skinCol = skinnedMesh.materials[0].color;
    }
    public void Begin()
    {
        // energy = 1;
        lives = 3;
        foreach (Image im in hearts) {
            im.color = heartCol;
        }
        skinnedMesh.materials[1].mainTexture = idle;
        skinnedMesh.materials[0].color = skinCol;

        score = 0;
        DisplayScore();
        StartCoroutine(SpawnConstantly());
    }
    [SerializeField] Text scoreText;
    void DisplayScore()
    {
        scoreText.text = $"Score: {score.ToString("N0")}";
    }
    [SerializeField] UnityEvent OnHeal, OnDamage, OnDead;

    // float energy = 0;
    // [SerializeField] float energyPerShot, energyPerSec;
    [SerializeField] Image ammo, ammoBG;
    void Update()
    {
        // add new energy
        // energy += Time.deltaTime * energyPerSec;
        // energy = Mathf.Min(energy, 1);

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
        // ammo.fillAmount = lives>0? energy : 1;
        // ammoBG.color = energy>=energyPerShot? new Color(.3f,.3f,.7f) : new Color(.1f,.1f,.1f);
    }
    [SerializeField] Projectile handPrefab, saniPrefab;
    Queue<Projectile>[] projectiles = new Queue<Projectile>[4] { new Queue<Projectile>(), new Queue<Projectile>(), new Queue<Projectile>(), new Queue<Projectile>() };
    int lastQuadrant = 0;

    readonly float minDelay = .3f, maxDelay = 2f, minSpeed = 1;
    IEnumerator SpawnConstantly()
    {
        void Spawn()
        {
            bool good = UnityEngine.Random.Range(0,100) > 75;

            int quadrant;
            while ((quadrant=UnityEngine.Random.Range(0,4)) == lastQuadrant) {}
            lastQuadrant = quadrant;

            Projectile projectile;
            projectile = Instantiate(good? saniPrefab : handPrefab);
            float speed = minSpeed + Mathf.Log(1+Mathf.Sqrt(.01f*score));
            projectile.Init(quadrant, speed);
            projectiles[quadrant].Enqueue(projectile);
        }
        float tNext = Time.time;
        while (true)
        {
            if (Time.time >= tNext)
            {
                float tDelay = minDelay + (maxDelay-minDelay)/(1+Mathf.Sqrt(.01f*score));
                tNext += tDelay;

                Spawn();
            }
            yield return null;
        }
    }
    int lives = 0;
    [SerializeField] Image[] hearts;
    void OnTriggerEnter(Collider other)
    {
        var touched = projectiles[other.GetComponent<Projectile>().Quadrant].Dequeue();
        if (touched.tag == "Hand") {
            Damage();
        } else if (touched.tag == "Sani") {
            Heal(10);
        }
        touched.Touch();
    }
    void Shoot(int quadrant)
    {
        // if (energy < energyPerShot) {
        //     return;
        // }
        // energy -= energyPerShot;
        if (projectiles[quadrant].Count > 0)
        {
            var smacked = projectiles[quadrant].Dequeue();
            if (smacked.tag == "Hand") {
                Heal((int)(5 * (smacked.transform.position - transform.position).magnitude));
            } else if (smacked.tag == "Sani") {
                Damage();
            }
            smacked.Smack();
        }
    }
    [SerializeField] Texture2D idle, happy, sad, dead;
    [SerializeField] SkinnedMeshRenderer skinnedMesh;
    [SerializeField] Effect pointsPrefab;
    int score;
    void Heal(int points)
    {
        int newScore = score + points;
        newScore = (int)(newScore * 1.2f); // exponential growth hehe
        var effect = Instantiate(pointsPrefab, scoreText.rectTransform);
        effect.GetComponent<Text>().text = $"+{newScore-score}";
        score = newScore;
        DisplayScore();

        TemporarilySwapFace(happy, skinCol, .85f);
        OnHeal.Invoke();
    }
    void Damage()
    {
        lives -= 1;
        hearts[lives].color = new Color(1,1,1,.1f);
        if (lives > 0) {
            TemporarilySwapFace(sad, new Color(1,.4f,.4f), .85f);
            OnDamage.Invoke();
        }
        else {
            GameOver();
        }
    }
    [SerializeField] Text overText;
    void GameOver()
    {
        StopAllCoroutines();
        foreach (var queue in projectiles) {
            while (queue.Count > 0) {
                queue.Dequeue().Smack();;
            }
        }
        skinnedMesh.materials[1].mainTexture = dead;
        skinnedMesh.materials[0].color = new Color(.1f,.5f,.2f);
        overText.text = $"Game Over!\nFinal Score: {score.ToString("N0")}";
        SendScore();
        OnDead.Invoke();
    }
    IEnumerator faceSwapRoutine;
    void TemporarilySwapFace(Texture2D tempFace, Color tempCol, float duration)
    {
        if (faceSwapRoutine != null) {
            StopCoroutine(faceSwapRoutine);
        }
        IEnumerator Swap()
        {
            float tEnd = Time.time + duration;
            skinnedMesh.materials[0].color = tempCol;
            skinnedMesh.materials[1].mainTexture = tempFace;
            while (Time.time < tEnd)
            {
                yield return null;
            }
            skinnedMesh.materials[0].color = skinCol;
            skinnedMesh.materials[1].mainTexture = idle;
            faceSwapRoutine = null;
        }
        StartCoroutine(faceSwapRoutine = Swap());
    }
    public void WashHands()
    {
        Application.OpenURL("https://www.nhs.uk/live-well/healthy-body/best-way-to-wash-your-hands/");
    }
    [SerializeField] Text leaderboard;
    [SerializeField] EcoBuilder.Postman pat;
    static readonly string serverURL = "https://www.ecobuildergame.org/Beta/";
    // static readonly string serverURL = "127.0.0.1/ecobuilder/";
    void SendScore()
    {
        var data = new Dictionary<string, string>() {
            { "score", score.ToString() },
            { "check", EcoBuilder.Postman.Encrypt(score.ToString()) },
            { "__address__", serverURL+"corona.php" },
        };
        pat.Post(data, (b,s)=> leaderboard.text = s);
    }
}