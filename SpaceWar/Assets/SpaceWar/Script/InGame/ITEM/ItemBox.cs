using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* @brief		ItemBox :: 아이템 박스
* @details		랜덤으로 아이템을 지급하는 오브젝트
* @author		이훈 (MoonAuSosiGi@gmail.com)
* @date			2017-11-06
* @file			ItemBox.cs
* @version		0.0.1
*/
namespace TimeForEscape.Object
{
    public class ItemBox : MonoBehaviour
    {

        #region ItemBox_INFO --------------------------------------------------------------------------
        private int m_itemBoxID = 0; ///< ItemBox의 네트워크 식별 아이디
        private bool m_isOpened = false; ///< 아이템 박스가 열려있는지에 대한 정보
        private bool m_networkRecv = false; ///< 네트워크 요청을 한번만 보내기 위해 제어하는 변수

        #region ItemBox Property ----------------------------------------------------------------------
        
        /**
         * @brief   아이템 박스의 네트워크 식별 아이디에 대한 프로퍼티
         */
        public int ITEMBOX_ID
        {
            get { return m_itemBoxID; }
            set { m_itemBoxID = value; }
        }

        /**
         * @brief   아이템 박스가 열렸는지에 대한 프로퍼티
         * @details  얻기만 가능
         */
        public bool OPENED
        {
            get { return m_isOpened; }

        }
        #endregion ------------------------------------------------------------------------------------
        #endregion ------------------------------------------------------------------------------------

        #region ItemBox Method ------------------------------------------------------------------------

        /**
         * @brief   아이템 박스를 사용한다.
         * @details  로컬에서 플레이어가 조작했을 때 수행
         */
        public void UseItemBox()
        {
            if (m_isOpened)
                return;
            // 이건 애니메이션 요청
            this.GetComponent<Animator>().SetInteger("ITEMBOX_STATE" , 1);
        }
        /**
         * @brief   아이템 박스 열리는 애니메이션이 종료되었을 때 수행
         * @details  애니메이션 클립에서 호출 / 서버에 아이템을 달라고 요청
         */
        void ItemBoxAniEnd()
        {
            m_isOpened = true;
            if (!m_networkRecv)
                NetworkManager.Instance().C2SRequestPlayerUseItemBox(this);
        }

        /**
         * @brief   네트워크 상에서 아이템 박스를 연다.
         * @details  다른 플레이어가 아이템 박스를 열었을 때 수행
         */
        public void ItemBoxNetworkOpen()
        {
            m_networkRecv = true;
            this.GetComponent<Animator>().SetInteger("ITEMBOX_STATE" , 1);
        }
        
        #endregion -----------------------------------------------------------------------------------
    }
}