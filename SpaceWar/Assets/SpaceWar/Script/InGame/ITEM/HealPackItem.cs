using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPackItem : Item {

    #region Heal Pack Item INFO
    private float m_heal = 0.0f;
    float m_healTime = 1.0f;

    public float HEAL { get { return m_heal; } set { m_heal = value; } }
    public float HEAL_TIME { get { return m_healTime; } set { m_healTime = value; } }
    float m_recoveryKitValue = 0.0f;
    
    #endregion

    #region Heal Pack Method
    public void Setup(string id)
    {
        Setup();
        this.m_itemID = id;

        WeaponTableData data = WeaponManager.Instance().GetWeaponData(id);

        m_type = ItemType.ETC_RECOVERY;

        m_healTime = data.Healusetime;
        m_heal = data.Heal;

        // 좌표 세팅 / 장작 포지션 / 장착 회전 / 장착 스케일 / 스폰 회전
        m_localSetPos = new Vector3(data.Equippos_x , data.Equippos_y , data.Equippos_z);
        m_localSetRot = new Vector3(data.Equiprot_x , data.Equiprot_y , data.Equiprot_z);
        m_localSetScale = new Vector3(data.Equipscale_x , data.Equipscale_y , data.Equipscale_z);
        m_sponeRotation = new Vector3(data.Sponerot_x , data.Sponerot_y , data.Sponerot_z);

        //무게 세팅
        ITEM_WEIGHT = data.Weight;

        // 콜라이더 삽입
        SphereCollider col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = data.Weapongetcolliderradius;


        this.tag = "Recoverykit";

    }

    #region Equip Weapon Logic
    protected override void EquipItem()
    {
        if (GameManager.Instance() != null)
            GameManager.Instance().EquipWeapon(ITEM_ID , 0 , WeaponManager.Instance().GetWeaponData(ITEM_ID).Bulletcount);
        this.GetComponent<SphereCollider>().enabled = false;
    }

    protected override void UnEquipItem()
    {
        if (GameManager.Instance() != null)
            GameManager.Instance().UnEquipWeapon(GameManager.Instance().PLAYER.m_player.CUR_EQUIP_INDEX);
        this.GetComponent<SphereCollider>().enabled = true;
    }
    #endregion


    public void Recovery(PlayerController player)
    {
        // 여기서 게이지를 넣자

        // 다되면 힐
        GameManager.Instance().SLIDER_UI.ShowSlider();
        
        m_recoveryKitValue += Time.deltaTime / m_healTime;
        GameManager.Instance().SLIDER_UI.SliderProcess(m_recoveryKitValue);

        if (m_recoveryKitValue >= 1.0f)
        {
            player.RecoveryItemUseEnd();
            
            GameManager.Instance().SLIDER_UI.HideSlider();
            GameManager.Instance().m_inGameUI.HideObjectUI();
            if (NetworkManager.Instance() != null)
            {
                NetworkManager.Instance().RequestHpUpdate(GameManager.Instance().PLAYER.m_hp + HEAL);
            }
        }

    }
    public void RecoveryUp()
    {
        m_recoveryKitValue = 0.0f;
        GameManager.Instance().SLIDER_UI.Reset();
        GameManager.Instance().SLIDER_UI.HideSlider();
        
    }
    #endregion
}
