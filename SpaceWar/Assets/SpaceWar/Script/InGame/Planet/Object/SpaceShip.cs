using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/**
* @brief		SpaceShip :: 우주선
* @details		서바이벌에서 탈출에 필요한 우주선
* @author		이훈 (MoonAuSosiGi@gmail.com)
* @date			2017-11-06
* @file			SpaceShip.cs
* @version		0.0.1
*/
namespace TimeForEscape.Object
{
    public class SpaceShip : MonoBehaviour
    {
        #region SpaceShip_INFO ------------------------------------------------------------------
        private float m_fuel = 0.0f; ///< 우주선 연료
        private bool m_isEnd = false; ///< 우주선에 연료가 다 채워졌는지 체크하는 변수

        private int m_spaceShipID = 0; ///< 우주선 네트워크 식별 아이디
        [SerializeField] private AudioSource m_spaceShipSource = null; ///< 우주선 사운드 재생
        private bool m_isSpaceShipEnabled = false; ///< 우주선이 사용 가능한지 여부

        #region SpaceShip Property --------------------------------------------------------------
        /**
         * @brief   우주선의 네트워크 식별 아이디에 대한 프로퍼티
         */
        public int SPACESHIP_ID
        {
            get { return m_spaceShipID; }
            set { m_spaceShipID = value; }
        }
        
        /**
         * @brief   우주선 사용 가능 여부에 대한 프로퍼티
         */
        public bool IS_SPACESHIP_ENABLED
        {
            get { return m_isSpaceShipEnabled; }
            set { m_isSpaceShipEnabled = value; }
        }
        #endregion ------------------------------------------------------------------------------
        #endregion ------------------------------------------------------------------------------

        /**
         * @brief   우주선 충전 최초 수행시 세팅
         * @details  우주선 조작 게이지를 띄운다.
         */
        public void StartSpaceShipEngineCharge()
        {
            GameManager.Instance().m_inGameUI.StartSpaceShipUI();
        }

        /**
         * @brief   우주선 충전을 그만둘때 수행
         * @details  게이지 하이드 / 취소 명령 날림
         */
        public void StopSpaceShipEngineCharge()
        {
            if (!m_isEnd)
                m_fuel = 0.0f;
            // 충전 취소 메시지 날림
            NetworkManager.Instance().C2SNotifySpaceShipEngineFailed(SPACESHIP_ID);
            // 게이지 숨김
            GameManager.Instance().m_inGameUI.StopSpaceShipUI();

            // 취소 로직 수행
            SpaceShipEngineChargerProcessCancel();
        }

        /**
         * @brief   우주선 연료 충전 프로세스
         * @details  InvokeReapeating 으로 값을 변경한다.
         */
        public void SpaceShipEngineChargeProcess()
        {
            if (IsInvoking("ChargeProcess") == false)
                InvokeRepeating("ChargeProcess" , Time.deltaTime , Time.deltaTime);
        }

        /**
         * @brief   우주선 충전 프로세스 취소 로직
         * @details  Invoke 를 취소하고 충전값을 되돌린다
         */
        public void SpaceShipEngineChargerProcessCancel()
        {
            // 충전 프로세스 중단
            if (IsInvoking("ChargeProcess"))
                CancelInvoke("ChargeProcess");

            // 연료 초기화
            m_fuel = 0.0f;
        
            // 우주선 충전량 0.0을 알림
            NetworkManager.Instance().C2SNotifySpaceShipEngineCharge(m_spaceShipID , m_fuel);
        }

        /**
         * @brief   주기적으로 호출되는 충전 게이지 프로세스
         */
        void ChargeProcess()
        {
            SpaceShipEngineCharge(0.1f * Time.deltaTime , true);
        }

        /**
         * @brief   우주선 실 충전 프로세스
         * @details  게이지에 실제 값을 할당하는 함수
         * @todo    카메라 제어값 매니저로 옮길것
         * @param   t 더할 연료량
         * @param   me true일 경우 로컬 플레이어가 호출. 그게 아닌경우 서버측이 호출
         */
        public void SpaceShipEngineCharge(float t , bool me)
        {
            // 끝났으면 수행하지 않음
            if (m_isEnd)
                return;

            // 로컬 플레이어가 수행하는 것이라면
            if (me)
            {
                // 연료를 더하고 UI 에 표시한 뒤에 서버에 알림
                m_fuel += t;
                GameManager.Instance().m_inGameUI.ProcessSpaceShipEngine(m_fuel);
                NetworkManager.Instance().C2SNotifySpaceShipEngineCharge(m_spaceShipID , m_fuel);

            }
            // 서버측에서 수행하는 것이면 연료에 걍 넣어준다.
            else
                m_fuel = t;
            
            // 애니메이션과 연동
            this.GetComponent<Animator>().SetFloat("SPACE_SHIP_SETTING" , m_fuel);

            // 연료가 1.0이 넘을 경우 완료
            if (m_fuel > 1.0f)
            {
                // 로컬 플레이어라면 승리
                if (me)
                {
                    // 우주선을 탔음을 알림
                    NetworkManager.Instance().C2SRequestSpaceShip();
                    // 승리 했음을 로컬에서 체크할 수 있어야 하므로  해당 값 넣기
                    GameManager.Instance().WINNER = true;
                    // 게이지 UI는 감춘다.
                    GameManager.Instance().m_inGameUI.StopSpaceShipUI();
                    // 카메라 제어도 필요없다.
                    CameraManager.Instance().enabled = false;

                    // 카메라 시야를 넓게 함
                    Camera.main.fieldOfView = 70.0f;
                    // 카메라는 더이상 플레이어에게 붙지 않고 우주선에 붙는다.
                    Camera.main.transform.SetParent(transform.GetChild(0).GetChild(0) , false);
                    // 회전값 / 포지션값 초기화
                    Camera.main.transform.localEulerAngles = new Vector3(0.0f , 0.0f , 0.0f);
                    Camera.main.transform.localPosition = new Vector3(0.0f , 0.0f , 0.0f);

                    // 기타 이펙트들을 감춘다.
                    CameraManager.Instance().HideNotEnoughHpEffect();
                    CameraManager.Instance().HideNotEnoughOxyEffect();
                    CameraManager.Instance().EffectHitExit();

                    // 플레이어에서 수행할 일 처리
                    GameManager.Instance().PLAYER.m_player.SpaceShipChargeEnd();
                    // 플레이어도 숨긴다.
                    GameManager.Instance().PLAYER.m_player.gameObject.SetActive(false);

                    // 플레이어에 붙어있는 리스너를 더이상 못쓰므로 여기에 추가
                    this.gameObject.AddComponent<AudioListener>();
                }
                // 우주선 사운드 재생
                m_spaceShipSource.Play();
                // 끝
                m_isEnd = true;
            }
        }

        /**
         * @brief   우주선 탈출 하는 애니메이션 재생 후 호출되는 함수
         * @details  결과 씬으로 이동해야한다.
         */
        public void SpaceShipAnimationEnd()
        {
            // 씬 이동
            SceneManager.LoadScene("Space_1Result");
        }
    }
}