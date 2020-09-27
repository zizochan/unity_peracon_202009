using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverSceneController : MonoBehaviour
{
    TextController scoreText;

    // Start is called before the first frame update
    void Start()
    {
        FadeManager.FadeIn(1f);

        SetInstances();
        SetScoreText();
    }

    void SetInstances()
    {
        scoreText = GameObject.Find("ScoreText").GetComponent<TextController>();
    }

    void SetScoreText()
    {
        string text = "SCORE: " + Data.tmpScore.ToString();
        scoreText.SetText(text);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
