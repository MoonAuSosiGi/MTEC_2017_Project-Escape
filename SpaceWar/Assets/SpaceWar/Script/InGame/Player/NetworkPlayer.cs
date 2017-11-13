using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nettention.Proud;

public class NetworkPlayer : MonoBehaviour {

    #region NetworkPlayer_INFO

    private PlayerController m_playerController = null;
    // -- Network Player -----------------------------------------------------------//
    public Animator PlayerAnim;
    public HostID m_hostID;
    public WeaponItem m_weapon = null;
    public Item[] m_weapons = new Item[4];
    public HealPackItem m_curHealPack = null;
    public GameObject m_weaponAnchor;
    public string m_userName = null;
    private int m_index = -1;
    [SerializeField] private TextMesh m_userNameUI = null;

    private bool m_isDeath = false;
    public bool IS_DEATH { get { return m_isDeath; }
        set {
            m_isDeath = value;
            ShowPlayerName(m_isDeath) ;
        }
    }

    public void HPUpdate(float cur,float max)
    {
        if (iTween.Count(gameObject) > 0)
            iTween.Stop(gameObject);
        m_hpUI.gameObject.SetActive(true);

        m_hpUI.transform.position =  GameManager.Instance().m_inGameUI.GetUIPos(
            GetComponent<PlayerController>().HEAD_ANCHOR.transform.position);

        iTween.ValueTo(gameObject , iTween.Hash(
            "from" , m_hpUI.value ,
            "to" , cur / max ,
            "onupdatetarget" , gameObject ,
            "onupdate" , "HpUpdate",
            "time",0.2f,
            "oncompletetarget",gameObject,
            "oncomplete","HpHide"));
    }

    void HpUpdate(float value)
    {
        m_hpUI.transform.position = GameManager.Instance().m_inGameUI.GetUIPos(
            GetComponent<PlayerController>().HEAD_ANCHOR.transform.position);
        m_hpUI.value = value;
    }

    void HpHide()
    {
        m_hpUI.gameObject.SetActive(false);
    }

