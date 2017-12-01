using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{

    #region InGameUI_INFO
    [SerializeField] private Camera m_uiCamera = null;


    #region Player HP / OXY
    public UISlider m_hpSlider = null;
    public UISlider m_oxySlider = null;
    public interface PlayerHPOxyUpdate
    {
        void ChangeHP(float curHp , float prevHp , float maxHp);
        void ChangeOxy(float curOxy , float prevOxy , float maxOxy);
    }

    private List<PlayerHPOxyUpdate> m_uiUpdate = new List<PlayerHPOxyUpdate>();
    #endregion

    #region SpaceShip UI
    public UISlider m_spaceShipEngineSlider = null;
    public GameObject m_spaceShipUI = null;

    #endregion

    #region Player Gun
    public GameObject m_equipInfo = null;
    public UILabel m_curAmmo = null;
    public UILabel m_curWeaponName = null;
    public UISprite m_curWeaponSpr = null;
    #endregion

    #region Meteor UI    
    public UILabel m_meteoTimeLabel = null;
    public GameObject m_meteorObj = null;
    #endregion

    #region Weapon Inventory Icon
    // 실제 껐다 켰다 할 오브젝트 
    [SerializeField] private GameObject m_invenIconObject = null;
    [SerializeField] private List<GameObject> m_InvenIconList = new List<GameObject>();
    [SerializeField] private GameObject m_selectObject = null;

    [SerializeField] private Transform m_showInvenTarget = null;
    [SerializeField] private Transform m_hideInvenTarget = null;

    public GameObject INVEN_OBJECT { get { return m_invenIconObject; } }
    #endregion

    #region Debug Label
    [SerializeField] private UILabel m_debugLabel = null;
    #endregion

    #region Network Player HP
    [SerializeField] private GameObject m_hpOrigin = null;

    #endregion

    #region Object UI 
    [SerializeField] private GameObject m_objecUI;
    [SerializeField] private UILabel m_objectUIShowName;
    #endregion
    #endregion

    #region PlayerHP_OXY
    public void AddUIUpdate(PlayerHPOxyUpdate inter)
    {
        if (!m_uiUpdate.Contains(inter))
            m_uiUpdate.Add(inter);
    }

    public void PlayerHPUpdate(float curHp,float prevHP, float maxHp)
    {
        //m_playerHPBar.GetComponent<RectTransform>().sizeDelta = 
        //    new Vector2(m_mainHPBarWidth * (curHp / maxHp) , m_mainHPBarHeight);

        m_hpSlider.value = (curHp / maxHp);

        foreach (var inter in m_uiUpdate)
            inter.ChangeHP(curHp , prevHP,maxHp);
    }

    public void PlayerOxyUpdate(float curOxy,float prevOxy, float maxOxy)
    {
        //m_playerOXYBar.GetComponent<RectTransform>().sizeDelta = new Vector2(m_mainHPBarWidth * (curOxy / maxOxy) , m_mainHPBarHeight);

        m_oxySlider.value = (curOxy / maxOxy);

        foreach (var inter in m_uiUpdate)
            inter.ChangeOxy(curOxy , prevOxy,maxOxy);
    }
    #endregion

    #region EquipWeapon

    public void ShowInvenUI()
    {
        if (iTween.Count(m_invenIconObject) > 0)
            return;//iTween.Stop(m_invenIconObject);
        iTween.MoveTo(m_invenIconObject , iTween.Hash("y" , m_showInvenTarget.position.y,"time",0.5f));
    }

    public void HideInvenUI()
    {
        if (iTween.Count(m_invenIconObject) > 0)
            iTween.Stop(m_invenIconObject);
        iTween.MoveTo(m_invenIconObject , iTween.Hash("y" , m_hideInvenTarget.position.y,"time",0.5f));
    }

    public void EquipWeapon(string itemID,int index,int curCount,int maxCount)
    {
        //ShowDebugLabel("무기 장착 " + index + " 아이템 아이디 " + itemID);
        m_selectObject.transform.position = m_InvenIconList[index].transform.position;

        if (string.IsNullOrEmpty(itemID))
        {
            return;
        }
        // 인벤 전부 출력
        INVEN_OBJECT.SetActive(true);
        // 우측 하단 정보 출력 
        m_equipInfo.SetActive(true);

        // 아이템 정보 얻기
        WeaponTableData data = WeaponManager.Instance().GetWeaponData(itemID);
        Debug.Log("type " + data.Type + " " + (int)Item.ItemType.ROCKETLAUNCHER);

        if(data.Type == (int)Item.ItemType.GUN || data.Type == (int)Item.ItemType.RIFLE || data.Type == (int) Item.ItemType.ROCKETLAUNCHER)
        {
            // 장탄수 적용
            UpdateWeapon(curCount , maxCount);

        }
        else
        {
            m_curAmmo.text = "";
        }


        //아이템 이름 세팅
        m_curWeaponName.text = data.Name_kr;
        GetWeaponName(m_InvenIconList[index]).text = data.Name_kr;

        // 아이템 아이콘 세팅
        m_curWeaponSpr.spriteName = data.Inventoryicon;
        GetWeaponIcon(m_InvenIconList[index]).spriteName = data.Inventoryicon;
        GetWeaponIcon(m_InvenIconList[index]).gameObject.SetActive(true);

        
    }

    public void UpdateWeapon(int curCount,int maxCount)
    {
        m_curAmmo.text = curCount.ToString();
    }

    public void UnEquipWeapon(int index,bool iconHide =false)
    {
        //아이템 이름 세팅
        if(iconHide)
        {
            m_curWeaponName.text = "";
            GetWeaponName(m_InvenIconList[index]).text = "";
            GetWeaponIcon(m_InvenIconList[index]).gameObject.SetActive(false);
        }
        
        m_equipInfo.SetActive(false);
    }

    public void ThrowWeapon(int index)
    { 
        GetWeaponIcon(m_InvenIconList[index]).gameObject.SetActive(false);
        GetWeaponName(m_InvenIconList[index]).text = "";
    }

    // 인벤토리 관련 
    UISprite GetWeaponIcon(GameObject target)
    {
        return target.transform.GetChild(1).GetComponent<UISprite>();
    }

    UILabel GetWeaponName(GameObject target)
    {
        return target.transform.GetChild(2).GetComponent<UILabel>();
    }
    #endregion
    
    #region SpaceShipUI

    public void StartSpaceShipUI()
    {
        m_spaceShipUI.SetActive(true);

        m_spaceShipEngineSlider.value = 0.0f;
    }

    public void StopSpaceShipUI()
    {
        m_spaceShipUI.SetActive(false);
    }

    public void ProcessSpaceShipEngine(float t)
    {
        m_spaceShipEngineSlider.value = t;
    }
    #endregion

    #region MeteorUI
    public void StartMeteor()
    {
        m_meteoTimeLabel.text = "";
        m_meteorObj.SetActive(true);
    }

    public void StopMeteor()
    {
        m_meteoTimeLabel.text = "";
        m_meteorObj.SetActive(false);
    }

    public void RecvMeteorInfo(int time)
    {
        m_meteoTimeLabel.text = time.ToString();
    }
    #endregion

    #region Debug Label
    public void ShowDebugLabel(string text)
    {
        m_debugLabel.text = "Debug Label \n"+ text;
    }
    #endregion
    
    #region Network Player HP Method

    public GameObject SetupNetworkHPUI()
    {
        GameObject hpUI = GameObject.Instantiate(m_hpOrigin);
        hpUI.transform.parent = m_hpOrigin.transform.parent;
        hpUI.transform.localScale = m_hpOrigin.transform.localScale;
        return hpUI;
    }
    public Vector3 GetUIPos(Vector3 pos)
    {
        Vector3 p = Camera.main.WorldToScreenPoint(pos);
        p = m_uiCamera.ScreenToWorldPoint(p);
        return new Vector3(p.x , p.y + 0.25f , 0.0f);
    }
    #endregion

    #region Object UI Method
    public void ShowObjectUI(string msg)
    {
        m_objectUIShowName.text = msg;
        m_objecUI.SetActive(true);
    }

    public void HideObjectUI()
    {
        m_objecUI.SetActive(false);
    }
    #endregion
}
