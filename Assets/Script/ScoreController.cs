using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour {

    public static ScoreController instance;

    private Vector3 position = new Vector3(0,0.4f,-18);
	public GameObject table;
	public Rigidbody sphere;
	private int[] score;
	public Text score1;
	public Text score2;
    private AudioSource victory;
    public int winningScore;
    public GameObject startButton;
    private TouchableButtonController startButtonController;
    public bool isGameStarted;
    public GameObject ball;

	// Use this for initialization
	void Start () {
        if (instance == null) {
            instance = this;
        }
        victory = GetComponent<AudioSource>();
        startButtonController = startButton.GetComponent<TouchableButtonController>();
        score = new int[] { 0, 0 };
        score1.text = score[0].ToString();
        score2.text = score[1].ToString();
        isGameStarted = false;
        ball = null;
    }
	
	// Update is called once per frame
	void Update () {
        if (startButtonController.isTouched()) {
            startButtonController.desactive();
            start();
        }
        if (winCondition()) {
            int winner = getWinner();
            if (winner == 0){
                score1.text = "Winner";
            }
            else if (winner == 1) {
                score2.text = "Winner";
            }
            isGameStarted = false;
            startButtonController.active();
        }
        if (ball == null && !startButtonController.isActive()) {
            StartCoroutine(instanciateNewBall());
        }
	}

    private int getWinner(){
        if (score[0] > score[1]){
            return 0;
        }
        else {
            return 1;
        }
    }

    private bool winCondition(){
        return Mathf.Max(score) >= winningScore;
    }

    public IEnumerator instanciateNewBall(){
        yield return new WaitForSeconds(0.8f);
        if (isGameStarted && !ball)
        {
            Rigidbody newball = Instantiate(sphere, position, Quaternion.identity);
            newball.gameObject.transform.parent = table.transform;
            ball = newball.gameObject;
        }
    }

    public void playVictory() {
        victory.Play();
    }

    public void addScorePlayer1() {
        score[1]++;
        score2.text = score[1].ToString();
    }

    public void addScorePlayer2(){
        score[0]++;
        score1.text = score[0].ToString();
    }

    public void start() {
        score = new int[] { 0, 0 };
        score1.text = score[0].ToString();
        score2.text = score[1].ToString();
        isGameStarted = true;
        StartCoroutine(instanciateNewBall());
        startButtonController.desactive();
    }
}

