using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeForEscape.Object.Weapon
{
    /**
    * @brief		Bullet_Plasma :: 플라즈마탄
    * @details		반경 내로 끌어당기는 탄환 / 무기 타입이 로켓런쳐어야 한다.
    * @author		이훈 (MoonAuSosiGi@gmail.com)
    * @date			2017-11-14
    * @file			Bullet_Plasma.cs
    * @version		0.0.1
    */
    public class Bullet_Plasma : MonoBehaviour
    {
        #region Bullet Plasma --------------------------------------------------
        [SerializeField]
        private GameObject m_plasmaHitEffect = null; ///< 플레이어가 맞았을 때
        [SerializeField]
        private AudioClip m_plasmaHitSound = null; ///< 플라즈마 히트 사운드
        [SerializeField] private AudioSource m_hitSource = null;
        
        private float m_effectCoolTime = 0.0f; ///< 이펙트 띄울 쿨타임
        private WeaponItem m_targetWeapon = null; ///< 타겟 무기

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
         * @brief   처음 생성되었을 때 RocketExplosion을 제거해준다.
         */
        void Start()
        {
            var rocket = GetComponent<RocketBulletExplosion>();

            // 타겟 무기 넣기
            m_targetWeapon = rocket.ROCKET;
            GameObject.Destroy(rocket);
        }
        /**
         * @brief   지속적으로 끌어당긴다.
         * @todo    이펙트 출력 어떻게 할 것인지
         */
        void OnTriggerStay(Collider other)
        {
            // 플레이어 컨트롤러만
            if (other.CompareTag("PlayerCharacter") == false)
                return;

            // 거리로 체크
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
                {
                    // 이펙트를 보여주고
                    DamageEffectShow(other.transform.position);
                    // 데미지 요청
                    Damage(other.gameObject);
                }
                
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

            // 이펙트 생성
            GameObject obj = GameObject.Instantiate(m_plasmaHitEffect);
            obj.SetActive(true);
            obj.transform.position = position;

            // 이펙트 타입 세팅
            var effect = obj.AddComponent<TimeForEscape.Util.Effect.OneHitEffect>();
            effect.EFFECT_TYPE = Util.Effect.OneHitEffect.EffectDeleteType.TIME_EVENT;
            effect.EFFECT_DELETE_TIME = 2.0f;
            m_effectCoolTime = 2.0f;

            m_hitSource.clip = m_plasmaHitSound;
            m_hitSource.Play();
        }

        /**
         * @brief   데미지 로직
         */
        void Damage(GameObject target)
        {
            var bullet = transform.parent.GetComponent<RocketBullet>();
            if (bullet.IS_REMOTE == true)
                return;
            var p = target.GetComponent<PlayerController>();
            var np = target.GetComponent<NetworkPlayer>();

            // 내가 맞는다.
            if (p != null && np == null)
            {
                NetworkManager.Instance().C2SRequestPlayerDamage(
                   (int)NetworkManager.Instance().HOST_ID ,
                   GameManager.Instance().PLAYER.NAME ,
                   m_targetWeapon.ITEM_NAME ,
                   m_targetWeapon.DAMAGE ,
                   transform.position);
            }
            // 다른 플레이어가 맞는다
            else if(np != null)
            {
                NetworkManager.Instance().C2SRequestPlayerDamage(
                   (int)np.HOST_ID,
                   np.m_userName ,
                   m_targetWeapon.ITEM_NAME ,
                   m_targetWeapon.DAMAGE ,
                   transform.position);
            }
                
        }
        #endregion -------------------------------------------------------------
    }
}