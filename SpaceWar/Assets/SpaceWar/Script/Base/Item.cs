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

    public string ITEM_NAME { get { return m_itemName; } set { m_itemName = value; } }
    public string ITEM_ID { get { return m_itemID; } set { m_itemID = value; } }
    public float ITEM_WEIGHT { get { return m_itemWeight; } set { m_itemWeight = value; } }
    public string ITEM_NETWORK_ID { get { return m_itemNetworkID; } set { m_itemNetworkID = value; } }
    #endregion

    #region ITEM Method
    
    #endregion
}
