﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeForEscape.Object.Weapon.Bullet
{
    /**
    * @brief		Bullet_Plasma :: 플라즈마탄
    * @details		반경 내로 끌어당기는 탄환 / 무기 타입이 로켓런쳐어야 한다.
    * @author		이훈 (MoonAuSosiGi@gmail.com)
    * @date			2017-11-10
    * @file			Bullet_Plasma.cs
    * @version		0.0.1
    */
    public class Bullet_Plasma : MonoBehaviour
    {
        #region Bullet Plasma --------------------------------------------------
        [SerializeField]
        private GameObject m_plasmaHitEffect = null; ///< 플레이어가 맞았을 때
        private float m_effectCoolTime = 0.0f; ///< 이펙트 띄울 쿨타임
        #region Bullet Plasma Property -----------------------------------------

        /**
         * @brief   플라즈마 히트 이펙트 등록을 위한 프로퍼티
         */
        public GameObject PLASMA_HIT_EFFECT
        {
            get { return m_plasmaHitEffect; }
            set { m_plasmaHitEffect = value; }
        }
        #endregion -------------------------------------------------------------
        #endregion -------------------------------------------------------------
        #region Unity Mehtod ---------------------------------------------------
        /**
         * @brief   지속적으로 끌어당긴다.
         * @todo    이펙트 출력 어떻게 할 것인지
         */
        void OnTriggerStay(Collider other)
        {
            // 플레이어 컨트롤러만
            if (other.CompareTag("PlayerCharacter") == false)
                return;
            float distance = Vector3.Distance(transform.position , other.transform.position);
            Vector3 dir = transform.position - other.transform.position;
            dir.Normalize();
            if (distance > 1.0f)
                other.transform.position += dir * 3.0f * Time.deltaTime;

            // 데미지 구간
            if(distance <= 2.58f)
            {
                m_effectCoolTime -= Time.deltaTime;

                if (m_effectCoolTime < 0.0f)
                    m_effectCoolTime = 0.0f;

                if (m_effectCoolTime <= 0.0f)
                    DamageEffectShow(other.transform.position);
                
            }
            
        }
        #endregion -------------------------------------------------------------
        #region Bullet Plasma Method -------------------------------------------

        /**
         * @brief   데미지 이펙트 표시
         * @param   position 이펙트가 표시될 좌표
         */
        void DamageEffectShow(Vector3 position)
        {
            if (m_plasmaHitEffect == null)
                return;
            Debug.Log("HIT !!");
            GameObject obj = GameObject.Instantiate(m_plasmaHitEffect);
            obj.SetActive(true);
            obj.transform.position = position;
            var effect = obj.AddComponent<TimeForEscape.Util.Effect.OneHitEffect>();
            effect.EFFECT_TYPE = Util.Effect.OneHitEffect.EffectDeleteType.TIME_EVENT;
            effect.EFFECT_DELETE_TIME = 2.0f;
            m_effectCoolTime = 2.0f;
        }
        #endregion -------------------------------------------------------------
    }
}