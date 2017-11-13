using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TimeForEscape.Player
{
    /**
    * @brief		PlayerController :: 플레이어 컨트롤러
    * @details		플레이어 :: 플레이어 내 모든 컴포넌트를 총괄 / 하위 컨트롤러의 Updqte 순서를 제어한다.
    * @author		이훈 (MoonAuSosiGi@gmail.com)
    * @date			2017-11-07
    * @file			PlayerController.cs
    * @version		0.0.1
    */
    public class PlayerController : MonoBehaviour
    {
        #region Player Controller Inner Class ---------------------------------------------------------------------------------------

        /**
         * @brief   플레이어를 제어하는 기본 컴포넌트를 담을 클래스
         * @detail  유니티 인스펙터에서 보기 좋게 제어하기 위해 직렬화 처리.
         */
        [System.Serializable]
        public struct PlayerControlComponent
        {
            public Animator m_animator;             ///< 애니메이션 제어용 애니메이터
            public Rigidbody m_rigidBody;           ///<  중력 제어용 리지드바디
            public SkinnedMeshRenderer m_renderer;  ///<  셰이더 제어용 렌더러
        }

        /**
         * @brief   Player Control 에 관련된 컴포넌트 함수들이 정의되어 있음
         * @detail  하위 컨트롤러는 이걸 상속받아 처리
         */
        public interface PlayerComponent
        {
            /**
             * @brief   프레임마다 수행될 Update 함수.
             */
            void UpdateController();
        }

        
        #endregion ------------------------------------------------------------------------------------------------------------------

        #region PlayerController_INFO -----------------------------------------------------------------------------------------------
        [SerializeField]
        private PlayerControlComponent m_controlComponent = new PlayerControlComponent(); ///< 렌더러/애니메이터/리지드바디 관리용
        private bool m_isSingleMode = false; ///< 싱글 모드 유무

        #region Control Component ===================================================================================================
        [SerializeField] private PlayerMoveController m_moveController = null;    ///< 이동/점프/대시에 관련된 컴포넌트
        [SerializeField] private PlayerInteractionController m_interactionController = null; ///< 상호작용 컴포넌트
        [SerializeField] private PlayerItemController m_itemController = null; ///< 아이템 조작(인벤) / 공격에 관련된 컴포넌트
        #endregion ==================================================================================================================

        #region Player Controller Peroperty :: ======================================================================================
        /**
         * @brief   플레이어 컨트롤용 기본 컴포넌트에 대한 프로퍼티
         */
        public PlayerControlComponent PLAYER_COMPONENT { get { return m_controlComponent; } }

        /**
         * @brief   게임이 싱글모드인지에 대한 프로퍼티
         */
         public bool IS_SINGLEMODE { get { return m_isSingleMode; } }
        #endregion ==================================================================================================================
        #endregion ------------------------------------------------------------------------------------------------------------------
        


        /*
         * @todo    움직임에 소모되는 산소량 / 속도 등은 테이블 값에서 가져와서 쓸것
         *          애니메이션 컨트롤러 또한 게임 매니저에서 가져와서 쓸것
         *          키 컨트롤도 게임매니저로 빠질것
         *          
         */
    }
}