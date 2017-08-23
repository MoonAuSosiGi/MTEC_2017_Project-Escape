using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shelter : MonoBehaviour
{

    #region Shelter_INFO
    // animation 은 0 닫힘 1 열어라 2 불까지 꺼라

    private bool m_hasPlayer = false;
    public bool HAS_PLAYER { get { return m_hasPlayer; } set { m_hasPlayer = value; } }

    private int m_shelterID = 0;
    private bool m_curState = false;

    public int SHELTER_ID
    {
        get { return m_shelterID; }
        set { m_shelterID = value; }
    }

    public AudioSource m_shelterSoundSource = null;
    public AudioSource m_shelterInOutSource = null;
    public AudioClip m_openSound = null;
    public AudioClip m_closeSound = null;
    public AudioClip m_inIdleSound = null;
    public AudioClip m_outIdleSound = null;

    #endregion

    #region UnityMethod
    void Start()
    {
        m_shelterID = NetworkManager.Instance().GetShelterIndex(this);
    }


    #endregion

    #region Shelter_Method

    // 쉘터 문 트리거에서 감지한다.
    public void ShelterEnter()
    {
        if (HAS_PLAYER)
            return;
        HAS_PLAYER = true;
        NetworkManager.Instance().C2SRequestShelterEnter(m_shelterID , true);
    }

    public void ShelterExit()
    {
        if (!HAS_PLAYER)
            return;
        HAS_PLAYER = false;
        NetworkManager.Instance().C2SRequestShelterEnter(m_shelterID , false);
    }

    public void DoorControl()
    {
        if (!m_curState)
            OpenDoor();
        else
            CloseDoor();
    }

    public void OpenDoor(bool networkOrder = false)
    {
        if (m_curState)
            return;
        Debug.Log("OpenDoor");
        m_curState = true;

        m_shelterSoundSource.clip = m_openSound;
        m_shelterSoundSource.Play();




        // 열렸다.
        GetComponent<Animator>().SetInteger("DOOR_OPEN_STATE" , 1);
        if (networkOrder == false)
            NetworkManager.Instance().C2SRequestShelterDoorControl(m_shelterID , true);
    }

    public void CloseDoor(bool networkOrder = false)
    {
        if (!m_curState)
            return;
        Debug.Log("CloseDoor");
        m_curState = false;

        m_shelterSoundSource.clip = m_closeSound;
        m_shelterSoundSource.Play();
        // 닫혔다
        GetComponent<Animator>().SetInteger("DOOR_OPEN_STATE" , 2);
        if (networkOrder == false)
            NetworkManager.Instance().C2SRequestShelterDoorControl(m_shelterID , false);
    }

    public void LightOn()
    {
        Debug.Log("LightOn");
        //   GetComponent<Animator>().Play("LightON");
        GetComponent<Animator>().SetInteger("LIGHT_STATE" , 1);

        if (GameManager.Instance().PLAYER.m_player.IS_SHELTER)
        {
            m_shelterSoundSource.clip = m_inIdleSound;
            m_shelterSoundSource.Play();
        }
        else
        {
            m_shelterSoundSource.clip = m_outIdleSound;
            m_shelterSoundSource.Play();
        }
    }

    public void LightOff()
    {
        Debug.Log("LightOff");
        // 아무도 없다
        //  GetComponent<Animator>().Play("LightOFF");
        GetComponent<Animator>().SetInteger("LIGHT_STATE" , 2);
        m_shelterSoundSource.clip = null;
        m_shelterSoundSource.Stop();
    }

    #endregion
}
