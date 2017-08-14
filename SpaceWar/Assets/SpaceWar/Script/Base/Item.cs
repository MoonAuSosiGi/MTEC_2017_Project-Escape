using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    #region ITEM_INFO
    // -1 일 경우 미등록 아이템
    [SerializeField] protected int m_itemID = -1;
    protected int m_itemNetworkID = -1;


    private string m_itemName = "test";


    public string ITEM_NAME { get { return m_itemName; } set { m_itemName = value; } }
    public int ITEM_ID { get { return m_itemID; } set { m_itemID = value; } }
    public int ITEM_NETWORK_ID { get { return m_itemNetworkID; } set { m_itemNetworkID = value; } }
    #endregion

    #region ITEM Method
    
    #endregion
}
