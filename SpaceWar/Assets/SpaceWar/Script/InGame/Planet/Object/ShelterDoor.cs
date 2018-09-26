using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* @brief		ShelterDoor :: 쉘터 문
* @details		쉘터 입장 / 퇴장 여부를 판단한다.
* @author		이훈 (MoonAuSosiGi@gmail.com)
* @date			2017-11-06
* @file			ShelterDoor.cs
* @version		0.0.1
*/
namespace TimeForEscape.Object
{
    public class ShelterDoor : MonoBehaviour
    {
        #region ShelterDoor_INFO --------------------------------------------------------------
        [SerializeField] private bool m_enter = true; ///< 입장을 체크할 문인지 여부
        [SerializeField] private Shelter m_targetShelter = null; ///< 이 문이 붙어있는 쉘터
        [SerializeField] private Material m_colorMat = null;  ///< 원본 재질 
        [SerializeField] private Material m_opacityMat = null; ///<  투명 셰이더 재질
        #endregion ----------------------------------------------------------------------------

        /**
         * @brief   문에 들어왔을 때 혹은 나갔을 때에 대한 처리
         * @details  이 함수는 유니티 자체 충돌체크 함수
         * @todo    예전 코드인 투명 셰이더 재질 관련 제거
         * @param   other 부딪힌 무언가. 플레이어인지 검사해야함
         */
        private void OnTriggerEnter(Collider other)
        {
            // 플레이어 컨트롤러 태그 체크
            if (!other.CompareTag("PlayerCharacter"))
                return;

            PlayerController p = other.GetComponent<PlayerController>();

            // 정상적으로 얻어온게 아니라면 패스
            if (p == null)
                return;

            var np = other.GetComponent<NetworkPlayer>();

            if (np != null && NetworkManager.Instance().NETWORK_PLAYERS.IndexOf(np) !=
                GameManager.Instance().PLAYER.m_player.OBSERVER_INDEX)
                return;

            // 들어오는 문일 때의 처리
            if (m_enter)
            {
                // 쉘터 안에 들어왔음을 넣음
                p.IS_SHELTER = true;
                // 들어왔음을 쉘터에 알려준다.
                m_targetShelter.ShelterEnter();
                
                // 들어갈때의 이펙트
                ShelterEnterEffect();

                //transform.parent.parent.GetChild(2).GetComponent<MeshRenderer>().material = m_opacityMat;
            }
            // 나가는 문일 때의 처리
            else
            {
                // 쉘터 밖에 나갔음을 넣음
                p.IS_SHELTER = false;
                // 나갔음을 쉘터에 알려준다.
                m_targetShelter.ShelterExit();

                // 나갈때의 이펙트
                ShelterExitEffect();
                //   transform.parent.parent.GetChild(2).GetComponent<MeshRenderer>().material = m_colorMat;
            }
        }

        /**
         * @brief   쉘터 들어갈때의 이펙트
         * @details  투명하게 바꾸는 셰이더 tween
         */
        void ShelterEnterEffect()
        {
            // 기존 재생 tween 은 제거
            if (iTween.Count(gameObject) > 0)
                iTween.Stop(gameObject);

            // 현재 값을 기준으로 해야하므로 얻어옴
            float v = transform.parent.parent.
                GetChild(2).GetComponent<MeshRenderer>().material.GetFloat("_Cutoff");

            // 점점 투명해진다.
            iTween.ValueTo(gameObject , iTween.Hash(
                "from" , v , "to" , 1.0f - 0.015f ,
                "onupdatetarget" , gameObject ,
                "onupdate" , "DoorUpdate" ,
                "time" , 0.5f));
        }

        /**
         * @brief   쉘터 나갔을때의 이펙트
         * @details  원래대로 바꾸는 셰이더 tween
         */
        void ShelterExitEffect()
        {
            // 기존 재생 tween 은 제거
            if (iTween.Count(gameObject) > 0)
                iTween.Stop(gameObject);

            // 현재 값을 기준으로 해야하므로 얻어옴
            float v = transform.parent.parent.
                 GetChild(2).GetComponent<MeshRenderer>().material.GetFloat("_Cutoff");

            // 점점 원래대로 돌아온다.
            iTween.ValueTo(gameObject , iTween.Hash(
                "from" , v , "to" , 0.0f ,
                "onupdatetarget" , gameObject ,
                "onupdate" , "DoorUpdate" ,
                "time" , 0.5f));
        }

        /**
         * @brief   이펙트 tween 함수
         * @details  itween 에서 호출해야함
         * @param   v 변경될 AlphaCutOff 값
         */
        void DoorUpdate(float v)
        {
            transform.parent.parent.
                 GetChild(2).GetComponent<MeshRenderer>().material.SetFloat("_Cutoff" , v);
        }
    }
}