    public WeaponItem HAS_WEAPON
    {
        get { return m_weapon; }
        set {
            m_weapon = value;

            if(m_weapon == null)
            {
                // 수류탄이었으므로 애니메이션 변경
                PlayerController p = this.GetComponent<PlayerController>();
                PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_BAREHAND);
            }
        }
    }

    [SerializeField] private UISlider m_hpUI = null;



    #region Network Player Proud Net
    PositionFollower m_positionFollower = new PositionFollower();
    AngleFollower m_playerCamSeeX = new AngleFollower();
    AngleFollower m_playerCamSeeY = new AngleFollower();
    AngleFollower m_playerCamSeeZ = new AngleFollower();

    AngleFollower m_playerSeeX = new AngleFollower();
    AngleFollower m_playerSeeY = new AngleFollower();
    AngleFollower m_playerSeeZ = new AngleFollower();

    public HostID HOST_ID { get { return m_hostID; } }
    #endregion
    #endregion

    void Start()
    {
        m_playerController = this.GetComponent<PlayerController>();
    }

    public void ChangeRaderMode()
    {
        m_playerController.RENDERER.material = WeaponManager.Instance().ITEM_OUTLINE_MAT;
    }

    public void ChangeOriginalMode()
    {
        m_playerController.RENDERER.material = m_playerController.ORIGIN_MATERIAL;
    }


    public void NetworkPlayerSetup(HostID hostID,string userName)
    {
        m_hostID = hostID;
        m_userNameUI = transform.GetComponent<PlayerController>().USERNAME_UI;
        m_userNameUI.text = userName;
        m_userName = userName;
        ShowPlayerName(false);

        //ui
        m_hpUI = GameManager.Instance().m_inGameUI.SetupNetworkHPUI().GetComponent<UISlider>();
        m_hpUI.gameObject.SetActive(false);
        GameObject.Destroy(this.GetComponent<AudioListener>());
    }

    //임시 개인전
    public void NetworkPlayerColorSetup(int index)
    {
        Vector4[] vecs = { new Vector4(3.68276f , 0.0f , 6.0f) ,
            new Vector4(0.0f , 6.0f , 0.7862077f) , new Vector4(0.0f , 3.517242f , 6.0f) };
        transform.GetChild(0).GetChild(0).GetChild(3)
                .GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_EmissionColor" , vecs[index]);
    }

    public void RecvNetworkMove(UnityEngine.Vector3 pos,
        UnityEngine.Vector3 velocity, UnityEngine.Vector3 charrot, 
        UnityEngine.Vector3 rot)
    {
        var npos = new Nettention.Proud.Vector3();
        npos.x = pos.x;
        npos.y = pos.y;
        npos.z = pos.z;

        var nvel = new Nettention.Proud.Vector3();
        nvel.x = velocity.x;
        nvel.y = velocity.y;
        nvel.z = velocity.z;

        m_positionFollower.SetTarget(npos , nvel);

        m_playerCamSeeX.TargetAngle = charrot.x * Mathf.Deg2Rad;
        m_playerCamSeeY.TargetAngle = charrot.y * Mathf.Deg2Rad;
        m_playerCamSeeZ.TargetAngle = charrot.z * Mathf.Deg2Rad;

        m_playerSeeX.TargetAngle = rot.x * Mathf.Deg2Rad;
        m_playerSeeY.TargetAngle = rot.y * Mathf.Deg2Rad;
        m_playerSeeZ.TargetAngle = rot.z * Mathf.Deg2Rad;

    }

    public void RecvNetworkAnimation(string animationName,int aniValue)
    {
        PlayerController p = this.GetComponent<PlayerController>();
        //test code
        if (animationName.Equals("CW"))
        {
            if(m_weapon != null)
            {
                m_weapon.gameObject.SetActive(true);
                switch (m_weapon.ITEM_TYPE)
                {
                    case Item.ItemType.GUN: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_GUN01); break;
                    case Item.ItemType.RIFLE: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_GUN02); break;
                    case Item.ItemType.MELEE: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_MELEE); break;
                    case Item.ItemType.ROCKETLAUNCHER: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_ROCKETLAUNCHER); break;
                    case Item.ItemType.ETC_GRENADE: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_ETC); break;
                    case Item.ItemType.ETC_RECOVERY: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_ETC); break;
                }
            }
            else if(m_curHealPack != null)
            {
                PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_ETC); 
            }
            else
            {
                PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_BAREHAND);
            }

        }
        else if (aniValue == 1234)
        {
            if(animationName.Equals("Damage"))
            {
                GetComponent<PlayerController>().DamageEffect(false,false);
            }
            Debug.Log("DD " + animationName);
            PlayerAnim.Play(animationName);
        }
        else
        {
            if (animationName.Equals("INTERACTION") && aniValue != 0)
            {
                if (m_weapon != null && m_weapon.ITEM_TYPE != Item.ItemType.ETC_GRENADE && m_weapon.ITEM_TYPE != Item.ItemType.ETC_RECOVERY)
                    m_weapon.gameObject.SetActive(false);

                PlayerAnim.runtimeAnimatorController = this.GetComponent<PlayerController>().GetCurrentAnimator(PlayerController.AnimationType.ANI_ETC);
            }
            else if (m_weapon != null && m_weapon.gameObject.activeSelf == false)
                m_weapon.gameObject.SetActive(true);

            PlayerAnim.SetInteger(animationName , aniValue);

            if(animationName.Equals("WALK") && aniValue == 3)
            {
                this.GetComponent<PlayerController>().DASH_EFFECT.SetActive(true);
            }
            else
            {
                this.GetComponent<PlayerController>().DASH_EFFECT.SetActive(false);
            }

            if (m_weapon == null)
                return;
            
            if(animationName.Equals("ATTACK") && aniValue == 1)
              m_weapon.SoundPlay();

            // 근거리 무기용
            if(animationName.Equals("ATTACK") && aniValue == 1 && m_weapon.ITEM_TYPE == Item.ItemType.MELEE)
            {
                m_weapon.PLAYER = transform;
                m_weapon.AttackSword();
            }
            else if (animationName.Equals("ATTACK") && aniValue == 0 && m_weapon.ITEM_TYPE == Item.ItemType.MELEE)
            {
                m_weapon.AttackSwordEnd();
            }
            // 원거리
            else if (animationName.Equals("ATTACK") && aniValue == 0 && 
                ( m_weapon.ITEM_TYPE == Item.ItemType.GUN  || m_weapon.ITEM_TYPE == Item.ItemType.RIFLE ||
                m_weapon.ITEM_TYPE == Item.ItemType.ROCKETLAUNCHER))
            {
                //m_weapon.SHOT_EFFECT.transform.position = m_weapon.FIRE_POS.position;
                //m_weapon.SHOT_EFFECT.SetActive(true);
                //Invoke("ShotEffectEnd" , m_weapon.COOL_TIME - 0.1f);
            }
        }
    }

    void ShotEffectEnd()
    {
        if(m_weapon != null)
            m_weapon.SHOT_EFFECT.SetActive(false);
    }

    void NetworkMoveUpdate()
    {
        m_positionFollower.FrameMove(Time.deltaTime);
        m_playerCamSeeX.FrameMove(Time.deltaTime);
        m_playerCamSeeY.FrameMove(Time.deltaTime);
        m_playerCamSeeZ.FrameMove(Time.deltaTime);
        m_playerSeeX.FrameMove(Time.deltaTime);
        m_playerSeeY.FrameMove(Time.deltaTime);
        m_playerSeeZ.FrameMove(Time.deltaTime);

        m_playerCamSeeX.FollowerAngleVelocity = 200 * Time.deltaTime;
        m_playerCamSeeY.FollowerAngleVelocity = 200 * Time.deltaTime;
        m_playerCamSeeZ.FollowerAngleVelocity = 200 * Time.deltaTime;
        m_playerSeeX.FollowerAngleVelocity = 200 * Time.deltaTime;
        m_playerSeeY.FollowerAngleVelocity = 200 * Time.deltaTime;
        m_playerSeeZ.FollowerAngleVelocity = 200 * Time.deltaTime;


        var p = new Nettention.Proud.Vector3();
        var vel = new Nettention.Proud.Vector3();

        m_positionFollower.GetFollower(ref p , ref vel);
     
        transform.position = new UnityEngine.Vector3(
            (float)p.x , (float)p.y , (float)p.z);

        float fx = (float)m_playerCamSeeX.FollowerAngle * Mathf.Rad2Deg;
        float fy = (float)m_playerCamSeeY.FollowerAngle * Mathf.Rad2Deg;
        float fz = (float)m_playerCamSeeZ.FollowerAngle * Mathf.Rad2Deg;

        var rotate = Quaternion.Euler(fx , fy , fz);
        transform.localRotation = rotate;

        fx = (float)m_playerSeeX.FollowerAngle * Mathf.Rad2Deg;
        fy = (float)m_playerSeeY.FollowerAngle * Mathf.Rad2Deg;
        fz = (float)m_playerSeeZ.FollowerAngle * Mathf.Rad2Deg;

        rotate = Quaternion.Euler(fx , fy , fz);
        transform.GetChild(0).localRotation = rotate;

    }

    void FixedUpdate()
    {
        NetworkMoveUpdate();

        if(m_userNameUI.gameObject.activeSelf)
        {
            RotatePlayerName();
        }
      
    }

    // 이름 처리

    void RotatePlayerName()
    {
        m_userNameUI.transform.rotation = GameManager.Instance().PLAYER.m_player.transform.rotation;
    }

    void ShowPlayerName(bool isShow)
    {
        m_userNameUI.gameObject.SetActive(isShow);
    }


    public void EquipWeapon(GameObject weapon)
    {
        PlayerController p = this.GetComponent<PlayerController>();
        if (weapon != null)
        {
            int newIndex = p.GetEquipItemIndex(weapon.GetComponent<Item>().ITEM_TYPE);
            if (m_index != -1 && m_weapons[m_index] != null)
            {
                m_weapons[m_index].gameObject.SetActive(false);
               
                if ( m_weapons[newIndex] != null && m_weapons[newIndex].ITEM_ID != weapon.GetComponent<Item>().ITEM_ID)
                {
                    m_weapons[newIndex].gameObject.SetActive(false);
                }
               
            }

            m_weapons[newIndex] = weapon.GetComponent<Item>();
            m_index = newIndex;

            if (m_weapons[m_index].GetComponent<HealPackItem>() != null)
            {
                // 힐팩은 여기서 처리
                m_curHealPack = weapon.GetComponent<HealPackItem>();
                m_curHealPack.transform.parent = m_weaponAnchor.transform;
                m_curHealPack.transform.localPosition = m_curHealPack.LOCAL_SET_POS;
                m_curHealPack.transform.localRotation = Quaternion.Euler(m_weapon.LOCAL_SET_ROT);
                m_curHealPack.transform.localScale = m_curHealPack.LOCAL_SET_SCALE;
                m_curHealPack.GetComponent<SphereCollider>().enabled = false;
                Debug.Log("m_curHealPack " + m_curHealPack.ITEM_NETWORK_ID + " type " + m_curHealPack.ITEM_TYPE + " name " + m_curHealPack.name);
                PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_ETC);
                return;
            }
            m_weapon = m_weapons[m_index].GetComponent<WeaponItem>() ;
            Debug.Log("Weapon " + m_weapon.ITEM_NETWORK_ID + " type " + m_weapon.ITEM_TYPE + " name " + m_weapon.name);
            m_weapon.transform.parent = m_weaponAnchor.transform;
            m_weapon.transform.localPosition = m_weapon.LOCAL_SET_POS;
            m_weapon.transform.localRotation = Quaternion.Euler(m_weapon.LOCAL_SET_ROT);
            m_weapon.transform.localScale = m_weapon.LOCAL_SET_SCALE;
            m_weapon.GetComponent<SphereCollider>().enabled = false;

            
            switch (m_weapon.ITEM_TYPE)
            {
                case Item.ItemType.GUN:   PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_GUN01); break;
                case Item.ItemType.RIFLE: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_GUN02); break;
                case Item.ItemType.MELEE: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_MELEE); break;
                case Item.ItemType.ROCKETLAUNCHER: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_ROCKETLAUNCHER); break;
                case Item.ItemType.ETC_GRENADE: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_ETC); break;
                case Item.ItemType.ETC_RECOVERY: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_ETC); break;
            }

            if(m_weapon.ITEM_TYPE == Item.ItemType.ETC_GRENADE)
            {
                Grenade g = (m_weapon as Grenade);
                g.IS_NETWORK = true;
             //   g.NetworkGrenadeEnable();
            }
        }
    }

    public void UnEquipWeapon(UnityEngine.Vector3 pos, UnityEngine.Vector3 rot)
    {
        Debug.Log("UnEquip 상대방이 버렸는데 " + (m_curHealPack == null));

        PlayerController p = this.GetComponent<PlayerController>();
        int newIndex = p.GetEquipItemIndex(m_weapons[m_index].ITEM_TYPE);

        m_weapons[newIndex] = null;

        if (m_weapon != null)
        {
            // 버리는 로직
            m_weapon.transform.parent = null;
            m_weapon.transform.position = pos;
            m_weapon.transform.eulerAngles = rot;
            m_weapon.GetComponent<SphereCollider>().enabled = true;
            m_weapon.SHOT_EFFECT.SetActive(false);

            m_weapon = null;
           
            PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_BAREHAND);
        }
        else if(m_curHealPack != null)
        {
            m_curHealPack.transform.parent = null;
            m_curHealPack.transform.position = pos;
            m_curHealPack.transform.eulerAngles = rot;
            m_curHealPack.GetComponent<SphereCollider>().enabled = true;

            m_curHealPack = null;


            PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_BAREHAND);
        }
    }
}
