using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBulletExplosion : MonoBehaviour {

    #region Rocket Bullet Explosion
    [SerializeField] private WeaponItem m_weapon = null;
    public WeaponItem ROCKET { get { return m_weapon; } set { m_weapon = value; } }

    public void AnimationEnd()
    {
        gameObject.SetActive(false);
    }


    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("PlayerCharacter"))
        {
            NetworkPlayer p = col.transform.GetComponent<NetworkPlayer>();

            if(p!= null)
            {
                NetworkManager.Instance().C2SRequestPlayerDamage((int)p.m_hostID , p.m_userName , m_weapon.ITEM_ID.ToString() ,m_weapon.DAMAGE , transform.position);
            }

        }
            
    }

    #endregion
}
