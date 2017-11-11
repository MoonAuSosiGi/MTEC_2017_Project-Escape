using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletParticle_Off : MonoBehaviour {

    #region Bullet Particle Off INFO
    private ParticleSystem m_targetParticleSystem = null;
    #endregion

    #region Unity Method
    void Start()
    {
        m_targetParticleSystem = this.GetComponent<ParticleSystem>();
    } 
    #endregion

    public void BulletHitEvent()
    {
        Debug.Log("Particle Off");
        gameObject.SetActive(false);
    }

    public void Reset()
    {
        Debug.Log("Particle Reset ");
        gameObject.SetActive(true);
    }
}
