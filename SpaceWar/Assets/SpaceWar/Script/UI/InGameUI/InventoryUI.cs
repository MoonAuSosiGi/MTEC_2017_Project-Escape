using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour {

    #region InventoryUI_INFO
    public Material m_panelBlur = null;
    public GameObject m_weaponGrid = null;
    public GameObject m_utilityGrid = null;
    public GameObject m_itemGrid = null;

    private bool m_InvenOpenAble = true;
    private bool m_InvenOpenState = false;

    public bool INVEN_OPENABLE
    {
        get { return m_InvenOpenAble; }
        set { m_InvenOpenAble = value; }
    }

    public bool INVEN_OPENSTATE
    {
        get { return m_InvenOpenState; }
    }
    #endregion

    #region InventoryUI_Method

   

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ExitButton();
    }

    public void OpenInventory()
    {
        if(!m_InvenOpenAble)
        {
            Debug.Log("Inventory OpenAble = false");
            return;
        }
        m_InvenOpenState = true;
        gameObject.SetActive(true);
        // 블러사이즈 여기서 수정
        //m_panelBlur.SetFloat()
    }

    public void CloseInventory()
    {
        m_InvenOpenState = false;
    }
    public void ExitButton()
    {
        CloseInventory();
    }
    #endregion
}
