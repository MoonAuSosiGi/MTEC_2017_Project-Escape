using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceShip : MonoBehaviour {

    #region SpaceShip_INFO
    float m_fuel = 0.0f;
    bool m_isEnd = false;


    private int m_spaceShipID = 0;

    public int SPACESHIP_ID
    {
        get { return m_spaceShipID; }
        set { m_spaceShipID = value; }
    }

    #endregion

    #region UnityMethod
    void Start()
    {
        SPACESHIP_ID = NetworkManager.Instance().GetSpaceShipIndex(this);
    }
    #endregion

    public void StartSpaceShipEngineCharge()
    {
        if (!m_isEnd)
            m_fuel = 0.0f;
        GameManager.Instance().m_inGameUI.StartSpaceShipUI();
    }

    public void StopSpaceShipEngineCharge()
    {
        if(!m_isEnd)
            m_fuel = 0.0f;
        GameManager.Instance().m_inGameUI.StopSpaceShipUI();
    }
    
    public void SpaceShipEngineCharge(float t,bool me)
    {
        if (m_isEnd)
            return;

        if (me)
        {
            m_fuel += t;
            GameManager.Instance().m_inGameUI.ProcessSpaceShipEngine(m_fuel);
            NetworkManager.Instance().C2SNotifySpaceShipEngineCharge(m_spaceShipID, m_fuel);
            
        }
        else
            m_fuel = t;
        this.GetComponent<Animator>().SetFloat("SPACE_SHIP_SETTING" , m_fuel);

        if(m_fuel > 1.0f)
        {
            if(me)
            {
                GameManager.Instance().WINNER = true;
                GameManager.Instance().m_inGameUI.StopSpaceShipUI();
                Camera.main.GetComponent<CamRotate>().enabled = false;
                Camera.main.fieldOfView = 70.0f;
                Camera.main.transform.SetParent(transform.GetChild(0).GetChild(0) , false);
                Camera.main.transform.localEulerAngles = new Vector3(0.0f , 0.0f , 0.0f);
                Camera.main.transform.localPosition = new Vector3(0.0f , 0.0f , 0.0f);
                GameManager.Instance().PLAYER.m_player.gameObject.SetActive(false);
            }
          
            m_isEnd = true;
        }
    }

    public void SpaceShipAnimationEnd()
    {
       
        // 씬 이동
        SceneManager.LoadScene("Space_1Result");
    }

    void OnTriggerEnter(Collider col)
    {
        if (!col.CompareTag("PlayerCharacter"))
            return;

        var p = col.GetComponent<Player>();

        if (p.enabled)
        {
            // 실 플레이어일 경우에만
            p.IsMoveable = false;
            p.IsJumpable = false;
        }
    }

    
}
