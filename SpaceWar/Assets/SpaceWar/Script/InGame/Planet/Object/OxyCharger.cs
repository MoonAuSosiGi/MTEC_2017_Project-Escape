using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxyCharger : MonoBehaviour {

    #region OxyCharger
    public int m_index = -1;
    public bool m_isAlive = true;
    float m_oxy = 250.0f;

    public int OXY_CHARGER_ID
    {
        get { return m_index; }
        set { m_index = value; }
    }
    public float CURRENT_OXY { get { return m_oxy; } }
    #endregion

    #region UnityMethod
    void Start()
    {
        m_index = NetworkManager.Instance().GetOxyChargerIndex(this);
        
        UIUpdate();
    }
    #endregion

    #region OxyControl
    public void UseOxy(float oxy)
    {
        if (!m_isAlive)
            return;
        if (GameManager.Instance().PLAYER.m_oxy >= 100.0f)
            return;
        float useOxy = (m_oxy < oxy) ? oxy - m_oxy : oxy;

        NetworkManager.Instance().C2SRequestPlayerUseOxyCharger(this , useOxy);
    }

    public void RecvOxy(float oxy)
    {
        m_oxy -= oxy;
        UIUpdate();
    }

    void UIUpdate()
    {
        float percent = m_oxy / 250.0f;
        
        transform.GetChild(0).GetChild(0).transform.localScale = new Vector3(1.0f , percent , 1.0f);

        if(m_oxy <= 0.0f)
        {
            m_isAlive = false;
            var sources = this.GetComponents<AudioSource>();

            for(int i = 0; i < sources.Length; i++)
            {
                if (i != 0)
                    sources[i].enabled = false;
            }

            transform.GetChild(0).GetComponent<Animator>().SetInteger("ChargerEnd" , 1);
        }
    }
    #endregion
}
