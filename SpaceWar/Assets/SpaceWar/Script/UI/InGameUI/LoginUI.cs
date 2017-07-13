using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginUI : MonoBehaviour {

    #region LoginUI_Info
    public UILabel m_inputLabel = null;

    #endregion

    void Start()
    {
        NetworkManager.Instance().loginResult += (bool result) =>
        {
            if(result)
            {
                gameObject.SetActive(false);
                GameManager.Instance().OnJoinedRoom(GameManager.Instance().PLAYER.m_name , true ,
                   new UnityEngine.Vector3(0.0f , 80.0f , 0.0f));
                GameManager.Instance().m_inGameUI.gameObject.SetActive(true);
            }
            else
            {
                m_inputLabel.text = "서버가 켜져있지 않거나 잘못된 서버주소";
            }
        };
    }

    public void SeverConnectButton()
    {
        NetworkManager.Instance().SERVER_IP = m_inputLabel.text;
        NetworkManager.Instance().ServerConnect();
    }

    void LoginResult(bool result)
    {

    }

}
