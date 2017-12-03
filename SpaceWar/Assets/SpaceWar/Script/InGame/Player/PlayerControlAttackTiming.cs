using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlAttackTiming : MonoBehaviour {

    #region PlayerControlAttackTiming_INFO
    [SerializeField] private PlayerController m_player = null;
    [SerializeField] private AudioSource m_walkSource = null;
    [SerializeField] private AudioClip m_walkSound_land = null;
    [SerializeField] private AudioClip m_walkSound_land2 = null;
    [SerializeField] private AudioClip m_walkSound_shelter = null;
    [SerializeField] private AudioClip m_walkSound_water = null;
    [SerializeField] private AudioClip m_walkSound_water2 = null;
    private bool m_isWater = false;
    #endregion

    public void ItsAttackTime()
    {
        m_player.AttackAnimationEvent();
    }

    public void AttackEnd()
    {
        m_player.AttackAnimationEnd();
    }

    public void DeadAnimationEnd()
    {
        if(m_player.enabled == true)
        {
            m_player.DeadAnimationEnd();
        }
    }

    public void FootSoundPlay()
    {
        AudioClip clip = null;
        if(m_player != null)
        {
            if (m_player.IS_SHELTER)
                clip = m_walkSound_shelter;
            else
            {
                if (m_isWater == false)
                    clip = (Random.Range(0 , 2) == 0) ? m_walkSound_land : m_walkSound_land2;
                else
                    clip = (Random.Range(0 , 2) == 0) ? m_walkSound_water : m_walkSound_water2;
            }
        }

        m_walkSource.clip = clip;
        m_walkSource.Play();

    }

    public void WaterSoundChange(bool b)
    {
        m_isWater = b;
    }

    public void DashAnimationEnd()
    {
        m_player.DashAnimationEnd();
    }
}
