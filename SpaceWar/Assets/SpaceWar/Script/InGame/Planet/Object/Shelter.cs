using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
* @brief		Shelter :: 쉘터
* @details		메테오를 막을 수 있는 쉘터. 문을 열고 닫고, 조명을 켜고 끌 수 있는 오브젝트.
* @author		이훈 (MoonAuSosiGi@gmail.com)
* @date			2017-11-06
* @file			Shelter.cs
* @version		0.0.1
*/
namespace TimeForEscape.Object
{
    public class Shelter : MonoBehaviour
    {

        #region Shelter_INFO ----------------------------------------------------------------------------
        private bool m_hasPlayer = false; ///< 쉘터안에 사람이 있는지에 대한 정보
        private int m_shelterID = 0;    ///< 쉘터 인덱스 (네트워크 식별용)
        private bool m_curState = false; ///< 현재 상태...약간 이거 중복되는 코드 같은 냄새
        private bool m_doorState = false; ///< 문 상태 
        private bool m_lightState = false; ///< 조명 상태

        [SerializeField] private AudioSource m_shelterSoundSource = null; ///< 쉘터 사운드 재생용
        [SerializeField] private AudioSource m_shelterInOutSource = null; ///< ? 체크필 
        [SerializeField] private AudioClip m_openSound = null;  ///< 쉘터 열릴때 소리
        [SerializeField] private AudioClip m_closeSound = null; ///< 쉘터 닫힐때 소리
        [SerializeField] private AudioClip m_inIdleSound = null; ///< 쉘터 안에서 움직일때 소리
        [SerializeField] private AudioClip m_outIdleSound = null; ///< 쉘터 밖에서 움직일 때 소리

        #region Shelter_Property -----------------------------------------------------------------------
        /**
         * @brief   쉘터 안에 사람이 있는지에 대한 프로퍼티
         */
        public bool HAS_PLAYER { get { return m_hasPlayer; } set { m_hasPlayer = value; } }

        /**
         * @brief   쉘터의 문 상태에 관한 프로퍼티
         */
        public bool DOOR_STATE { get { return m_doorState; } set { m_doorState = value; } }

        /**
         * @brief   쉘터의 조명 상태에 관한 프로퍼티
         */
        public bool LIGHT_STATE { get { return m_lightState; } }

        /**
         * @brief   쉘터 네트워크 식별자에 관한 프로퍼티
         */
        public int SHELTER_ID { get { return m_shelterID; } set { m_shelterID = value; } }
        #endregion -------------------------------------------------------------------------------------
        #endregion -------------------------------------------------------------------------------------

        #region Shelter_Method -------------------------------------------------------------------------

        /**
         * @brief   쉘터 입장시 호출
         * @details  ShelterDoor 스크립트의 Trigger에서 호출함. 서버에 해당 정보를 보낸다.
         */
        public void ShelterEnter()
        {
            // 이미 사람이 있다면 수행할 필요 없음
            if (HAS_PLAYER) return;

            // 사람이 들어온 것이므로 변경
            HAS_PLAYER = true;

            // 사람이 들어왔음을 알림
            NetworkManager.Instance().C2SRequestShelterEnter(m_shelterID , true);
        }

        /**
         * @brief   쉘터 퇴장시 호출
         * @details  ShelterDoor 스크립트의 Trigger에서 호출함. 서버에 해당 정보를 보낸다.
         */
        public void ShelterExit()
        {
            // 이미 사람이 없는 상태라면 수행할 필요 없음
            if (!HAS_PLAYER) return;

            // 사람이 나갔으므로 없다고 호출
            HAS_PLAYER = false;
            
            // 사람이 나갔음을 알림
            NetworkManager.Instance().C2SRequestShelterEnter(m_shelterID , false);
        }

        /**
         * @brief   실제 문을 플레이어가 조작할때 호출
         * @details  플레이어가 F키를 눌렀을 때 호출된다.
         */
        public void DoorControl()
        {
            if (!m_curState)
                OpenDoor();
            else
                CloseDoor();
        }

        /**
         * @brief   실제 문을 여는 함수
         * @details  실제 문을 여는 함수 / 네트워크 상 메시지를 받았을 경우에도 호출
         * @todo    m_curState 체크 후 제거
         * @param   networkOrder 네트워크 상에서 호출될 경우 true, 로컬 호출시 false(디폴트)
         */
        public void OpenDoor(bool networkOrder = false)
        {
            if (m_curState)
                return;

            m_curState = true;

            // 쉘터 여는 소리 재생
            m_shelterSoundSource.clip = m_openSound;
            m_shelterSoundSource.Play();

            // 여는 애니메이션
            GetComponent<Animator>().SetInteger("DOOR_OPEN_STATE" , 1);
            
            // networkOrder가 false면 로컬 호출이므로 서버에 알린다.
            if (networkOrder == false)
                NetworkManager.Instance().C2SRequestShelterDoorControl(m_shelterID , true);
        }

        /**
         * @brief   실제 문을 닫는 함수
         * @details  실제 문을 닫는 함수 / 네트워크 상 메시지를 받았을 경우에도 호출
         * @todo    m_curState 체크 후 제거
         * @param   networkOrder 네트워크 상에서 호출될 경우 true, 로컬 호출시 false(디폴트)
         */
        public void CloseDoor(bool networkOrder = false)
        {
            if (!m_curState)
                return;

            m_curState = false;

            // 쉘터 닫는 소리 재생
            m_shelterSoundSource.clip = m_closeSound;
            m_shelterSoundSource.Play();

            // 닫는 애니메이션 재생
            GetComponent<Animator>().SetInteger("DOOR_OPEN_STATE" , 2);

            // networkOrder가 false면 로컬 호출이므로 서버에 알린다.
            if (networkOrder == false)
                NetworkManager.Instance().C2SRequestShelterDoorControl(m_shelterID , false);
        }

        /**
         * @brief   조명을 켜는 함수
         * @details  네트워크 상에서 호출 하는 조명 조작 함수 / 따로 호출하지 않음
         */
        public void LightOn()
        {
            // 조명값을 세팅
            m_lightState = true;

            // 조명 켜는 애니메이션
            GetComponent<Animator>().SetInteger("LIGHT_STATE" , 1);

            // 쉘터 안에 있을 때 / 밖일 때 걷고 뛰는 소리 수정
            if (GameManager.Instance().PLAYER.m_player.IS_SHELTER)
            {
                m_shelterSoundSource.clip = m_inIdleSound;
                m_shelterSoundSource.Play();
            }
            else
            {
                m_shelterSoundSource.clip = m_outIdleSound;
                m_shelterSoundSource.Play();
            }
        }

        /**
         * @brief   조명을 끄는 함수
         * @details  네트워크 상에서 호출하는 조명 조작 함수 / 따로 호출하지 않음
         */
        public void LightOff()
        {
            // 아무도 없다     
            m_lightState = false;
            // 조명을 끄는 애니메이션
            GetComponent<Animator>().SetInteger("LIGHT_STATE" , 2);

            // 쉘터 소리 끔
            m_shelterSoundSource.clip = null;
            m_shelterSoundSource.Stop();
        }

        #endregion -------------------------------------------------------------------------------------
    }

}
