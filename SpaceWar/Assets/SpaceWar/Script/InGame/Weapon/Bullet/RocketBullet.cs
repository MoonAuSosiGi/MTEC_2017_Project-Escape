using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBullet : Bullet {

    #region RocketBullet_INFO
    private float m_gravityPower = 0.0f;
    public float GRAVITY_POWER { get { return m_gravityPower; } set { m_gravityPower = value; } }
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
        Vector3 velo = (m_shotRot * Vector3.right * m_speed * Time.deltaTime); // + (dir * m_gravityPower * Time.deltaTime);
        m_gravityPower += 5f * Time.deltaTime;
        
        MoveSend(velo);
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
                    if (IS_REMOTE == true && TARGET_ID == (int)p.HOST_ID)
                        return;

                    if (m_shotEffect != null)
                        m_shotEffect.SetActive(true);
                    BULLET_TRAIL_EFFECT.SetActive(false);
                    if (IS_REMOTE == false)
                        NetworkManager.Instance().C2SRequestPlayerDamage((int)p.m_hostID , p.m_userName , m_weaponID , m_damage , m_startPos);
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
                BULLET_TRAIL_EFFECT.SetActive(false);
            }
            else
            {
                // 기타 오브젝트
                if (m_shotEffect != null)
                    m_shotEffect.SetActive(true);
                BULLET_TRAIL_EFFECT.SetActive(false);
            }
            
            this.GetComponent<SphereCollider>().enabled = false;
            Invoke("BulletDelete" , 1.5f);
        }

    }
    #endregion

}
