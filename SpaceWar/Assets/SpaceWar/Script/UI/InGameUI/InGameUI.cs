using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{

    #region InGameUI_INFO
    
    public Image m_playerHPBar = null;
    public Image m_playerOXYBar = null;


    public interface PlayerHPOxyUpdate
    {
        void ChangeHP(float curHp , float prevHp,float maxHp);
        void ChangeOxy(float curOxy , float prevOxy,float maxOxy);   
    }

    float m_mainHPBarWidth = 0.0f;
    float m_mainHPBarHeight = 0.0f;
    private List<PlayerHPOxyUpdate> m_uiUpdate = new List<PlayerHPOxyUpdate>();
    #endregion

    #region UnityMethod
    void Start()
    {
        m_mainHPBarWidth = m_playerHPBar.GetComponent<RectTransform>().sizeDelta.x;

        m_mainHPBarHeight = m_playerHPBar.GetComponent<RectTransform>().sizeDelta.y;
       
    }
    #endregion

    #region PlayerHP_OXY
    public void AddUIUpdate(PlayerHPOxyUpdate inter)
    {
        if (!m_uiUpdate.Contains(inter))
            m_uiUpdate.Add(inter);
    }

    public void PlayerHPUpdate(float curHp,float prevHP, float maxHp)
    {
        m_playerHPBar.GetComponent<RectTransform>().sizeDelta = 
            new Vector2(m_mainHPBarWidth * (curHp / maxHp) , m_mainHPBarHeight);

        Debug.Log("hp " + m_playerHPBar.GetComponent<RectTransform>().sizeDelta);
        foreach (var inter in m_uiUpdate)
            inter.ChangeHP(curHp , prevHP,maxHp);
    }

    public void PlayerOxyUpdate(float curOxy,float prevOxy, float maxOxy)
    {
        m_playerOXYBar.GetComponent<RectTransform>().sizeDelta = new Vector2(m_mainHPBarWidth * (curOxy / maxOxy) , m_mainHPBarHeight);
        foreach (var inter in m_uiUpdate)
            inter.ChangeOxy(curOxy , prevOxy,maxOxy);
    }
    #endregion
}
