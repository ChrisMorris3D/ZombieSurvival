using UnityEngine;

namespace CrispyCube
{
    public class GameManager : MonoBehaviour
    {
        public RoundTimer roundTimer;

        public void Start()
        {
           roundTimer.StartRound(); 
        }
    }
}
