using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeExplosion : MonoBehaviour {

    #region Grenad Explosion INFO
    [SerializeField] private Grenade m_grenade = null;
    #endregion

    void AnimationEnd()
    {
        if(m_grenade.IS_NETWORK)
        {
            m_grenade.GrenadeBoomEnd();
        }
    }
}
