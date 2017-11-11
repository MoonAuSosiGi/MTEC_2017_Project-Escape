using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    #region ITEM_INFO
    // -1 일 경우 미등록 아이템
    [SerializeField] protected string m_itemID = "";
    protected string m_itemNetworkID = null;
    private float m_itemWeight = 0.0f;
    private string m_itemName = "test";

    #region  InGame INFO
    [SerializeField]
    protected Vector3 m_localSetPos = Vector3.zero;
    [SerializeField]
    protected Vector3 m_localSetRot = Vector3.zero;
    [SerializeField]
    protected Vector3 m_localSetScale = Vector3.zero;
    [SerializeField]
    protected Vector3 m_sponeRotation = Vector3.zero;

    public Vector3 LOCAL_SET_POS { get { return m_localSetPos; } set { m_localSetPos = value; } }
    public Vector3 LOCAL_SET_ROT { get { return m_localSetRot; } set { m_localSetRot = value; } }
    public Vector3 LOCAL_SET_SCALE { get { return m_localSetScale; } set { m_localSetScale = value; } }
    public Vector3 SPONE_ROTATITON { get { return m_sponeRotation; } set { m_sponeRotation = value; } }
    #endregion
    public enum ItemType
    {
        NONE = -1,
        GUN = 0,
        RIFLE,
        ROCKETLAUNCHER,
        MELEE,
        ETC_GRENADE,
        ETC_RECOVERY // 이친구 임시다 웨폰 아이템 아니다 
    }

    [SerializeField] protected ItemType m_type = ItemType.NONE;
    public ItemType ITEM_TYPE { get { return m_type; } set { m_type = value; } }
    public string ITEM_NAME { get { return m_itemName; } set { m_itemName = value; } }
    public string ITEM_ID { get { return m_itemID; } set { m_itemID = value; } }
    public float ITEM_WEIGHT { get { return m_itemWeight; } set { m_itemWeight = value; } }
    public string ITEM_NETWORK_ID { get { return m_itemNetworkID; } set { m_itemNetworkID = value; } }

    // 메시 렌더러 얻기
    private MeshRenderer m_renderer = null;
    // 메테리얼 얻기
    private Material m_originMat = null;


    // 어떤 호스트가 들고있는지 
    private int m_targetHostID = -1;
    public int TARGET_HOST_ID { get { return m_targetHostID; } set { m_targetHostID = value; } }

    private bool m_equipState = false;
    public bool EQUIP_STATE { get { return m_equipState; } set { m_equipState = value; if (m_equipState) EquipItem(); else UnEquipItem(); } }

    protected virtual void EquipItem() { }
    protected virtual void UnEquipItem() { }
    #endregion

    protected void Setup()
    {
        m_renderer = this.GetComponentInChildren<MeshRenderer>();
        m_originMat = m_renderer.material;
    }

    public void OutLineShow()
    {
        if (m_renderer == null || m_renderer.material == null)
            Debug.Log("name " + transform.name);
        Debug.Log("null ? " + (WeaponManager.Instance().ITEM_OUTLINE_MAT == null)+ " +  " + (m_renderer == null));
        m_renderer.material = WeaponManager.Instance().ITEM_OUTLINE_MAT;
    }

    public void OutLineHide()
    {
        Debug.Log("OutLine Hide " + (m_originMat == null) + " r " + (m_renderer == null));
        m_renderer.material = m_originMat;
    }
}
