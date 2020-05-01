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
    [SerializeField] Text sidesText, cornersText;
    [SerializeField] GameObject sideArrows, quitButton;
    [SerializeField] bool useSides;
    void Awake()
    {
        mainCam = Camera.main;
        heartCol = hearts[0].color;
        skinCol = skinnedMesh.materials[0].color;

        sidesText.enabled = useSides;
        cornersText.enabled = !useSides;
        sideArrows.SetActive(useSides);
        RectTransform heaRTs = hearts[0].transform.parent.GetComponent<RectTransform>();
        if (useSides)
        {
            mainCam.fieldOfView = 40;
            heaRTs.anchorMin = heaRTs.anchorMax = new Vector2(0,0);
            heaRTs.pivot = new Vector2(0,0);
        }
        else
        {
            mainCam.fieldOfView = 25;
            heaRTs.anchorMin = heaRTs.anchorMax = new Vector2(.5f,0);
            heaRTs.pivot = new Vector2(.5f,0);
        }
#if UNITY_WEBGL
        quitButton.SetActive(false);
#endif
    }
    readonly float minDelay = .3f, maxDelay = 2f, minSpeed = 1;
    public void Begin()
    {
        lives = 3;
        foreach (Image im in hearts) {
            im.color = heartCol;
        }
        skinnedMesh.materials[1].mainTexture = idle;
        skinnedMesh.materials[0].color = skinCol;

        score = 0;
        DisplayScore();
        // if (mode == Difficulty.Easy) {
        //     StartCoroutine(SpawnConstantly(()=>minSpeed + Mathf.Log(1+.01f*Time.time),
        //                                    ()=>minDelay + (maxDelay-minDelay)/(1+.01f*Time.time)));
        // } else if (mode == Difficulty.Hard) {
            StartCoroutine(SpawnConstantly(()=>minSpeed + (float)Math.Log(1+Math.Sqrt(.01*score)),
                                           ()=>minDelay + (maxDelay-minDelay)/(1+(float)Math.Sqrt(.01f*score))));
        // }
        OnBegin.Invoke();
    }
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    [SerializeField] Text scoreText;
    void DisplayScore()
    {
        scoreText.text = $"Score: {score.ToString("N0")}";
    }
    [SerializeField] UnityEvent OnHeal, OnDamage, OnDead, OnBegin;

    void Update()
    {
        if (useSides) {
            ShootSides();
        } else {
            ShootCorners();
        }

        if (lives == 0 && Input.GetKeyDown(KeyCode.Space))
        {
            Begin();
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SendScore();
        }
#endif
    }
    void ShootSides()
    {
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
    }
    void ShootCorners()
    {
        int Corner(Vector2 viewportPos)
        {
            int quadrant;
            if (viewportPos.x >= .5f) {
                if (viewportPos.y >= .5f) {
                    quadrant = 0;
                } else {
                    quadrant = 3;
                }
            } else {
                if (viewportPos.y >= .5f) {
                    quadrant = 1;
                } else {
                    quadrant = 2;
                }
            }
            return quadrant;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Shoot(Corner(mainCam.ScreenToViewportPoint(Input.mousePosition)));
        }
        // get touches if needed
        foreach (var touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                Shoot(Corner(mainCam.ScreenToViewportPoint(touch.position)));
            }
        }
    }
    [SerializeField] Projectile handPrefab, saniPrefab;
    Queue<Projectile>[] projectiles = new Queue<Projectile>[4] { new Queue<Projectile>(), new Queue<Projectile>(), new Queue<Projectile>(), new Queue<Projectile>() };
    int lastQuadrant = 0;

    IEnumerator SpawnConstantly(Func<float> Speed, Func<float> Delay)
    {
        void Spawn()
        {
            bool good = UnityEngine.Random.Range(0,100) > 75;

            int quadrant;
            while ((quadrant=UnityEngine.Random.Range(0,4)) == lastQuadrant) {}
            lastQuadrant = quadrant;

            Projectile projectile;
            projectile = Instantiate(good? saniPrefab : handPrefab);
            float speed = Speed();

            if (useSides) {
                projectile.InitSides(quadrant, speed, mainCam);
            } else {
                projectile.InitCorners(quadrant, speed, mainCam);
            }
            projectiles[quadrant].Enqueue(projectile);
        }
        float tNext = Time.time + 1;
        while (true)
        {
            if (Time.time >= tNext)
            {
                float tDelay = Delay();
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
        if (projectiles[quadrant].Count > 0)
        {
            var smacked = projectiles[quadrant].Dequeue();
            if (smacked.tag == "Hand") {
                // Heal((long)(5 * (smacked.transform.position - transform.position).magnitude));
                var viewportPos = (Vector2)mainCam.WorldToViewportPoint(smacked.transform.position);
                viewportPos -= new Vector2(.5f,.5f);
                Heal((long)(30 * (viewportPos).magnitude));
            } else if (smacked.tag == "Sani") {
                Damage();
            }
            smacked.Smack();
        }
    }
    [SerializeField] Texture2D idle, happy, sad, dead;
    [SerializeField] SkinnedMeshRenderer skinnedMesh;
    [SerializeField] Effect pointsPrefab;
    long score;
    void Heal(long points)
    {
        long newScore = score + points;
        newScore = (long)(newScore * 1.2f); // exponential growth hehe
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

    [SerializeField] Text leaderboard;
    [SerializeField] EcoBuilder.Postman pat;
#if !UNITY_EDITOR
    static readonly string address = "https://www.ecobuildergame.org/Corona/corona.php";
#else
    static readonly string address = "127.0.0.1/corona/corona.php";
#endif
    void SendScore()
    {
        var data = new Dictionary<string, string>() {
            { "score", score.ToString() },
            { "check", EcoBuilder.Postman.Encrypt(score.ToString()) },
            { "__address__", address },
        };
        pat.Post(data, (b,s)=> leaderboard.text = s);
    }
    public void RefreshLeaderboard()
    {
        long foo = -1;
        var data = new Dictionary<string, string>() {
            { "score", foo.ToString() },
            { "check", EcoBuilder.Postman.Encrypt(foo.ToString()) },
            { "__address__", address },
        };
        pat.Post(data, (b,s)=> leaderboard.text = s);
    }
}