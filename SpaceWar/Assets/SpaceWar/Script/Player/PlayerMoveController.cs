using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeForEscape.Player
{
    /**
    * @brief		PlayerMoveController :: 플레이어 이동 컨트롤러
    * @details		이동 / 점프 / 대시에 관련된 조작을 하는 컴포넌트
    * @author		이훈 (MoonAuSosiGi@gmail.com)
    * @date			2017-11-08
    * @file			PlayerMoveController.cs
    * @version		0.0.1
    */
    public class PlayerMoveController : MonoBehaviour, PlayerController.PlayerComponent
    {
        #region Player Move Controller INFO --------------------------------------------------------------------------------
        [SerializeField] private PlayerController m_playerController = null; ///< 플레이어 컨트롤러 
        [SerializeField] private bool m_isMoveAble = true; ///< 이동 가능 상태인지에 대한 변수
        [SerializeField] private bool m_isJumpAble = true; ///< 점프 가능 상태인지에 대한 변수
        [SerializeField] private bool m_isDashAble = true; ///< 대시 가능 상태인지에 대한 변수

        [SerializeField] private GameObject m_dashEffect = null; ///< 대시 이펙트
        [SerializeField] private AnimationCurve m_dashCurve = null; ///< 대시 커브
        [SerializeField] private AnimationCurve m_jumpCurve = null; ///< 점프 커브

        [SerializeField] private bool m_isMoving = false; ///< 현재 이동중인지
        [SerializeField] private bool m_isJumping = false; ///< 현재 점프중인지

        #region Player Direction -------------------------------------------------------------------------------------------

        /**
         * @brief   플레이어의 이동 방향에 관한 enum
         */
        public enum PlayerMoveDir
        {
            NONE,                   // IDLE
            WALK_FOWARD,            // up
            WALK_FOWARD_LEFT,       // up + left
            WALK_FOWARD_RIGHT,      // up + right
            WALK_LEFT,              // left
            WALK_RIGHT,             // right
            WALK_BACK,              // down
            WALK_BACK_LEFT,         // down + left
            WALK_BACK_RIGHT,        // down + right
            RUN_FOWARD,             // run up
            RUN_FOWARD_LEFT,        // run up + left
            RUN_FOWARD_RIGHT,       // run up + right
            RUN_LEFT,               // run left
            RUN_RIGHT,              // run right
            RUN_BACK,               // run back
            RUN_BACK_LEFT,          // run back + left
            RUN_BACK_RIGHT          // run back + right
        }

        private PlayerMoveDir m_currentDir = PlayerMoveDir.NONE; ///< 캐릭터 방향이 담길 변수
        private Vector3 m_dashDirection = Vector3.zero; ///< 대시를 진행할 방향이 담길 벡터
        #endregion ---------------------------------------------------------------------------------------------------------

        #region Key Input INFO ---------------------------------------------------------------------------------------------

        [SerializeField] private KeyCode m_KeyUp = KeyCode.W;       ///< 위쪽 이동
        [SerializeField] private KeyCode m_KeyLeft = KeyCode.A;     ///< 왼쪽 이동
        [SerializeField] private KeyCode m_KeyDown = KeyCode.S;     ///< 아래쪽 이동
        [SerializeField] private KeyCode m_KeyRight = KeyCode.D;    ///< 오른쪽 이동
        [SerializeField] private KeyCode m_KeyRun = KeyCode.LeftShift;    ///< 달리기
        [SerializeField] private KeyCode m_keyDash = KeyCode.LeftControl; ///< 대쉬
        [SerializeField] private KeyCode m_KeyJump = KeyCode.Space; ///< 점프 
        #endregion ---------------------------------------------------------------------------------------------------------

        #region Player Move Controller Property ----------------------------------------------------------------------------
        /**
         * @brief   이동 가능 여부에 대한 프로퍼티
         * @detail  이동 불가 상태로 변경되었을 때는 기타 이동 관련 변수들도 끈다.
        */
        public bool IS_MOVEABLE
        {
           get { return m_isMoveAble; }
           set
            {
                m_isMoveAble = value;

                if(m_isMoveAble == false)
                {
                    // 이동중이 아니라는 것을 세팅
                    m_isMoving = false;
                }
            }
        }
        /**
         * @brief   점프 가능 여부에 대한 프로퍼티
        */
        public bool IS_JUMPABLE
        {
            get { return m_isJumpAble; }
            set { m_isJumpAble = value; }
        }
        /**
         * @brief   대시 가능 여부에 대한 프로퍼티
        */
        public bool IS_DASHABLE
        {
            get { return m_isDashAble; }
            set { m_isDashAble = value; }
        }

        /**
         * @brief   현재 움직이고 있는지에 대한 프로퍼티
         */
        public bool IS_MOVING
        {
            get { return m_isMoving; }
        }

        /**
         * @brief   현재 점프하고 있는지에 대한 프로퍼티
         */
        public bool IS_JUMPING
        {
            get { return m_isJumping; }
        }
        #endregion ---------------------------------------------------------------------------------------------------------

        #endregion ---------------------------------------------------------------------------------------------------------

        /**
         * @brief   매 프레임 호출되는 함수 ( Update 와 동일 )
         */
        void PlayerController.PlayerComponent.UpdateController()
        {
            // 이동 가능할 때 이동
            if(IS_MOVEABLE)
            {
                MoveProcess();
            }

            // 점프 가능할 때 점프
            if(IS_JUMPABLE)
            {
                JumpProcess();
            }
        }

        #region Move Method ------------------------------------------------------------------------------------------------
        
        /**
         * @brief   이동 / 대시 로직
         * @todo    애니메이션 재생 처리
         */
        void MoveProcess()
        {
            #region Dash :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            bool dashKey = Input.GetKeyDown(m_keyDash);
            float horizontalSpeed = 0.0f, verticalSpeed = 0.0f;
            
            // 대시 체크로직 / 대시 가능한 상태고 && 대시 키가 눌렸다
            if(m_isDashAble == true && dashKey == true)
            {
                // 대시 애니메이션이 끝나기 전까지 수행 불가능
                m_isDashAble = false;
                // 대시를 하기 전, 캐릭터의 방향을 세팅
                transform.GetChild(0).localRotation = Quaternion.Slerp(transform.GetChild(0).localRotation ,
                                                                        Quaternion.Euler(GetCurrentAngle()) , 0.23f);
                // 현재 방향을 저장
                m_dashDirection = transform.GetChild(0).forward;

                // 이 부분에서 대시 애니메이션 재생
                //

                // 싱글모드가 아닐 때
                if(m_playerController.IS_SINGLEMODE == false)
                {
                    // 네트워크로 산소 소모 요청 보내기
                    NetworkManager.Instance().C2SRequestPlayerUseOxy(GameManager.Instance().PLAYER.NAME , 
                        GameManager.Instance().GetGameTableValue(GameManager.DASH_USE_OXY));
                }
                m_isMoving = true;
                // 코루틴으로 대시 호출
                StartCoroutine(DashCall());
                return;
            }
            #endregion :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            // 대시 진행중일 경우 하위 로직은 수행하지 않는다.
            if (m_isDashAble == false)
                return;

            #region Move :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

            bool runKey = Input.GetKey(m_KeyRun);

            // 이부분 중량 계산후 얻는 값으로 바꿀것
            float runSpeed = GameManager.Instance().GetGameTableValue(GameManager.RUN_SPEED);
            float walkSpeed = GameManager.Instance().GetGameTableValue(GameManager.WALK_SPEED);

            // 가로 이동
            if (Input.GetKey(m_KeyLeft)) horizontalSpeed = (runKey) ? -runSpeed : -walkSpeed;
            if (Input.GetKey(m_KeyRight)) horizontalSpeed = (runKey) ? runSpeed : walkSpeed;

            // 세로 이동
            if (Input.GetKey(m_KeyUp)) verticalSpeed = (runKey) ? runSpeed : walkSpeed;
            if (Input.GetKey(m_KeyDown)) verticalSpeed = (runKey) ? -runSpeed : -walkSpeed;

            // 애니메이션 세팅할 것
            if ((Mathf.Abs(verticalSpeed) > 0.0f || Mathf.Abs(horizontalSpeed) > 0.0f))
                ;
            //WALK_ANI_VALUE = (dash) ? 2 : 1;

            //          else
            //                WALK_ANI_VALUE = 0;

            // 실제 이동로직 
            Vector3 speed = new Vector3(horizontalSpeed , 0 , verticalSpeed);
            transform.Translate(speed * Time.deltaTime);
            #endregion :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

            #region Rotate :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            // 현재 이동방향 세팅
            SetCurrentDirection(runKey);

            // 회전 속도 
            float rotateSpeed = 0.23f;

            // 실제 회전 로직
            transform.GetChild(0).localRotation =
                Quaternion.Slerp(transform.GetChild(0).localRotation ,
                Quaternion.Euler(GetCurrentAngle()) , rotateSpeed);
            #endregion :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

            // 이동중인지 체크용
            if ((Mathf.Abs(verticalSpeed) > 0.0f || Mathf.Abs(horizontalSpeed) > 0.0f))
            {
                m_isMoving = true;
            }
            else
            {
                m_isMoving = false;
            }

            // 싱글모드가 아닐때 네트워크 전송
            if(m_playerController.IS_SINGLEMODE == false)
            {
                NetworkMoveSend(speed);
            }
        }

        /**
         * @brief   점프 로직
         * @todo    애니메이션 재생 처리
         */
        void JumpProcess()
        {
            if(Input.GetKey(m_KeyJump))
            {
                StartCoroutine(JumpCall());
            }
        }

        /**
         * @brief   대시를 수행한다.
         * @detail  지정된 대시 커브 형태로 대시를 진행
         */
        IEnumerator DashCall()
        {
            // 대시 시작 시간
            float startTime = Time.time;
            // 대시 지속 시간
            float dashTick = GameManager.Instance().GetGameTableValue(GameManager.DASH_TICK);
            // 대시 진행 속도
            float dashSpeed = GameManager.Instance().GetGameTableValue(GameManager.DASH_SPEED);

            // 대시 이펙트 켜기
            m_dashEffect.SetActive(true);

            while (Time.time - startTime < dashTick)
            {
                // 진행 방향으로 레이를 쏴서 갈 수 있는지 없는지를 체크한다.
                Ray ray = new Ray(transform.position , m_dashDirection);
                RaycastHit hit;
                if (Physics.Raycast(ray , out hit))
                {
                    float distance = Vector3.Distance(transform.position , hit.transform.position);

                    if (!hit.transform.CompareTag("Untagged") && distance <= 5.0f)
                        yield return new WaitForFixedUpdate();
                }
                // 현재 시간 계산
                float nowTick = (Time.time - startTime) / dashTick;
                
               
                // 실제 이동로직
                this.transform.RotateAround(
                    GravityManager.Instance().CurrentPlanet.transform.position ,
                    GravityManager.Instance().GRAVITY_TARGET.transform.GetChild(0).rotation * Vector3.right ,
                    (dashSpeed * (m_dashCurve.Evaluate(nowTick + Time.fixedDeltaTime) - m_dashCurve.Evaluate(nowTick))));

                
                // 네트워크 플레이시엔 전송해야 한다.
                if (m_playerController.IS_SINGLEMODE == false)
                {
                    // 전송할 속도 
                    Vector3 velo = GravityManager.Instance().GRAVITY_TARGET.transform.GetChild(0).rotation * Vector3.right * dashSpeed
                        * (m_dashCurve.Evaluate(nowTick + Time.fixedDeltaTime) - m_dashCurve.Evaluate(nowTick));
                    // 속도 전송
                    NetworkMoveSend(velo);
                }
                
                yield return new WaitForFixedUpdate();
            }
            // 이부분에서 애니메이션 원래대로 돌린다. 
            //
            m_isMoving = false;
            m_isDashAble = true;
            m_dashEffect.SetActive(false);


        }

        /**
         * @brief   점프를 수행한다.
         * @detail  지정된 점프 커브 형태로 점프 진행
         */
        IEnumerator JumpCall()
        {
            // 점프 불가 상태
            m_isJumpAble = false;

            float startTime = Time.time;

            //중력 적용 안함
            GravityManager.Instance().SetGravityEnable(false);

            float jumpTick = GameManager.Instance().GetGameTableValue(GameManager.JUMP_TICK);
            float jumpPower = GameManager.Instance().GetGameTableValue(GameManager.JUMP_POWER);
            // 점프중
            m_isJumping = true;

            // 이부분 점프 애니메이션
            //

            while (Time.time - startTime < jumpTick)
            {
                float nowTick = (Time.time - startTime) / jumpTick;
                transform.Translate(Vector3.up *
                    (jumpPower * (m_jumpCurve.Evaluate(nowTick + Time.fixedDeltaTime) - m_jumpCurve.Evaluate(nowTick))));
                yield return new WaitForFixedUpdate();
            }

            // 이부분 점프 애니메이션 종료
            //

            // 나중에 수정할것
            GravityManager.Instance().SetGravityEnable(true);
            m_isJumpAble = true;
            // 점프 종료
            m_isJumping = false;

        }
        #endregion ---------------------------------------------------------------------------------------------------------
        #region Jump Method ------------------------------------------------------------------------------------------------
        #endregion ---------------------------------------------------------------------------------------------------------
        #region Dash Method ------------------------------------------------------------------------------------------------
        #endregion ---------------------------------------------------------------------------------------------------------

        #region Network Method ---------------------------------------------------------------------------------------------
        /**
         * @brief   서버 상에 이동 정보를 던진다.
         * @detail  네트워크 이동 동기화를 위한 전송 함수
         * @param   velocity 전송할 속도에 대한 정보
         */
        void NetworkMoveSend(Vector3 velocity)
        {
            NetworkManager.Instance().C2SRequestPlayerMove(GameManager.Instance().PLAYER.NAME ,
                  transform.position , velocity ,
                  transform.localRotation.eulerAngles ,
                  transform.GetChild(0).localRotation.eulerAngles);
        }
        #endregion ---------------------------------------------------------------------------------------------------------

        #region Util Method ------------------------------------------------------------------------------------------------

        /**
         * @brief   현재 플레이어의 이동 방향을 설정한다.
         * @detail  현재 어느 방향으로 이동중인지 설정한다.
         * @param   run 달리고 있는지에 대한 여부
         */
        void SetCurrentDirection(bool run)
        {
            if (run)
            {
                // 전진
                if (Input.GetKey(m_KeyUp) && !Input.GetKey(m_KeyDown) && !Input.GetKey(m_KeyLeft) && !Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.RUN_FOWARD;
                // 전진 + 왼쪽
                else if (Input.GetKey(m_KeyUp) && !Input.GetKey(m_KeyDown) && Input.GetKey(m_KeyLeft) && !Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.RUN_FOWARD_LEFT;
                // 전진 + 오른쪽
                else if (Input.GetKey(m_KeyUp) && !Input.GetKey(m_KeyDown) && !Input.GetKey(m_KeyLeft) && Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.RUN_FOWARD_RIGHT;
                // 왼쪽
                else if (!Input.GetKey(m_KeyUp) && !Input.GetKey(m_KeyDown) && Input.GetKey(m_KeyLeft) && !Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.RUN_LEFT;
                // 오른쪽
                else if (!Input.GetKey(m_KeyUp) && !Input.GetKey(m_KeyDown) && !Input.GetKey(m_KeyLeft) && Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.RUN_RIGHT;
                // 후진
                else if (!Input.GetKey(m_KeyUp) && Input.GetKey(m_KeyDown) && !Input.GetKey(m_KeyLeft) && !Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.RUN_BACK;
                // 후진 + 왼쪽
                else if (!Input.GetKey(m_KeyUp) && Input.GetKey(m_KeyDown) && Input.GetKey(m_KeyLeft) && !Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.RUN_BACK_LEFT;
                // 후진 + 오른쪽
                else if (!Input.GetKey(m_KeyUp) && Input.GetKey(m_KeyDown) && !Input.GetKey(m_KeyLeft) && Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.RUN_BACK_RIGHT;
                else
                    m_currentDir = PlayerMoveDir.NONE;

            }
            else
            {
                // 전진
                if (Input.GetKey(m_KeyUp) && !Input.GetKey(m_KeyDown) && !Input.GetKey(m_KeyLeft) && !Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.WALK_FOWARD;
                // 전진 + 왼쪽
                else if (Input.GetKey(m_KeyUp) && !Input.GetKey(m_KeyDown) && Input.GetKey(m_KeyLeft) && !Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.WALK_FOWARD_LEFT;
                // 전진 + 오른쪽
                else if (Input.GetKey(m_KeyUp) && !Input.GetKey(m_KeyDown) && !Input.GetKey(m_KeyLeft) && Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.WALK_FOWARD_RIGHT;
                // 왼쪽
                else if (!Input.GetKey(m_KeyUp) && !Input.GetKey(m_KeyDown) && Input.GetKey(m_KeyLeft) && !Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.WALK_LEFT;
                // 오른쪽
                else if (!Input.GetKey(m_KeyUp) && !Input.GetKey(m_KeyDown) && !Input.GetKey(m_KeyLeft) && Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.WALK_RIGHT;
                // 후진
                else if (!Input.GetKey(m_KeyUp) && Input.GetKey(m_KeyDown) && !Input.GetKey(m_KeyLeft) && !Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.WALK_BACK;
                // 후진 + 왼쪽
                else if (!Input.GetKey(m_KeyUp) && Input.GetKey(m_KeyDown) && Input.GetKey(m_KeyLeft) && !Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.WALK_BACK_LEFT;
                // 후진 + 오른쪽
                else if (!Input.GetKey(m_KeyUp) && Input.GetKey(m_KeyDown) && !Input.GetKey(m_KeyLeft) && Input.GetKey(m_KeyRight))
                    m_currentDir = PlayerMoveDir.WALK_BACK_RIGHT;
                else
                    m_currentDir = PlayerMoveDir.NONE;
            }
        }

        /**
         * @brief 현재 캐릭터가 바라보고 있는 방향을 가져온다.
         */
        Vector3 GetCurrentAngle()
        {
            Vector3 angle = Vector3.zero;
            // 이제 회전
            switch (m_currentDir)
            {
                case PlayerMoveDir.NONE: break;
                case PlayerMoveDir.WALK_FOWARD: angle = Vector3.zero; break;
                case PlayerMoveDir.WALK_BACK: angle = new Vector3(0.0f , 180.0f , 0.0f); break;
                case PlayerMoveDir.WALK_LEFT: angle = new Vector3(0.0f , -90.0f , 0.0f); break;
                case PlayerMoveDir.WALK_RIGHT: angle = new Vector3(0.0f , 90.0f , 0.0f); break;
                case PlayerMoveDir.WALK_FOWARD_LEFT: angle = new Vector3(0.0f , -45.0f , 0.0f); break;
                case PlayerMoveDir.WALK_FOWARD_RIGHT: angle = new Vector3(0.0f , 45.0f , 0.0f); break;
                case PlayerMoveDir.WALK_BACK_LEFT: angle = new Vector3(0.0f , -135.0f , 0.0f); break;
                case PlayerMoveDir.WALK_BACK_RIGHT: angle = new Vector3(0.0f , 135.0f , 0.0f); break;
                case PlayerMoveDir.RUN_FOWARD: angle = new Vector3(0.0f , 0.0f , 0.0f); break;
                case PlayerMoveDir.RUN_BACK: angle = new Vector3(0.0f , 180.0f , 0.0f); break;
                case PlayerMoveDir.RUN_LEFT: angle = new Vector3(0.0f , -90.0f , 0.0f); break;
                case PlayerMoveDir.RUN_RIGHT: angle = new Vector3(0.0f , 90.0f , 0.0f); break;
                case PlayerMoveDir.RUN_FOWARD_LEFT: angle = new Vector3(0.0f , -45.0f , 0.0f); break;
                case PlayerMoveDir.RUN_FOWARD_RIGHT: angle = new Vector3(0.0f , 45.0f , 0.0f); break;
                case PlayerMoveDir.RUN_BACK_LEFT: angle = new Vector3(0.0f , -135.0f , 0.0f); break;
                case PlayerMoveDir.RUN_BACK_RIGHT: angle = new Vector3(0.0f , 135.0f , 0.0f); break;
            }
            return angle;
        }
        #endregion ---------------------------------------------------------------------------------------------------------
    }
}