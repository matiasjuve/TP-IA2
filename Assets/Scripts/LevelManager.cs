using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public float gameTime;

    public Text timerMinutes;
    public Text timerSeconds;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        gameTime -= Time.deltaTime;

        string minutes = Mathf.Floor(gameTime / 60).ToString("00");
        string seconds = (gameTime % 60).ToString("00");

        timerMinutes.text = minutes;
        timerSeconds.text = seconds;
    }
}
