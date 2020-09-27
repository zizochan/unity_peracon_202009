using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    float score;
    float restCreateEnemyTime;

    GameObject enemyObject;
    TextController scoreText;
    GameObject fukidashiDestroyParticle;
    PlayerController playerController;

    private AudioSource audioSource;
    public AudioClip bombSound;
    public AudioClip cureSound;
    float soundVolume = 1f;

    const int INITIAL_ENEMY_COUNT = 1;
    const float INITIAL_REST_CREATE_ENEMY_TIME = 2.0f;
    const float MIN_CREATE_ENEMY_TIME = 0.9f;
    const float CREATE_ENEMY_TIME_REDUCE_TIME = 0.003f;
    const float DEFAULT_ENEMY_Y_POSITION = 9.3f;
    const float DEFAULT_ENEMY_SCALE = 1.3f;
    const float FUKIDASHI_ROTATION_RANGE = 20f;
    const int GOOD_MESSAGE_SCORE = 2;
    const int GOOD_MESSAGE_PER_COUNT = 5;

    int enemyCreatePosition = 0; // どの位置で作ったか
    int fukidashiCreateCount = 0;

    public CameraShake shake;

    // Start is called before the first frame update
    void Start()
    {
        Data.ChangeStatus(Data.STATUS_INITIAL);

        FadeManager.FadeIn(0.5f);

        SetInstances();
        ResetGame();

        CreateInitialEnemies();

        Data.ChangeStatus(Data.STATUS_PLAY);
    }

    void ResetGame()
    {
        score = 0f;
        Data.tmpScore = 0;

        restCreateEnemyTime = INITIAL_REST_CREATE_ENEMY_TIME;

        enemyCreatePosition = 0;
        fukidashiCreateCount = 0;
    }

    void SetInstances()
    {
        enemyObject = (GameObject)Resources.Load("Fukidashi");
        scoreText = GameObject.Find("ScoreText").GetComponent<TextController>();
        fukidashiDestroyParticle = (GameObject)Resources.Load("FukidashiDestroyParticle");
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.deltaTime;

        if (!Data.IsGamePlay())
        {
            return;
        }

        PastTime(deltaTime);
        AddScore(deltaTime);
    }

    void PastTime(float deltaTime)
    {
        restCreateEnemyTime -= deltaTime;
        if (restCreateEnemyTime < 0f)
        {
            CreateEnemy();
        }
    }

    void CreateEnemy()
    {
        string objectName = "enemy";
        float posX = ChoiceEnemyX();
        float posY = DEFAULT_ENEMY_Y_POSITION;
        float scaleX = DEFAULT_ENEMY_SCALE * 1.1f; // 少し横幅にしておく
        float scaleY = DEFAULT_ENEMY_SCALE;
        float rotation = ChoiceEnemyRotation();

        GameObject enemy = CreateObject(enemyObject, posX, posY, objectName, scaleX, scaleY, rotation);
        FukidashiController enemyController = enemy.GetComponent<FukidashiController>();
        int fukidashiType = ChoiceFukidashiType();

        enemyController.Initialize(fukidashiType, fukidashiCreateCount);
        fukidashiCreateCount++;

        ResetRestCreateEnemyTime();
    }

    void ResetRestCreateEnemyTime()
    {
        restCreateEnemyTime = GetNextRestCreateEnemyTime();
    }

    float GetNextRestCreateEnemyTime()
    {
        float nextTime = INITIAL_REST_CREATE_ENEMY_TIME;
        float minTime = MIN_CREATE_ENEMY_TIME;

        nextTime -= (GetTrueScore() * CREATE_ENEMY_TIME_REDUCE_TIME);

        if (nextTime < minTime)
        {
            nextTime = minTime;
        }

        return nextTime;
    }

    private GameObject CreateObject(GameObject baseObject, float x, float y, string objectName = null, float scaleX = 1f, float scaleY = 1f, float rotation = 0f)
    {
        Quaternion rote = Quaternion.Euler(0f, 0f, rotation);
        GameObject instance = (GameObject)Instantiate(baseObject, new Vector2(x, y), rote);
        instance.transform.localScale = new Vector2(scaleX, scaleY);

        if (objectName != null)
        {
            instance.name = objectName;
        }

        return instance;
    }

    float ChoiceEnemyX()
    {
        float minX;
        float maxX;

        switch (enemyCreatePosition)
        {
            // 中央に作る
            case 1:
                minX = -3f;
                maxX = 3f;
                enemyCreatePosition = 2;
                break;
            // 右に作る
            case 2:
                minX = 4f;
                maxX = 8f;
                enemyCreatePosition = 0;
                break;
            // 左に作る
            default:
                minX = -8f;
                maxX = -4f;
                enemyCreatePosition = 1;
                break;
        }

        return Random.Range(minX, maxX);
    }

    float ChoiceEnemyRotation()
    {
        return Random.Range(FUKIDASHI_ROTATION_RANGE * -1, FUKIDASHI_ROTATION_RANGE);
    }

    public void ExecuteGameOver()
    {
        if (Data.CHEAT_MUTEKI)
        {
            return;
        }

        Data.ChangeStatus(Data.STATUS_GAMEOVER);

        PlaySound(bombSound);
        shake.Shake(3f, 0.5f);

        Data.tmpScore = GetTrueScore();

        FadeManager.FadeOut("GameOverScene", 2f);
    }

    void PlaySound(AudioClip clip)
    {
        if (Data.IsBgmStop())
        {
            return;
        }

        audioSource.PlayOneShot(clip, soundVolume);
    }

    void AddScore(float deltaTime)
    {
        score += deltaTime;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        string text = "SCORE: " + GetTrueScore().ToString();
        scoreText.SetText(text);
    }

    int GetTrueScore()
    {
        return (int)(score * 5);
    }

    void CreateInitialEnemies()
    {

        for (int i = 0; i < INITIAL_ENEMY_COUNT; i++)
        {
            CreateEnemy();
        }

        // ２体目はすぐに作る
        restCreateEnemyTime = INITIAL_REST_CREATE_ENEMY_TIME * 0.5f;
    }

    public void GetGoodMessage()
    {
        score += GOOD_MESSAGE_SCORE;
        UpdateScoreText();

        PlaySound(cureSound);

        Vector2 particlePosition = GetPlayerPosition();
        CreateFukidashiDestroyParticle(particlePosition.x, particlePosition.y);
    }

    int ChoiceFukidashiType()
    {
        if (fukidashiCreateCount == 0)
        {
            return Data.MESSAGE_TYPE_BAD;
        }

        if (fukidashiCreateCount % GOOD_MESSAGE_PER_COUNT == 0)
        {
            return Data.MESSAGE_TYPE_GOOD;
        }

        return Data.MESSAGE_TYPE_BAD;
    }

    Vector2 GetPlayerPosition()
    {
        return playerController.transform.localPosition;
    }

    void CreateFukidashiDestroyParticle(float posX, float posY)
    {
        string name = "FukidashiDestroyParticle";
        GameObject particle = CreateObject(fukidashiDestroyParticle, posX, posY, name);

        particle.GetComponent<FukidashiDestroyParticleController>().Initialze(posX, posY);
    }
}
