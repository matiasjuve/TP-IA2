using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class LevelManager : MonoBehaviour
{
    //public static LevelManager Instance;
    public float gameTime;

    public Text timerMinutes;
    public Text timerSeconds;

    public Text top3;
    public Text kda;
    public Text victims;

    public List<Agent> players;
    public GameObject stats;
    public GameObject timer;

    public List<Tuple<string, int>> top3killers = new List<Tuple<string, int>>();

    public Tuple<string, float> kdaTuple = Tuple.Create("",0f);
    public List<string> names;

    void Start()
    {
        top3.text = "";
        kda.text = "";
        victims.text = "";
        GetTop3();
        GetKds();
        GetKillList();
        
        //Instance = this;
        
    }

    void FixedUpdate()
    {
        gameTime -= Time.deltaTime;

        string minutes = Mathf.Floor(gameTime / 60).ToString("00");
        string seconds = (gameTime % 60).ToString("00");

        timerMinutes.text = minutes;
        timerSeconds.text = seconds;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            stats.SetActive(true);
            timer.SetActive(false);
            
            /*foreach (var item in GetTop3())
            {
                Debug.Log(item.Item1 + "" + item.Item2);
            }*/
            //names = GetTop3()
        }
    }

    public void GetTop3()
    {
        var Top3PlayerWithMostKills = players.Select(x => Tuple.Create(x.user, x.kills))
            .OrderBy(x => x.Item2)
            .Reverse()
            .Take(3);

        foreach (var item in Top3PlayerWithMostKills)
        {
            top3.text +=item.Item1 + "    " + item.Item2.ToString() + "\r\n";
            top3killers.Add(item);
        }
    }
    public void GetKds()
    {
        var kdapositivos = players.Aggregate(new List<Tuple<string,float>>(), (x, y) =>
        {
            if (y.Deaths != 0)
            {
                if (y.kills / y.Deaths >= 1)
                {
                    x.Add(Tuple.Create(y.user, (float)(y.kills / y.Deaths)));
                }
            }
            else
            {
                x.Add(Tuple.Create(y.user, (float)y.kills));
            }
            return x;
        });

        foreach (var item in kdapositivos)
        {
            kda.text += item.Item1 + "    " + item.Item2.ToString() + "\r\n";
        }

    }
    public void GetKillList()
    {
        var victimsList = players.Select(x => x.victims).SelectMany(x => x).Where(x => x.Item1 == top3killers.First().Item1);

        victims.text += victimsList.First().Item1 + "Killed These Players /r/n/r/n";

        foreach (var item in victimsList)
        {
            victims.text += item.Item2.user + "/r/n";
        }
    }
}
