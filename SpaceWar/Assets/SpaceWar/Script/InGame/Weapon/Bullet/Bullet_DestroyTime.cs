using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_DestroyTime : MonoBehaviour {

    #region Bullet DestroyTime INFO

    // 몇초 후에 제거될 것인가 ::* 실 제거가 아님 
    [SerializeField] private float m_destroyTime = 2.0f;
    private Bullet m_targetBullet = null;

    public Bullet TARGET_BULLET { get { return m_targetBullet; }set { m_targetBullet = value; } }
    #endregion

    public void BulletHitEvent()
    {
        
        Invoke("HideBullet" , m_destroyTime);   
    }

    void HideBullet()
    {
        if(m_targetBullet != null)
        {
            m_targetBullet.BulletDelete();
        }
    }
}
