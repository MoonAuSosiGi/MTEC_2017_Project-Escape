using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeForEscape.Object.Planet
{
    /**
    * @brief		ObstacleObject 함정 오브젝트
    * @details		맵에 배치되는 함정 오브젝트들 처리를 함 
    * @author		이훈 (MoonAuSosiGi@gmail.com)
    * @date		    2017-11-29
    * @file		    ObstacleObject.cs
    * @version		0.0.1
    */
    public class ObstacleObject : MonoBehaviour
    {
        #region Obstacle Object INFO --------------------------------------------------------------------

        public enum ObstacleObjectType
        {
            DAMAGE_OBJECT = 0
        }

        [SerializeField]
        private string m_obstacleName = null; ///< 함정 이름
        [SerializeField]
        private ObstacleObjectType m_obstacleType = ObstacleObjectType.DAMAGE_OBJECT; ///< 함정 타입
        [SerializeField]
        private float m_damage = 0.0f; ///< 데미지
        [SerializeField]
        private float m_coolTime = 0.0f; ///< 쿨타임

        private bool m_cool = false; ///< 쿨 상황
        #endregion --------------------------------------------------------------------------------------
        #region Obstacle Object Method ------------------------------------------------------------------

        /**
         * @brief   비동기로 테이블 데이터를 세팅한다.
         */
        void Start()
        {
            StartCoroutine(TableSetup());
        }
        /**
         * @brief   테이블 데이터를 세팅한다.
         */
        IEnumerator TableSetup()
        {
            var table = GravityManager.Instance().OBSTACLE_TABLE;
            if (table == null)
                yield return null;

            int targetIndex = -1;

            for(int i = 0; i < table.dataArray.Length; i++)
            {
                if(table.dataArray[i].Name.Equals(m_obstacleName))
                {
                    targetIndex = i;
                    break;
                }
            }
            if (targetIndex == -1)
                yield return null;

            var data = table.dataArray[targetIndex];
            m_obstacleType = (ObstacleObjectType)data.Type;
            m_damage = data.Damage;
            m_coolTime = data.Cooltime;

            yield return null;
        }

        void OnTriggerStay(Collider col)
        {
            if (col.CompareTag("PlayerCharacter") == false || m_cool == true)
                return;

            if(m_obstacleType == ObstacleObjectType.DAMAGE_OBJECT)
            {
                var p = col.GetComponent<PlayerController>();
                
                if(p.enabled == true)
                {
                    NetworkManager.Instance().C2SRequestPlayerDamage(
                        (int)NetworkManager.Instance().HOST_ID ,
                        GameManager.Instance().PLAYER.NAME ,
                        m_obstacleName ,
                        m_damage , transform.position);
                    m_cool = true;
                    Invoke("CoolTimeEnd" , m_coolTime);
                }
            }
        }

        /**
         * @brief   쿨타임 끄기
         */
        void CoolTimeEnd()
        {
            m_cool = false;
        }
        #endregion --------------------------------------------------------------------------------------
    }
}