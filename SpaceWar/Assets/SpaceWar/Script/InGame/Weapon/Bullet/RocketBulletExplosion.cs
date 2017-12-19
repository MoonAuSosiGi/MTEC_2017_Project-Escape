using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBulletExplosion : MonoBehaviour {

    #region Rocket Bullet Explosion

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
                var b = transform.parent.GetComponent<Bullet>();
                NetworkManager.Instance().C2SRequestPlayerDamage((int)p.m_hostID , p.m_userName ,
                    b.TARGET_WEAPON.ITEM_ID.ToString() ,
                    b.TARGET_WEAPON.DAMAGE , transform.position);
            }

        }
            
    }

    #endregion
}
