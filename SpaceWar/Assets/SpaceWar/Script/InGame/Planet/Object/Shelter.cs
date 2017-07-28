using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shelter : MonoBehaviour {

    #region Shelter_INFO
    // animation 은 0 닫힘 1 열어라 2 불까지 꺼라

    private bool m_hasPlayer = false;
    private Player m_player = null;
    public bool HAS_PLAYER { get { return m_hasPlayer; } set { m_hasPlayer = value; } }

    private int m_shelterID = 0;
    private bool m_curState = false;

    public int SHELTER_ID
    {
        get { return m_shelterID; }
        set { m_shelterID = value; }
    }

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

    public void OpenDoor()
    {
        if (m_curState)
            return;
        Debug.Log("OpenDoor");
        m_curState = true;
      
        // 열렸다.
        GetComponent<Animator>().SetInteger("DOOR_OPEN_STATE" , 1);
        NetworkManager.Instance().C2SRequestShelterDoorControl(m_shelterID , true);
    }

    public void CloseDoor()
    {
        if (!m_curState)
            return;
        Debug.Log("CloseDoor");
        m_curState = false;
        // 닫혔다
        GetComponent<Animator>().SetInteger("DOOR_OPEN_STATE" , 2);
        NetworkManager.Instance().C2SRequestShelterDoorControl(m_shelterID , false);
    }

    public void LightOn()
    {
        Debug.Log("LightOn");
     //   GetComponent<Animator>().Play("LightON");
        GetComponent<Animator>().SetInteger("LIGHT_STATE" , 1);
    }

    public void LightOff()
    {
        Debug.Log("LightOff");
        // 아무도 없다
      //  GetComponent<Animator>().Play("LightOFF");
        GetComponent<Animator>().SetInteger("LIGHT_STATE" , 2);
    }

    #endregion
}
