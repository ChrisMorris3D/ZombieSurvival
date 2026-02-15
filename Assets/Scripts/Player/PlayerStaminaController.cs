using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrispyCube
{
    public class PlayerStaminaController : MonoBehaviour
    {
        public FloatVariable PlayerStamina;

        [Header("CONSTANTS")]
        public FloatVariable MaxStamina;

        [Header("RATES")]
        public FloatVariable staminaIncreaseRate;
        public FloatVariable staminaDecreaseRate;

        void Start()
        {
            ResetStaminaToMax();
        }

        void ResetStaminaToMax()
        {
            PlayerStamina.Value = MaxStamina.Value;
        }

        public void UpdateStamina(bool isDecreasing)
        {
            float staminaChangeAmount;

            if (isDecreasing)
            {
                staminaChangeAmount = staminaDecreaseRate.Value;
            }
            else
            {
                staminaChangeAmount = staminaIncreaseRate.Value;
            }

            float updatedHealth = PlayerStamina.Value + staminaChangeAmount;
            float clampedHealth = Mathf.Clamp(updatedHealth, 0, MaxStamina.Value);
            PlayerStamina.Value = clampedHealth;
        }
    }
}