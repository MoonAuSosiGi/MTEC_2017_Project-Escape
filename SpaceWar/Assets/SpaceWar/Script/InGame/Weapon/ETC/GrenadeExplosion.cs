using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeExplosion : MonoBehaviour {

    #region Grenad Explosion INFO
    [SerializeField] private Grenade m_grenade = null;
    public Grenade TARGET_GRENADE { get { return m_grenade; } set { m_grenade = value; } }
    #endregion

    void AnimationEnd()
    {
        if(m_grenade.IS_NETWORK)
        {
            
        }
    }

    void OnTriggerEnter(Collider col)
    {
        Debug.Log("Boom " + col.name);
    }
}
