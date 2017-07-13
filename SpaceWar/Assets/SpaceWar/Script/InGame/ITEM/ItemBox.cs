using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour {

    #region ItemBox_INFO
    private int m_itemBoxID = 0;
    private bool m_isOpened = false;
    private bool m_networkRecv = false;

    public int ITEMBOX_ID
    {
        get { return m_itemBoxID; }
        set { m_itemBoxID = value; }
    }

    public bool OPENED
    {
        get { return m_isOpened; }
        
    }
    #endregion

    #region UnityMethod
    void Start()
    {
        m_itemBoxID = NetworkManager.Instance().GetItemBoxIndex(this);
    }
    #endregion

    public void UseItemBox()
    {
        if (m_isOpened)
            return;
        // 이건 애니메이션 요청
        this.GetComponent<Animator>().SetInteger("ITEMBOX_STATE" , 1);
    }
    
    void ItemBoxAniEnd()
    {
        m_isOpened = true;
        if(!m_networkRecv)
            NetworkManager.Instance().C2SRequestPlayerUseItemBox(this);
    }

    //단순히 열기만 함
    public void ItemBoxNetworkOpen()
    {
        m_networkRecv = true;
        this.GetComponent<Animator>().SetInteger("ITEMBOX_STATE" , 1);
    }

    public void ItemBoxClose()
    {
    }

}
