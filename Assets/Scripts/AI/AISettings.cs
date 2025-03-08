using UnityEngine;

namespace AI
{
    public class AISettings 
    {
        #region Inspector Variables

        //Минимальное время принятия решения (милисек)
        public int MinDecisionDelay = 1000;
        //Максимальное время принятия решения (милисек)
        public int MaxDecisionDelay = 4000;
        //Время имитации прицеливания (милисек)
        public int AimingTime = 2000;
        //Радиус поиска соседних шашек
        public float NeighborRadius = 2.0f;
        //Вес группировки своих шашек
        public float FriendlyGroupWeight = 0.7f;
        //Вес близости к вражеским шашкам
        public float EnemyProximityWeight = 1.2f;
        //Вес линии огня (потенциальных попаданий)
        public float LineOfFireWeight = 2.0f;
        //Вес позиции ближе к центру доски
        public float BoardCenterWeight = 0.5f;
        //Штраф за близость к краям доски
        public float EdgePenaltyWeight = 0.3f;

        #endregion
    }
}