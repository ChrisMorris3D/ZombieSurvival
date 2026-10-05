using System;
using UnityEngine;

namespace CrispyCube
{
    [DisallowMultipleComponent]
    public class RoundTimer : MonoBehaviour
    {
        [Header("ROUND TIMER")]
        [Min(0)]
        [SerializeField] int roundDurationSeconds = 60;
        [SerializeField] IntegerVariable roundTimer;

        [Header("ROUND OVER")]
        [SerializeField] GameObject roundOverUI;

        float timeRemaining;
        int displayedSeconds;
        bool roundIsOver;

        public event Action RoundEnded;
        public bool IsRoundOver => roundIsOver;

        void Start()
        {
            StartRound();
        }

        void Update()
        {
            if (roundIsOver)
            {
                return;
            }

            timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
            int secondsRemaining = Mathf.CeilToInt(timeRemaining);

            if (secondsRemaining != displayedSeconds)
            {
                displayedSeconds = secondsRemaining;
                UpdateTimerValue();
            }

            if (timeRemaining <= 0f)
            {
                EndRound();
            }
        }

        public void StartRound()
        {
            timeRemaining = roundDurationSeconds;
            displayedSeconds = roundDurationSeconds;
            roundIsOver = false;

            UpdateTimerValue();

            if (roundOverUI != null)
            {
                roundOverUI.SetActive(false);
            }

            if (timeRemaining <= 0f)
            {
                EndRound();
            }
        }

        void EndRound()
        {
            roundIsOver = true;
            displayedSeconds = 0;
            UpdateTimerValue();

            if (roundOverUI != null)
            {
                roundOverUI.SetActive(true);
            }

            RoundEnded?.Invoke();
        }

        void UpdateTimerValue()
        {
            if (roundTimer != null)
            {
                roundTimer.SetValue(displayedSeconds);
            }
        }

        void OnValidate()
        {
            roundDurationSeconds = Mathf.Max(0, roundDurationSeconds);
        }
    }
}
