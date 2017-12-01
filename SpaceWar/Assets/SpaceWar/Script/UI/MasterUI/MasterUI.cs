using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeForEscape.UI
{

    /**
      * @brief		    MasterUI :: 마스터 모드 UI
      * @details		치트 모드
      * @author		    이훈 (MoonAuSosiGi@gmail.com)
      * @date			2017-11-26
      * @file			MasterUI.cs
      * @version		0.0.1
      */
    public class MasterUI : MonoBehaviour
    {

        #region Master UI INFO ============================================================================
        [SerializeField]
        private UILabel m_targetLabel = null; ///< 타겟 지정 라벨
        [SerializeField]
        private UILabel m_statusLabel = null; ///< 현재 상태 라벨 
        private int m_meteorCreateLimit = 0; ///< 10초마다 한번씩 생성가능하게
        #endregion ========================================================================================

        #region Master UI Method ==========================================================================

        /**
         * @brief   마스터 유아이 켜기
         */
        public void ShowMasterUI()
        {
            gameObject.SetActive(true);
        }
        /**
        * @brief   마스터 유아이 끄기
        */
        public void HideMasterUI()
        {
            gameObject.SetActive(false);
        }

        /**
         * @brief   마스터 유아이 조작 관련
         */
        void Update()
        {
            int command = -1;
            // 기말 발표용
            // 6 : 체력회복 7 : 산소 회복 8 : 강제 부활
            // 9 메테오
            if (Input.GetKeyDown(KeyCode.Alpha6)) command = 6;
            if (Input.GetKeyDown(KeyCode.Alpha7)) command = 7;
            if (Input.GetKeyDown(KeyCode.Alpha8)) command = 8;
            if (Input.GetKeyDown(KeyCode.Alpha9)) command = 9;

            switch(command)
            {
                case 6: HpUpdate(10.0f); break;
                case 7: OxyUpdate(10.0f); break;
                case 8: RebirthPlayer((int)NetworkManager.Instance().HOST_ID); break;
                case 9: MeteorCreate(); break;

            }

        }
        // 시연 대비해서 잠시..
        void Temp()
        {
            int command = -1;
            if (Input.GetKey(KeyCode.Alpha1)) command = 1;
            else if (Input.GetKey(KeyCode.Alpha2)) command = 2;
            else if (Input.GetKey(KeyCode.Alpha3)) command = 3;
            else if (Input.GetKey(KeyCode.Alpha4)) command = 4;
            else if (Input.GetKey(KeyCode.Alpha5)) command = 5;
            else if (Input.GetKey(KeyCode.Alpha6)) command = 6;
            else if (Input.GetKey(KeyCode.Alpha7)) command = 7;
            else if (Input.GetKey(KeyCode.Alpha8)) command = 8;

            switch (command)
            {
                case 1: HpUpdate(10); break;
                case 2: HpUpdate(-10); break;
                case 3: OxyUpdate(10); break;
                case 4: OxyUpdate(-10); break;
                case 5: break;
                case 6: break;
                case 7: break;
            }
        }

        #endregion ========================================================================================

        #region CheatMethod ===============================================================================
        /**
         * @brief   체력 회복 / 체력 소모 메소드
         * @param   hp + 일 경우 회복 - 일 경우 소모
         */
        void HpUpdate(float hp)
        {
            if(NetworkManager.Instance() != null)
            {
                NetworkManager.Instance().RequestHpUpdate(GameManager.Instance().PLAYER.HP + hp);
            }
        }

        /**
         * @brief   산소 회복 / 산소 소모 메소드
         * @param   oxy + 일 경우 회복 - 일 경우 소모
         */
         void OxyUpdate(float oxy)
        {
            if(NetworkManager.Instance() != null)
            {
                NetworkManager.Instance().RequestOxyUpdate(GameManager.Instance().PLAYER.OXY + oxy);
            }
        }

        /**
         * @brief   대시 무한 모드 설정
         * @param   infinity 무한으로 설정할 것인지에 대한 bool 변수
         */
        void SetDashInfinity(bool infinity)
        {

        }

        /**
         * @brief   총알 무한 모드 설정
         * @param   infinity 무한으로 설정할 것인지에 대한 bool 변수
         */
         void SetAmmoInfinity(bool infinity)
        {

        }

        /**
         * @brief   부활 명령 날리기
         * @param   targetHostID 살릴 플레이어의 HostID
         */
        void RebirthPlayer(int targetHostID)
        {

        }

        /**
         * @brief   죽이는 명령 날리기
         * @param   targetHostID 죽일 플레이어의 HostID
         */
         void KillPlayer(int targetHostID)
        {

        }

        /**
         * @brief   현재 위치로 메테오 날리기
         */
         void MeteorCreate()
        {
           if(m_meteorCreateLimit == 0)
            {
                NetworkManager.Instance().CreateMeteor(
                    GameManager.Instance().PLAYER.m_player.transform.position);
            }
        }
        #endregion ========================================================================================
    }
}