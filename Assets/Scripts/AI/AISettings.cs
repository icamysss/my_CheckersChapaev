using UnityEngine;

namespace AI
{
    public class AISettings 
    {
        #region Inspector Variables

        public int TimeAfterCamSetPositionMS = 500;
        //Минимальное время принятия решения (милисек)
        public int MinDecisionDelayMS = 1000;
        //Максимальное время принятия решения (милисек)
        public int MaxDecisionDelayMS = 4000;
        //Время имитации прицеливания (милисек)
        public int AimingTimeMS = 2000;
        //Радиус поиска соседних шашек
        public float NeighborRadius = 2.0f;
        //Вес группировки своих шашек
        public float FriendlyGroupWeight = 0.3f;
        //Вес близости к вражеским шашкам
        public float EnemyProximityWeight = 1.2f;
        //Вес линии огня (потенциальных попаданий)
        public float LineOfFireWeight = 2.0f;
        //Вес позиции ближе к центру доски
        public float BoardCenterWeight = 0.5f;
        //Штраф за близость к краям доски
        public float EdgePenaltyWeight = 0.3f;
        // Штраф за своих на линии
        public float LineOfFriendlyFireWeight = 2.5f;

        #endregion
        
        public AISettings(bool random = true)
        {
            if (!random) return;
            
            AimingTimeMS = Random.Range(0 , 3500);
            NeighborRadius *= Random.Range(0.8f, 1.2f);
            FriendlyGroupWeight *= Random.Range(0.8f, 1.2f);
            EnemyProximityWeight *= Random.Range(0.8f, 1.2f);
            LineOfFireWeight *= Random.Range(0.8f, 1.2f);
            BoardCenterWeight *= Random.Range(0.8f, 1.2f);
            EdgePenaltyWeight *= Random.Range(0.8f, 1.2f);
        }
    }
}