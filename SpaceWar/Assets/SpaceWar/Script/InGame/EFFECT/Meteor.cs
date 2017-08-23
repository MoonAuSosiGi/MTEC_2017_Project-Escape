using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour {

    #region Meteor_INFO
    public AudioSource m_meteorOneShotSource = null;
    public AudioSource m_meteorSoundLoopSource = null;
    public AudioClip m_moveSound = null;
    public AudioClip m_shotIDLESound = null;
    #endregion

    void Start()
    {
        m_meteorSoundLoopSource.clip = m_moveSound;
        m_meteorSoundLoopSource.Play();
    }

    public void MeteorAnimationEvent()
    {
        Camera.main.GetComponent<UBER_GlobalParams>().enabled = true;
        

        m_meteorSoundLoopSource.clip = m_shotIDLESound;
        m_meteorSoundLoopSource.Play();
    }

    public void MeteorShotSound()
    {
        m_meteorOneShotSource.Play();
    }

    public void MeteorAnimationEnd()
    {
        Camera.main.GetComponent<UBER_GlobalParams>().enabled = false;

        GameObject.Destroy(transform.parent.gameObject);
    }
}
