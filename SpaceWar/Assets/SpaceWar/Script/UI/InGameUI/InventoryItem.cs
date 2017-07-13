using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem : MonoBehaviour {

    #region InventoryItem_INFO
    public enum ITEM_TYPE
    {
        ITEM_WEAPON,
        ITEM_UTILITY,
        ITEM_OTHER
    }
    public ITEM_TYPE m_itemType = ITEM_TYPE.ITEM_OTHER;
    public UISprite m_itemIcon = null;
    public UILabel m_itemTitle = null;
    public UILabel m_equipLabel = null;
    public string m_itemID = null;

    #endregion

    #region InventoryMethod
    // 기타 세팅
    public void SetItem(string itemID)
    {
        // 아이템 아이디를 통해 세팅 후 이미지 및 변경 처리 
        m_itemID = itemID; 
    }



    // 장비
    public void EquipButton()
    {

    }
    #endregion
}
