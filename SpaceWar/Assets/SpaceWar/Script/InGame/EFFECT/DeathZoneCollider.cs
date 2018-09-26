using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZoneCollider : MonoBehaviour {

    #region Death Zone Collider INFO ------------------------------------------------------------------------
    [SerializeField] private DeathZone m_targetZone = null;

    List<HitObject> m_hitList = new List<HitObject>();

    private struct HitObject
    {
        public GameObject hitObj;
        public float tick;
        public HitObject(GameObject hitObj,float tick)
        {
            this.hitObj = hitObj; this.tick = tick;
        }
    }
    #endregion

    void Update()
    {
        for(int i = 0; i < m_hitList.Count; i++)
        {
            HitObject obj = m_hitList[i];
            m_hitList[i] = new HitObject(obj.hitObj , obj.tick - Time.deltaTime);
         // Debug.Log("hit " + m_hitList[i].tick);
        }
    }

    void OnTriggerStay(Collider col)
    {

        if (col.transform.CompareTag("PlayerCharacter"))
        {
            // 사망한 친구들은 처리하지 않음
            NetworkPlayer nplayer = col.GetComponent<NetworkPlayer>();

            if (nplayer != null)
            {
                if (nplayer.IS_DEATH)
                    return;
            }
            else
            {
                if (GameManager.Instance().PLAYER.m_player.IS_DEATH())
                    return;
            }

            bool check = false;
            for(int i = 0; i < m_hitList.Count;i++)
            {
                // 있을 경우엔 시간 체크
                float tick = m_hitList[i].tick;
                if(m_hitList[i].hitObj == col.gameObject)
                {
                    check = true;
                    if (tick <= 0.0f)
                    {
                        // 생성
                        m_targetZone.DeathZoneHit(col.gameObject);
                        m_hitList[i] = new HitObject(m_hitList[i].hitObj , 1.0f);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            if(check == false)
            {
                m_targetZone.DeathZoneHit(col.gameObject);
                m_hitList.Add(new HitObject(col.gameObject , 1.0f));
            }
           
        }
    }

  
}
