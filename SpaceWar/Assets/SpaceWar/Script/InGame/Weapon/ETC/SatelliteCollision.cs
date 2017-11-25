using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeForEscape.Object.Weapon
{
    /**
    * @brief		Satellite Collision :: 전투 위성 충돌 체크 스크립트
    * @details		충돌 체크를 수행하고 상위에 알려준다.
    * @author		이훈 (MoonAuSosiGi@gmail.com)
    * @date			2017-11-22
    * @file			Satellite.cs
    * @version		0.0.1
    */
    public class SatelliteCollision : MonoBehaviour
    {
        #region Satellite Collision ------------------------------------------------------------------------
        [SerializeField] private Satellite m_satellite = null; ///< 타겟 위성
        #endregion -----------------------------------------------------------------------------------------

        #region Satellite Method ---------------------------------------------------------------------------
        /**
          * @brief   충돌시 데미지 전송
          */
        void OnTriggerStay(Collider other)
        {
            if (m_satellite != null)
                m_satellite.HitTarget(other);
        }
        #endregion -----------------------------------------------------------------------------------------
    }
}