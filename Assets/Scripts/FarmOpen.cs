using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Match3 {
    public class FarmOpen : MonoBehaviour
    {
        [SerializeField] GameObject barrel, house, farm;
        [SerializeField] GameObject p1Barrel, p2House, p3Farm;
        [SerializeField] GameObject butBarrel, butHouse, butFarm;
        [SerializeField] GameObject explosion;
        ScoreManager scoreManager;
        int hightscore;
        string done50, done120, done200;

        void Start() {
            scoreManager = GetComponent<ScoreManager>();
            hightscore = PlayerPrefs.GetInt("highscore", 0);

            done50 = "done50";
            done120 = "done120";
            done200 = "done200";

            done50 = PlayerPrefs.GetString(done50);
            done120 = PlayerPrefs.GetString(done120);
            done200 = PlayerPrefs.GetString(done200);

            CheckedOpen();
            CheckScoreToOpenFarm();
        }

        public void CheckedOpen() {
            if(PlayerPrefs.GetString(done50) == "50")
            {
                butBarrel.SetActive(false);
                barrel.SetActive(true);
            }

            if(PlayerPrefs.GetString(done120) == "120")
                {
                    butHouse.SetActive(false);
                    house.SetActive(true);
                    
                }

            if(PlayerPrefs.GetString(done200) == "200")
                {
                    butFarm.SetActive(false);
                    farm.SetActive(true);
                }

        }
        void VFX(GameObject explosion, GameObject positionObject) {
            Instantiate(explosion, positionObject.transform.position, positionObject.transform.rotation);
        }


        void CheckScoreToOpenFarm() {
            if (hightscore >= 50) {
            butBarrel.GetComponent<Button>().enabled = true;
            } 
            if (hightscore >= 120) {
            butHouse.GetComponent<Button>().enabled = true;    
            } 
            
            if (hightscore >= 200) {
            butFarm.GetComponent<Button>().enabled = true;
                }
        }  

        public void ClickOpenBarrel() {
            barrel.SetActive(true);
            VFX(explosion, p1Barrel);
            PlayerPrefs.SetString(done50, "50");
            PlayerPrefs.Save();
        }      

        public void ClickOpenHouse() {
            house.SetActive(true);
            VFX(explosion, p2House);
            PlayerPrefs.SetString(done120, "120");
            PlayerPrefs.Save();
        }

        public void ClickOpenFarm() {
            farm.SetActive(true);
            VFX(explosion, p3Farm);
            PlayerPrefs.SetString(done200, "200");
            PlayerPrefs.Save();
        }
    }
}
