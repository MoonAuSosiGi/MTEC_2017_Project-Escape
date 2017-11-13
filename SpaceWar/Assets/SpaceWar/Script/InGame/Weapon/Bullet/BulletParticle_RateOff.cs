using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletParticle_RateOff : MonoBehaviour
{

    #region BulletParticle RateOff
    private ParticleSystem m_targetParticleSystem = null;
    private float m_startValue = 0.0f;
    #endregion


    public void BulletHitEvent()
    {
        if(m_targetParticleSystem == null)
        {
            m_targetParticleSystem = this.GetComponent<ParticleSystem>();
            m_startValue = m_targetParticleSystem.emission.rateOverTime.constant;
        }
        var em = m_targetParticleSystem.emission;
        em.enabled = true;
        em.rateOverTime = 0.0f;

        //test
        Debug.Log("bullet Hit Event " + m_targetParticleSystem.emission.rateOverTime.constant);
        
    }

    public void Reset()
    {
        var em = m_targetParticleSystem.emission;
        em.enabled = true;
        em.rateOverTime = m_startValue;

        //test
        Debug.Log("Reset " + m_targetParticleSystem.emission.rateOverTime.constant + " enabled " + m_targetParticleSystem.emission.enabled);

    }
}
