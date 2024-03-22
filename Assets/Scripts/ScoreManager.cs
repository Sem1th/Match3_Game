using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3 {
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager instance;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] TMP_Text hightScoreText;
        [SerializeField] TMP_Text movesText;
        [SerializeField] TMP_Text targetScoreText;
        [SerializeField] AudioManager audioManager;
        [SerializeField] GameObject game;
        Match3 match3;
        int score = 0;
        int hightscore = 0;
        int counter;
        int targetScore;
        void Awake() {
            instance = this;
        }
        void Start() {
            match3 = GetComponent<Match3>();
            counter = Random.Range(17, 28);
            targetScore = Random.Range(150, 250);

            hightscore = PlayerPrefs.GetInt("highscore", 0);
            scoreText.text = score.ToString() + " Счет";
            hightScoreText.text = "Лучший счет: " + hightscore.ToString();
            movesText.text = "Ходов: " + counter.ToString();
            targetScoreText.text = "Min счет для победы: " + targetScore.ToString();
        }

        public void AddPoint() {
            score +=1;
            scoreText.text = score.ToString() + " Счет";
            if (hightscore < score)
                PlayerPrefs.SetInt("highscore", score);
        }

        public void CheckerCounter() {
            counter -=1;
            movesText.text = "Ходов: " + counter.ToString();
        }

        public void CounterAddMoves() {
            counter ++;
            movesText.text = "Ходов: " + counter.ToString();
        }
        
        public void CheckingWinOrLose() {
            if (counter <= 0 && score < targetScore) {
                MainMenu.instance.Lose();
                MainMenu.instance.Pause();
                audioManager.PlayLose();
            } else if (counter <= 0 && score >= targetScore) {
                MainMenu.instance.Win();
                MainMenu.instance.Pause();
                audioManager.PlayWin();
            }

            if (score > 100) {
                Match3.instance.SecretActivated();
            }
        }

    }
}