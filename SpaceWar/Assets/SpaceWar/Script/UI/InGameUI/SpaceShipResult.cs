using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipResult : MonoBehaviour {

    [SerializeField] private Camera m_camera = null;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (NetworkManager.Instance().IS_DRAW)
        {
            NetworkManager.Instance().RequestDrawGame();
            ResultUIAlready();
            m_camera.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }else if (NetworkManager.Instance().IS_LOSE)
        {
            ResultAnimationStart();
            ResultUIAlready();
            m_camera.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
           // win
            GameManager.Instance().GetComponent<AudioSource>().Play();
        }
    }

    public void ResultAnimationStart()
    {
        NetworkManager.Instance().C2SRequestGameEnd();
    }

    public void ResultUIAlready()
    {
        GameManager.Instance().ResultUIAlready();
    }
}
