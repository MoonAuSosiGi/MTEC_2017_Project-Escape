using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nettention.Proud;

namespace TimeForEscape.Object
{
    /**
     * @brief		NetworkObject 네트워크 동기화 오브젝트
     * @details		네트워크 동기화 오브젝트로, 이동 동기화 등의 작업을 수행
     * @author		이훈 (MoonAuSosiGi@gmail.com)
     * @date		2017-11-20
     * @file		NetworkObject.cs
     * @version		0.0.1
   */
    public class NetworkObject : MonoBehaviour
    {
        #region NetworkObject INFO ------------------------------------------------------------------
        protected WeaponManager.NetworkObjectType m_type =
            WeaponManager.NetworkObjectType.NONE; ///< 오브젝트 타입 ( 이것으로 식별 )
        protected Quaternion m_dirRot; ///< 진행방향
        protected float m_speed = 0.0f; ///< 이동 속도

        #region NetworkObject Network INFO ----------------------------------------------------------
        protected string m_networkID = null; ///< 네트워크 식별 아이디
        protected bool m_isNetwork = false; ///< 다른 사람이 만든 오브젝트인지
        protected bool m_isNetworkMoving = true; ///< 동기화 오브젝트인 경우 계속 좌표 동기화를 할건지
        protected PositionFollower m_positionFollower = null; ///< 위치 동기화용
        protected AngleFollower m_angleFollowerX = null; ///< X 회전 동기화
        protected AngleFollower m_angleFollowerY = null; ///< y 회전 동기화
        protected AngleFollower m_angleFollowerZ = null; ///< z 회전 동기화
        #endregion ----------------------------------------------------------------------------------

        #region NetworkObject Property --------------------------------------------------------------
        /**
         * @brief   네트워크 식별 아이디에 대한 프로퍼티
         */
        public string NETWORK_ID
        {
            get { return m_networkID; }
            set { m_networkID = value; }
        }

        /**
         * @brief   진행방향에 관련된 프로퍼티
         */
        public Quaternion DIR_ROT
        {
            get { return m_dirRot; }
            set { m_dirRot = value; }
        }

        /**
         * @brief   네트워크 오브젝트인지에 대한 프로퍼티
         */
        public bool IS_NETWORK
        {
            get { return m_isNetwork; }
            set { m_isNetwork = value; }
        }

        /**
         * @brief   네트워크 동기화 오브젝트일때 , 이동을 계속 할 건지에 대한 프로퍼티
         */
        public bool IS_NETWORK_MOVING
        {
            get { return m_isNetworkMoving; }
            set { m_isNetworkMoving = value; }
        }

        /**
         * @brief   오브젝트 타입에 대한 프로퍼티
         */
        public WeaponManager.NetworkObjectType OBJ_TYPE
        {
            get { return m_type; }
            set { m_type = value; }
        }

        /**
         * @brief   속도에 관한 프로퍼티
         */
        public float SPEED
        {
            get { return m_speed; }
            set { m_speed = value; }
        }
        #endregion ----------------------------------------------------------------------------------
        #endregion ----------------------------------------------------------------------------------
        #region NetworkObject Method ----------------------------------------------------------------

        /**
         * @brief   Frame Update
         */
        void FixedUpdate()
        {
            if (m_isNetwork == true && m_isNetworkMoving == true)
            {
                NetworkUpdate();
            }
            else
            {
                MoveUpdate();
            }
        }

        /**
         * @brief   이동 관련 업데이트 로직
         * @detail  FixedUpdate 에서 호출됨
         */
        protected virtual void MoveUpdate()
        {
            UnityEngine.Vector3 velo = m_dirRot * 
                UnityEngine.Vector3.right * m_speed * Time.deltaTime;

            this.transform.RotateAround(
                GravityManager.Instance().CurrentPlanet.transform.position ,
                m_dirRot * UnityEngine.Vector3.right ,
                m_speed * Time.deltaTime);

            MoveSend(velo);
        }

        /**
         * @brief   네트워크 좌표 전송
         */
        protected void MoveSend(UnityEngine.Vector3 velo)
        {

            if (NetworkManager.Instance() != null)
                NetworkManager.Instance().C2SRequestNetworkObjectMove(m_networkID ,
                    transform.position , velo , transform.localEulerAngles);
        }

        #region Network Logic -----------------------------------------------------------------------
        /**
        * @brief   네트워크 오브젝트일 경우 
        */
        public void NetworkEnable()
        {
            m_positionFollower = new PositionFollower();
            m_angleFollowerX = new AngleFollower();
            m_angleFollowerY = new AngleFollower();
            m_angleFollowerZ = new AngleFollower();
        }
        /**
         * @brief   네트워크 동기화
         * @param   pos 동기화할 좌표
         * @param   velocity 동기화할 속도
         * @param   rot 동기화할 회전값
         */
        public void NetworkMoveRecv(UnityEngine.Vector3 pos , UnityEngine.Vector3 velocity , UnityEngine.Vector3 rot)
        {
            var npos = new Nettention.Proud.Vector3();
            npos.x = pos.x;
            npos.y = pos.y;
            npos.z = pos.z;

            var nvel = new Nettention.Proud.Vector3();
            nvel.x = velocity.x;
            nvel.y = velocity.y;
            nvel.z = velocity.z;

            m_positionFollower.SetTarget(npos , nvel);

            m_angleFollowerX.TargetAngle = rot.x * Mathf.Deg2Rad;
            m_angleFollowerY.TargetAngle = rot.y * Mathf.Deg2Rad;
            m_angleFollowerZ.TargetAngle = rot.z * Mathf.Deg2Rad;
        }

        /**
         * @brief   동기화된 좌표로 업데이트
         */
        void NetworkUpdate()
        {
            if (m_isNetworkMoving == false)
                return;
            m_positionFollower.FrameMove(Time.deltaTime);
            m_angleFollowerX.FrameMove(Time.deltaTime);
            m_angleFollowerY.FrameMove(Time.deltaTime);
            m_angleFollowerZ.FrameMove(Time.deltaTime);

            m_angleFollowerX.FollowerAngleVelocity = 200 * Time.deltaTime;
            m_angleFollowerY.FollowerAngleVelocity = 200 * Time.deltaTime;
            m_angleFollowerZ.FollowerAngleVelocity = 200 * Time.deltaTime;

            var p = new Nettention.Proud.Vector3();
            var vel = new Nettention.Proud.Vector3();

            m_positionFollower.GetFollower(ref p , ref vel);
            transform.position = new UnityEngine.Vector3((float)p.x , (float)p.y , (float)p.z);

            float fx = (float)m_angleFollowerX.FollowerAngle * Mathf.Rad2Deg;
            float fy = (float)m_angleFollowerY.FollowerAngle * Mathf.Rad2Deg;
            float fz = (float)m_angleFollowerZ.FollowerAngle * Mathf.Rad2Deg;
            var rotate = Quaternion.Euler(fx , fy , fz);
            transform.localRotation = rotate;
        }
        #endregion ----------------------------------------------------------------------------------

        #endregion ----------------------------------------------------------------------------------
    }
}