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
    #endregion

}
