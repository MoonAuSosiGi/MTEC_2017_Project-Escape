using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBullet : Bullet {

    #region RocketBullet_INFO
    private float m_gravityPower = 0.0f;
    private Vector3 m_gravityPosition = Vector3.zero;
    #endregion

    #region Bullet Process
    public override void BulletMove()
    {
        this.transform.RotateAround(
            GravityManager.Instance().CurrentPlanet.transform.position , m_shotRot * Vector3.right,
            m_speed  * Time.deltaTime);

        Vector3 dir = (m_gravityPosition - transform.position).normalized;
        transform.position += dir * m_gravityPower * Time.deltaTime;

        m_gravityPower += 5f * Time.deltaTime;
        MoveSend();
    }

    public override void BulletSetup()
    {
        base.BulletSetup();
        m_gravityPower = 0.0f;
        m_gravityPosition = GravityManager.Instance().transform.position;
    }
    protected override void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Weapon") && !other.CompareTag("Bullet") && !other.CompareTag("DeathZone"))
        {
            m_hitEnemy = true;
            // 상관 없는 이펙트 
            if (m_shotEffect != null)
                m_shotEffect.SetActive(true);
            if(m_hitMain != null)
            {
                m_bulletAudioSource.clip = m_hitMain;
                m_bulletAudioSource.Play();
            }
            

            if (other.CompareTag("PlayerCharacter"))
            {
                NetworkPlayer p = other.transform.GetComponent<NetworkPlayer>();

                if (p != null)
                {

                    NetworkManager.Instance().C2SRequestPlayerDamage((int)p.m_hostID , p.m_userName , "test" , Random.Range(10.0f , 15.0f) , m_startPos);
                }
                else
                {
                    m_hitEnemy = false;
                    return;
                }
            }
            else if (string.IsNullOrEmpty(other.tag) || other.CompareTag("NonSpone"))
            {
                // 여기에 부딪치면 다른 이펙트를 보여준다.
                if (m_shotOtherObjectEffect != null)
                    m_shotOtherObjectEffect.SetActive(true);
            }
            else
            {
                // 기타 오브젝트
                if (m_shotEffect != null)
                    m_shotEffect.SetActive(true);
            }



            BulletDelete();
            if (NetworkManager.Instance() != null)
                NetworkManager.Instance().C2SRequestBulletRemove(m_networkID);
        }

    }
    #endregion

}
