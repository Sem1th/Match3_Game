using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3 {
    public class HintManager : MonoBehaviour
    {
        public static HintManager instance;
        [SerializeField] float hintDelay = 25f;
        [SerializeField] GameObject hintParticle;
        bool isHintActive = false;
        float lastInputTime = 0f;
        private int numberOfGemsToMatch;

        void Awake()
        {
            instance = this;
        }
        
        void Update()
        {
            if (Time.time - lastInputTime > hintDelay && !isHintActive)
            {
                DoHint();
            }
        }

        void DoHint() {
            isHintActive = true;
            ResetInputTime();
        }

        public void ResetInputTime()
        {
            lastInputTime = Time.time;
            
        }
    }
}