using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nettention.Proud;

namespace TimeForEscape.Object.Weapon
{
    /**
    * @brief		Satellite :: 전투 위성
    * @details		전투 위성
    * @author		이훈 (MoonAuSosiGi@gmail.com)
    * @date			2017-11-15
    * @file			Satellite.cs
    * @version		0.0.1
    */
    public class Satellite : NetworkObject
    {
        #region Satellite ----------------------------------------------------------------------------------
        [SerializeField]
        GameObject m_hitBottom = null; ///< 밑바닥에 닿는 부분
        [SerializeField]
        GameObject m_hitCenter = null; ///< 센터에 닿는 부분
        private float m_damage = 0.0f; ///< 데미지
        private float m_distance = 0.0f; ///< 위성 포 거리 조절용 변수
        private UnityEngine.Vector3 m_startPos = UnityEngine.Vector3.zero; ///< 처음  좌표
        #region Satellite Property -------------------------------------------------------------------------
        /**
         * @brief   데미지에 대한 프로퍼티
         */
        public float DAMAGE
        {
            get { return m_damage; }
            set { m_damage = value; }
        }
        #endregion -----------------------------------------------------------------------------------------
        #endregion -----------------------------------------------------------------------------------------
        #region Satellite Method ---------------------------------------------------------------------------

        void Awake()
        {

        }

        /**
         * @brief   시작시 종료 시점 세팅
         */
        void Start()
        {
            m_speed = 2.0f;
            m_damage = WeaponManager.Instance().GetWeaponData("G_Satellite").Damage;
            var pup = (transform.position - GravityManager.Instance().CurrentPlanet.transform.position).normalized;

            Quaternion rot = Quaternion.FromToRotation(transform.up , pup) * transform.rotation;
            transform.rotation = rot;

            m_distance = UnityEngine.Vector3.Distance(transform.GetChild(0).position, m_hitCenter.transform.parent.position);
            m_startPos = m_hitCenter.transform.parent.position;
            // temp
            Invoke("SatelliteEnd" , 10.0f);
        }

        /**
         * @brief   지속적으로 충돌체크하고 위치를 조절
         */
        void Update()
        {
            var dir = GravityManager.Instance().CurrentPlanet.transform.position - m_hitCenter.transform.parent.position;
            dir.Normalize();
            Ray ray = new Ray(transform.GetChild(0).position , dir);
            RaycastHit[] hit = Physics.RaycastAll(ray);

            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].transform.CompareTag("Satellite"))
                    continue;
                var up = transform.up;
                up.Normalize();
                m_hitCenter.transform.parent.position = hit[i].point + up;// * 0.025f;
                float dis = UnityEngine.Vector3.Distance(m_hitCenter.transform.parent.position, transform.GetChild(0).position);
                float value = dis / m_distance;
                
                m_hitCenter.transform.localScale = new UnityEngine.Vector3(1.0f , value , 1.0f);
                break;
            }
            Debug.DrawRay(transform.GetChild(0).position , dir , Color.yellow);
        }
        /**
         * @brief   새틀라이트 종료 애니메이션 시작
         */
        void SatelliteEnd()
        {
            this.GetComponent<Animator>().SetInteger("END" , 1);
        }

        /**
         * @brief   삭제 시점!
         */
        void SatelliteAnimationEnd()
        {
            WeaponManager.Instance().RequestNetworkObjectRemove(m_networkID);
        }


        /**
         * @brief   충돌시 데미지 전송
         */
        void OnTriggerStay(Collider other)
        {
            HitTarget(other);
        }

        /**
         * @brief   데미지를 주는 로직
         */
        public void HitTarget(Collider col)
        {
            if (col.CompareTag("PlayerCharacter") == false)
                return;

            {
                var p = col.GetComponent<PlayerController>();
                var np = col.GetComponent<NetworkPlayer>();

                // 네트워크 상의 총알에서만 판단
                if (p != null && p.IS_SHELTER == false && np == null && m_isNetwork == true)
                {
                    NetworkManager.Instance().C2SRequestPlayerDamage(m_createHostID,(int)NetworkManager.Instance().HOST_ID ,
                        NetworkManager.Instance().USER_NAME , "SATELLITE" , m_damage , transform.position);

                }
            }
        }
        #endregion -----------------------------------------------------------------------------------------
    }
}