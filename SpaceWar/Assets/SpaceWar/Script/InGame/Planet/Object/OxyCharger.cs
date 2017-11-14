using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* @brief		OxyCharger :: 산소 충전기
* @details		산소를 충전하는 오브젝트
* @author		이훈 (MoonAuSosiGi@gmail.com)
* @date			2017-11-06
* @file			OxyCharger.cs
* @version		0.0.1
*/
namespace TimeForEscape.Object
{
    public class OxyCharger : MonoBehaviour
    {

        #region OxyCharger_INFO -----------------------------------------------------------------

        [SerializeField] private int m_oxyChargerID = -1; ///< 산소충전기 네트워크 식별 아이디
        [SerializeField] private bool m_isAlive = true; ///< 산소 충전기 사용 가능 여부 
        private float m_oxy = 250.0f; ///< 사용가능한 산소량                                                
        private bool m_oxyChargerEnable = false; ///< 산소 충전기가 사용가능한 상태인지

        #region OxyCharger Property -------------------------------------------------------------
        /**
         * @brief   산소 충전기 네트워크 식별 아이디에 대한 프로퍼티
         */
        public int OXY_CHARGER_ID
        {
            get { return m_oxyChargerID; }
            set { m_oxyChargerID = value; }
        }
        
        /**
         * @brief   산소량에 대한 프로퍼티
         */
        public float CURRENT_OXY { get { return m_oxy; } }

        /**
         * @brief   산소 충전기 사용 가능 여부에 대한 프로퍼티
         * @details  서버상에서 체크해서 값을 넣어줌
         */
        public bool OXY_CHARGER_ENABLE
        {
            get { return m_oxyChargerEnable; }
            set {
                m_oxyChargerEnable = value;
                Debug.Log("ENABLE ?! " + value + " " + m_oxyChargerEnable);
            } }
        #endregion ------------------------------------------------------------------------------
        #endregion ------------------------------------------------------------------------------

        #region UnityMethod ---------------------------------------------------------------------

        /**
         * @brief   시작시 산소 게이지 세팅을 수행한다.
         */
        void Start()
        {
            UIUpdate();
        }
        #endregion ------------------------------------------------------------------------------

        #region Oxy Charger Method --------------------------------------------------------------

        /**
         * @brief   산소 충전기 동작
         * @details  충전기의 산소를 소모하면서 플레이어의 산소 증가 요청을 보낸다.
         * @param   oxy 충전할 산소량
         */
        public void UseOxy(float oxy)
        {
            // 산소 충전기가 사용 불가할 경우 리턴
            if (!m_isAlive) return;

            // 이미 산소가 풀일 경우는 패스 
            if (GameManager.Instance().PLAYER.m_oxy >= 
                GameManager.Instance().GetGameTableValue(GameManager.FULL_OXY))
                return;


            float useOxy = (m_oxy < oxy) ? oxy - m_oxy : oxy;
            // 충전할 산소 전송
            NetworkManager.Instance().C2SRequestPlayerUseOxyCharger(this , useOxy);
        }

        /**
         * @brief   서버에서 충전이 되었음을 전달받는 함수
         * @details  서버에서 요청받고 실제로 산소를 소모하는 부분
         * @param   oxy 소모할 산소
         */
        public void RecvOxy(float oxy)
        {
            m_oxy -= oxy;
            UIUpdate();
        }

        /**
         * @brief   산소 게이지 UI 업데이트 함수
         * @details  산소 변동량에 따라 변경되어야함.
         */
        void UIUpdate()
        {
            // 퍼센트 계산
            float percent = m_oxy / 
                GameManager.Instance().GetGameTableValue(GameManager.OXY_CHARGER_FULL);

            // 실제 게이지 조작
            transform.GetChild(0).GetChild(0).transform.localScale 
                = new Vector3(1.0f , percent , 1.0f);

            // 산소가 없을 때에 대한 처리
            if (m_oxy <= 0.0f)
            {
                m_isAlive = false;
                var sources = this.GetComponents<AudioSource>();

                // 사운드 제거
                for (int i = 0; i < sources.Length; i++)
                {
                    if (i != 0)
                        sources[i].enabled = false;
                }

                // 애니메이션 종료 ( 조명 꺼짐 )
                transform.GetChild(0).GetComponent<Animator>().SetInteger("ChargerEnd" , 1);
            }
        }
        #endregion ------------------------------------------------------------------------------
    }
}