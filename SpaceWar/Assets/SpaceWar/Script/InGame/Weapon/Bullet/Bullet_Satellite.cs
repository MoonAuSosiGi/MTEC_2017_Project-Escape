using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeForEscape.Object.Weapon
{
    /**
    * @brief		Bullet_Satellite :: 새틀라이트 탄
    * @details		새틀라이트 탄 ( 날아가서 부딪히면 새틀라이트를 소환한다.
    * @author		이훈 (MoonAuSosiGi@gmail.com)
    * @date			2017-11-14
    * @file			Bullet_Satellite.cs
    * @version		0.0.1
    */
    public class Bullet_Satellite : MonoBehaviour
    {

        #region Bullet Satellite INFO --------------------------------------------------------------------------------
        [SerializeField]
        private GameObject m_satellite = null; ///< 위성 
        private RocketBullet m_rocketBullet = null; ///< 타겟 총알
        #endregion ---------------------------------------------------------------------------------------------------
        #region Bullet Satellite Method ------------------------------------------------------------------------------

        /**
         * @brief   시작시 RocketBulletExplosion 을 제거한다.
         * @detail  제거한 뒤 Weapon 을 받아옴
         */
        void Start()
        {
            // 타겟 무기 넣기
            m_rocketBullet = transform.parent.GetComponent<RocketBullet>();
            GameObject.Destroy(GetComponent<RocketBulletExplosion>());

            if (m_rocketBullet.IS_REMOTE == true)
                return;
            // 이 시점에서 호출해 본다.
            Invoke("SatelliteCall" , 2.0f);
        }



        /**
         * @brief   위성 호출 메소드
         * @detail  2초뒤 호출된다.
         */
        void SatelliteCall()
        {
            WeaponManager.Instance().NetworkObjectCreate(WeaponManager.NetworkObjectType.SATELLITE ,
                m_rocketBullet.NETWORK_ID + "_satellite" , transform.position , transform.eulerAngles);
        }
        #endregion ---------------------------------------------------------------------------------------------------
    }
}