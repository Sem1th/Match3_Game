using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Match3 {
    public class MainMenu : MonoBehaviour
    {

        public static MainMenu instance;

        [SerializeField] Button bt_Play;
        [SerializeField] Button bt_Info;
        [SerializeField] Button bt_Close;
        [SerializeField] GameObject gameMenuPanel, win, lose;
        [SerializeField] GameObject match3Game, score;
        [SerializeField] string currentSceneName;
        UnityEvent m_MyEvent = new UnityEvent();
       


        void Awake() {
            instance = this;
        }
        
        public void PlayGame() {
            gameMenuPanel.SetActive(false);
            match3Game.SetActive(true);
            score.SetActive(true);
        }

        public void Restart() {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }

        public void QuitGame() {

        /* #if UNITY_STANDALONE_WIN
            EditorApplication.isPaused = true;
        #endif */
            Application.Quit();
        }

        public void Win() {
            score.SetActive(false);
            win.SetActive(true);
        }

        public void Lose() {
            score.SetActive(false);
            lose.SetActive(true);
        }

        public void Pause() {
            Time.timeScale = 0f;
        }

        public void Resume() {
           Time.timeScale = 1f; 
        }

    }
}
