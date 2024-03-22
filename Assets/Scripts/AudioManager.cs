using UnityEngine;

namespace Match3 {
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour {
        private static AudioManager instance;
        [SerializeField] AudioClip click;
        [SerializeField] AudioClip deselect;
        [SerializeField] AudioClip match;
        [SerializeField] AudioClip noMatch;
        [SerializeField] AudioClip woosh;
        [SerializeField] AudioClip pop;
        [SerializeField] AudioClip hint;
        [SerializeField] AudioClip bonus;
        [SerializeField] AudioClip win;
        [SerializeField] AudioClip lose;
        [SerializeField] AudioSource audioSource;

        public static AudioManager GetInstance() {
            if(instance == null) {
                instance =  new AudioManager();
            }
            return instance;
        }
        void Awake(){
            GetInstance();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }
        void OnValidate() {
            
        }

        public void PlayClick() => audioSource.PlayOneShot(click);
        public void PlayDeselect() => audioSource.PlayOneShot(deselect);
        public void PlayMatch() => audioSource.PlayOneShot(match);
        public void PlayNoMatch() => audioSource.PlayOneShot(noMatch);
        public void PlayWoosh() => audioSource.PlayOneShot(woosh);
        public void PlayPop() {
            Debug.Log(audioSource);
            audioSource.PlayOneShot(pop);

        } 
        public void PlayHint() => audioSource.PlayOneShot(hint);
        public void PlayBonus() => audioSource.PlayOneShot(bonus);
        public void PlayWin() => audioSource.PlayOneShot(win);
        public void PlayLose() => audioSource.PlayOneShot(lose);

        /* void PlayRandomPitch(AudioClip audioClip) {
            audioSource.pitch = Random.Range(0.5f, 1.1f);
            audioSource.PlayOneShot(audioClip);
            audioSource.pitch = 1f;
        } */
        
    }
}
