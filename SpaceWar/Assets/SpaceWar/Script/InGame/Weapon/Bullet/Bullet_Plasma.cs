using System.Collections;
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
                // TODO 이펙트 띄우기 
                
            }
            
        }
        #endregion -------------------------------------------------------------
        #region Bullet Plasma Method -------------------------------------------

        #endregion -------------------------------------------------------------
    }
}