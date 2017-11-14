using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlAttackTiming : MonoBehaviour {

    #region PlayerControlAttackTiming_INFO
    [SerializeField] private PlayerController m_player = null;
    [SerializeField] private AudioSource m_walkSource = null;
    [SerializeField] private AudioClip m_walkSound_land = null;
    [SerializeField] private AudioClip m_walkSound_shelter = null;
    
    #endregion

    public void ItsAttackTime()
    {
        m_player.AttackAnimationEvent();
    }

    public void AttackEnd()
    {
        m_player.AttackAnimationEnd();
    }

    public void FootSoundPlay()
    {
        AudioClip clip = null;
        if(m_player != null)
        {
            if (m_player.IS_SHELTER)
                clip = m_walkSound_shelter;
            else
                clip = m_walkSound_land;
        }
        else
        {
            // loading 임시
            clip = m_walkSound_shelter;
        }
      

        m_walkSource.clip = clip;
        m_walkSource.Play();

    }

    public void DashAnimationEnd()
    {
        m_player.DashAnimationEnd();
    }
}
