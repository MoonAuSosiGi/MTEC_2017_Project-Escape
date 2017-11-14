using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeForEscape.Util.Effect
{
    /**
        * @brief		OneHitEffect :: 한번 쓰고 사라질 이펙트들
        * @details		애니메이션 이벤트 / 타임 이벤트 형태로 선택
        * @todo         후에 이펙트 풀을 만들어 관리할 것
        * @author		이훈 (MoonAuSosiGi@gmail.com)
        * @date			2017-11-11
        * @file			OneHitEffect.cs
        * @version		0.0.1
       */
    public class OneHitEffect : MonoBehaviour
    {
        #region One Hit Effect INFO --------------------------------------------------------

        /**
         * @brief   이펙트가 어느 시점에서 사라질지 선택할 enum
         */
        public enum EffectDeleteType
        {
            ANIMATION_EVENT = 0,
            TIME_EVENT
        }

        [SerializeField]
        private EffectDeleteType m_type = EffectDeleteType.ANIMATION_EVENT; ///< 현재 타입
        [SerializeField]
        float m_deleteTime = 2.0f; ///< 지정된 값만큼의 시간이 지난 후 삭제된다.

        #region One Hit Effect Property ----------------------------------------------------

        /**
         * @brief   이펙트 삭제 타입에 대한 프로퍼티
         */
        public EffectDeleteType EFFECT_TYPE
        {
            get { return m_type; }
            set { m_type = value; }
        }

        /**
         * @brief   이펙트 삭제 시간에 대한 프로퍼티
         */
        public float EFFECT_DELETE_TIME
        {
            get { return m_deleteTime; }
            set { m_deleteTime = value; }
        }
        #endregion -------------------------------------------------------------------------
        #endregion -------------------------------------------------------------------------

        /**
         * @brief   첫 시작시 이펙트 타입 체크
         */
        void Start()
        {
            if(m_type == EffectDeleteType.TIME_EVENT)
            {
                Invoke("EffectEnd" , m_deleteTime);
            }
        }

        /**
         * @brief   이펙트 오브젝트 삭제
         * @todo    추후 이펙트 풀로 관리시 따로 체크할것
         */
        public void EffectEnd()
        {
            GameObject.Destroy(gameObject);
        }
    }
}