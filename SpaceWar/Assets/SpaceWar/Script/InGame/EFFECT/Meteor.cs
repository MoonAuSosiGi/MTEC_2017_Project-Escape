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

    void Update()
    {
        Vector3 pos = transform.parent.position;

    }

    // 메테오가 지면에 충돌 했을 때 
    public void MeteorAnimationEvent()
    {
        // 이펙트 보여주기 
        CameraManager.Instance().ShowMeteorCameraEffect(Vector3.Distance(transform.position ,
            GameManager.Instance().PLAYER.m_player.transform.position));

        m_meteorSoundLoopSource.clip = m_shotIDLESound;
        m_meteorSoundLoopSource.Play();
    }

    public void MeteorShotSound()
    {
        m_meteorOneShotSource.Play();
    }

    // 메테오가 끝났다.
    public void MeteorAnimationEnd()
    {
        transform.GetChild(0).GetComponent<SphereCollider>().enabled = false;

        GameObject.Destroy(transform.parent.gameObject);
    }
}
