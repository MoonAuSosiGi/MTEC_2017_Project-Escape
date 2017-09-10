using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singletone<GameManager> {

    #region GameManager_INFO

    #region Table
    [SerializeField] GameTable m_gameTable = null;

    #region GameTable Tag
    public const string FULL_HP = "FullHP";
    public const string FULL_OXY = "FullOXY";
    public const string OXY_DAMAGE = "OxyDamage";
    public const string WALK_SPEED = "WalkSpeed";
    public const string RUN_SPEED = "RunSpeed";
    public const string JUMP_POWER = "JumpPower";
    public const string JUMP_TICK = "JumpTick";
    public const string USEOXY_IDLE = "UseOxy_IDLE";
    public const string USEOXY_RUN = "UseOxy_RUN";
    public const string USEOXY_WALK = "UseOxy_WALK";
    public const string OXY_CHARGER_FULL = "OxyCharger_FullOxy";
    public const string OXY_CHARGER_USE = "OxyCharger_UseOxy";
    public const string METEOR_HIT_DAMAGE = "Meteor_HitDamage";
    public const string METEOR_DAMAGE = "Meteor_Damage";
    public const string DAMAGE_COOLTIME = "Damage_CoolTime";
    public const string DZ_SPEED1 = "DeathZoneSpeed_Level1";
    public const string DZ_SPEED2 = "DeathZoneSpeed_Level2";
    public const string DZ_SPEED3 = "DeathZoneSpeed_Level3";
    public const string DZ_SPEED4 = "DeathZoneSpeed_Level4";
    public const string DZ_SPEED5 = "DeathZoneSpeed_Level5";
    public const string DZ_DAMAGE = "DeathZoneDamage";
    #endregion
    #endregion

    #region Game Manager Main
    // -- 게임 매니저 전반적인 것들을 관리합니다 ------------------------------------------//
    public GameObject PlayerPref;
    public GameObject MainCam;
    public GameObject Plant;
    public InGameUI m_inGameUI;
    public GameObject m_map = null;
    public GameObject m_itemParent = null;
    public GameObject m_meteorParent = null;
    public Transform PlanetAnchor;


    public GameObject[] Item;
    public Transform ItemCreaterAnchor;
    public int CreateItemNum;

    public List<GameObject> CreateWeaponList = new List<GameObject>();

    public RaycastHit SponeHitInfo;

    float deltaTime = 0.0f;

    public GameObject m_playersCreateParent = null;

    private PlayerInfo m_playerInfo = new PlayerInfo();

    public GameObject m_meteorPrefab = null;

    public PlayerInfo PLAYER
    {
        get { return m_playerInfo; }
        set { m_playerInfo = value; }
    }

    #region Network Item 
    [SerializeField] private Transform m_networkItemParent = null;
    [SerializeField] private List<GameObject> m_itemList = new List<GameObject>();
    #endregion
    #endregion

    #region NetworkObject
    public GameObject m_spaceShipParent = null;
    public GameObject m_shelterParent = null;
    public GameObject m_oxyChargerParent = null;
    public GameObject m_itemBoxParent = null;
    #endregion

    // 메테오 떨어지는 시간
    private int m_meteorTime = 30;

    #region Slider UI Menu -------------------------------------------------------------------
    [SerializeField] private SliderMenuUI m_sliderUI = null;
    public SliderMenuUI SLIDER_UI { get { return m_sliderUI; } }

    #endregion

    // result ------------------------------------------------------------------------------//
    #region RESULT 
    public ResultUI m_resultUI;
    private bool m_winner = false;
    public bool WINNER { get { return m_winner; } set { m_winner = value; } }

    public void ResultUIAlready()
    {
        m_resultUI.RESULT_UI_ALREADY = true;
    }


    #endregion
    // -------------------------------------------------------------------------------------//
    // 
    public InventoryUI m_InventoryUI = null;

    #endregion
    #region PlayerINFO
    public class PlayerInfo
    {
        public string m_name = "";
        public float m_hp = 100.0f;
        public float m_oxy = 100.0f;
        public float m_fullHp = 100.0f;
        public float m_fullOxy = 100.0f;
        public PlayerController m_player = null;
    }
    #endregion

    #region UnityMethod
    private void Awake()
    {
        // DontDestroyOnLoad(this.gameObject);

        if (GravityManager.Instance() == null || NetworkManager.Instance() == null)
            return;

        m_playerInfo.m_name = NetworkManager.Instance().USER_NAME;
        m_playerInfo.m_hp = GetGameTableValue(FULL_HP);
        m_playerInfo.m_oxy = GetGameTableValue(FULL_OXY);
        m_playerInfo.m_fullHp = GetGameTableValue(FULL_HP);
        m_playerInfo.m_fullOxy = GetGameTableValue(FULL_OXY);

        float planetScale = GravityManager.Instance().CurrentPlanet.transform.localScale.x + 20.8f;

        OnJoinedRoom(m_playerInfo.m_name , true , new Vector3(9.123454f , 48.63797f , -32.4867f));
          //  GetPlanetPosition(planetScale , Random.Range(-360.0f , 360.0f) , Random.Range(-360.0f , 360.0f)));
    }

    public float PLANET_XANGLE = 0.0f;
    public float PLANET_ZANGLE = 0.0f;

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    if (m_InventoryUI.INVEN_OPENSTATE)
        //        m_InventoryUI.CloseInventory();
        //    else
        //        m_InventoryUI.OpenInventory();
        //}
    //    float planetScale = Plant.transform.localScale.x + 12.8f;
    //    Debug.DrawLine(transform.position , GetPlanetPosition(planetScale , PLANET_XANGLE , PLANET_ZANGLE) , Color.red);
      
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0 , 0 , w , h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(1.0f , 1.0f , 1.0f , 1.0f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)" , msec , fps);
        GUI.Label(rect , text , style);
    }

    #endregion

    public GameObject CommandItemCreate(string itemID,string networkID,Vector3 pos,Vector3 rot)
    {
        Debug.Log("CommandItemCreate.. " + itemID + " pos " + rot);
        GameObject weapon = WeaponManager.Instance().CreateWeapon(itemID);
        weapon.GetComponent<Item>().ITEM_NETWORK_ID = networkID;
        weapon.transform.position = pos;
        weapon.transform.localEulerAngles = rot;
        return weapon;
    }

    IEnumerator CreateItem(int Num)
    {
        float SponHeight = 0.15f;

       // System.Array.Resize(ref CreateWeaponList , Num);

        for (int i = 0; i < Num; i++)
        {
            
            ItemCreaterAnchor.rotation = Quaternion.Euler(Random.Range(-360f , 360f) , Random.Range(-360f , 360f) , Random.Range(-360f , 360f));
            

            ItemCreaterAnchor.GetChild(0).LookAt(ItemCreaterAnchor);

            Physics.Raycast(ItemCreaterAnchor.GetChild(0).position , ItemCreaterAnchor.rotation * Vector3.forward , out SponeHitInfo, 30.0f);



            while (SponeHitInfo.collider.gameObject.CompareTag("NonSpone") 
                || SponeHitInfo.collider.gameObject.CompareTag("PlayerCharacter"))
            {
                ItemCreaterAnchor.rotation = Quaternion.Euler(Random.Range(-360f , 360f) ,
                    Random.Range(-360f , 360f) , Random.Range(-360f , 360f));
                
                ItemCreaterAnchor.GetChild(0).LookAt(ItemCreaterAnchor);

                Physics.Raycast(ItemCreaterAnchor.GetChild(0).position , 
                    ItemCreaterAnchor.rotation * Vector3.forward , out SponeHitInfo, 30.0f);

            }

            // 웨폰 생성 리스트의 관리는 ?
            string id = WeaponManager.Instance().GetRandomWeaponID();
            GameObject weapon = WeaponManager.Instance().CreateWeapon(id);
            weapon.transform.position = SponeHitInfo.point; //new Vector3(SponeHitInfo.normal.x + 45.0f , SponeHitInfo.normal.y + 45.0f , SponeHitInfo.normal.z + 90.0f); //ItemCreaterAnchor.GetChild(0).position;
            Item item = weapon.GetComponent<Item>();
            item.ITEM_NETWORK_ID = NetworkManager.Instance().m_hostID + "_" + i + "_" + id;
            NetworkManager.Instance().ITEM_DICT.Add(
                item.ITEM_NETWORK_ID,item);
            weapon.layer = 2;

            // CreateWeaponList[i].layer = 2;
            
            Vector3 SponeRot = (weapon.transform.position - GravityManager.Instance().CurrentPlanet.transform.position).normalized;

           // SponeRot += item.SPONE_ROTATITON;

            Quaternion targetRotation = Quaternion.FromToRotation(weapon.transform.up , SponeRot) * weapon.transform.rotation;
            
            weapon.transform.rotation = targetRotation;
            weapon.transform.Rotate(weapon.GetComponent<Item>().SPONE_ROTATITON);
            weapon.transform.Translate(Vector3.right * SponHeight);
            NetworkManager.Instance().C2SRequestItemCreate(
                item.ITEM_ID , item.ITEM_NETWORK_ID ,
                item.transform.position , item.transform.localEulerAngles);
            yield return new WaitForSeconds(0.001f);

        }
    }

    // 메테오 생성
    public void CreateMeteor(float anglex,float anglez)
    {
        float planetScale = Plant.transform.localScale.x + 12.8f;

        Vector3 pos = GetPlanetPosition(planetScale , anglex , anglez);
        Vector3 pos2 = GetPlanetPosition(planetScale + 30.0f , anglex , anglez);

        
        GameObject obj = Instantiate(m_meteorPrefab , new Vector3(pos.x,pos.y,pos.z),Quaternion.Euler(0.0f,0.0f,0.0f));
        
        obj.transform.rotation = Quaternion.LookRotation((pos - Vector3.zero).normalized);
        Vector3 r = obj.transform.eulerAngles;
        obj.transform.eulerAngles =new Vector3( r.x + 90.0f,r.y,r.z);

        obj.transform.parent = m_meteorParent.transform;

 

        m_meteorTime = 30;
        m_inGameUI.RecvMeteorInfo(m_meteorTime);
        m_inGameUI.StartMeteor();
        InvokeRepeating("MeteorTimer" , 1.0f , 1.0f);

    }

    void MeteorTimer()
    {
        m_meteorTime--;
        m_inGameUI.RecvMeteorInfo(m_meteorTime);

        if(m_meteorTime < 0)
        {
            m_inGameUI.StopMeteor();
            CancelInvoke("MeteorTimer");
        }
    }

    public Vector3 GetPlanetPosition(float scale,float anglex,float anglez)
    {
        float x = scale * Mathf.Sin(anglex * Mathf.Deg2Rad) * Mathf.Cos(anglez * Mathf.Deg2Rad);
        float y = scale * Mathf.Sin(anglex * Mathf.Deg2Rad) * Mathf.Sin(anglez * Mathf.Deg2Rad);
        float z = scale * Mathf.Cos(anglex * Mathf.Deg2Rad);
        return new Vector3(x,y,z); 
    }
    
    public void RecvItem(string itemID,string networkID,GameObject box)
    {
        Vector3 boxPos = box.transform.position;
        GameObject weapon = WeaponManager.Instance().CreateWeapon(itemID);
        //CreateWeaponList.Add(Instantiate(
        //        Item[itemCID] ,SponeHitInfo.point ,
        //        Quaternion.Euler(SponeHitInfo.normal.x + 45 , SponeHitInfo.normal.y + 45 ,
        //        SponeHitInfo.normal.z + 90)));
        
        //obj.transform.parent = box.transform ;
        weapon.transform.position = boxPos;

        // TODO NetworkID 등록

        Vector3 scale = weapon.transform.localScale;
        weapon.transform.localScale = new Vector3(0.0f , 0.0f , 0.0f);
        Item item = weapon.GetComponent<Item>();
        item.ITEM_NETWORK_ID = networkID;
        //CreateWeaponList[CreateWeaponList.Count - 1].GetComponent<WeaponItem>().ITEM_NETWORK_ID = CreateWeaponList.Count - 1;

        //CreateWeaponList[CreateWeaponList.Count - 1].layer = 2;
        NetworkManager.Instance().ITEM_DICT.Add(networkID, item);

        iTween.ScaleTo(weapon , iTween.Hash(
            "x" , scale.x , "y" , scale.y , "z" , scale.z ,
            "oncompletetarget" , gameObject ,
            "easeType","easeOutCubic",
            "speed",300.0f,
            "oncomplete" , "RecvItemTweenEnd", 
            "oncompleteparams", networkID));
        

    }
    
    void RecvItemTweenEnd(string networkID)
    {
        // 딱히 하는거 없음 
        Item item = NetworkManager.Instance().ITEM_DICT[networkID];
        NetworkManager.Instance().C2SRequestItemCreate(
            item.ITEM_ID,
            networkID,
            item.transform.position,
            item.transform.localEulerAngles);
    }

    public GameObject OnJoinedRoom(string name,bool me,Vector3 startPos)
    {
        
        GameObject MP = GameObject.Instantiate(this.PlayerPref);
        MP.transform.parent = m_playersCreateParent.transform;

        MP.transform.position = new Vector3(startPos.x, startPos.y , startPos.z);
        MP.transform.rotation = Quaternion.identity;      
        
        MP.name = name;

        if (me)
        {
            NetworkManager.Instance().m_itemBoxParent = m_itemBoxParent;
            NetworkManager.Instance().m_spaceShipParent = m_spaceShipParent;
            NetworkManager.Instance().m_oxyChargerParent = m_oxyChargerParent;
            NetworkManager.Instance().m_shelterParent = m_shelterParent;

            NetworkManager.Instance().NetworkObjectSetup();

            MP.AddComponent<AudioListener>();

            if (NetworkManager.Instance().IS_HOST)
            {
                StartCoroutine(CreateItem(CreateItemNum));
                NetworkManager.Instance().NetworkShelterServerSetup();
            }
            

            GameManager.Instance().PLAYER.m_name = name;

            PlayerController player = MP.GetComponent<PlayerController>();

            //MP.AddComponent<Rigidbody>();
            MP.GetComponent<Rigidbody>().freezeRotation = true;
            MP.GetComponent<Rigidbody>().useGravity = false;


            GravityManager.Instance().SetGravityTarget(MP.GetComponent<Rigidbody>());

            player.enabled = true;
            player.SetCameraThirdPosition();



            //Plant.GetComponent<Gravity>().TargetObject = MP.GetComponent<Rigidbody>();

            //AnchorPlanet.PlayerCharacter = MP.transform;
            GameManager.Instance().PLAYER.m_player = player;

            //NetworkManager.Instance().C2SRequestClientJoin(
            //    GameManager.Instance().PLAYER.m_name , MP.transform.position);

            NetworkManager.Instance().RequestGameSceneJoin(startPos);
            m_inGameUI.gameObject.SetActive(true);
        }
        else
        {
            // 네트워크 플레이일 경우 
            PlayerController l = MP.GetComponent<PlayerController>();
            NetworkPlayer p = MP.AddComponent<NetworkPlayer>();
            p.PlayerAnim = l.ANIMATOR;
            p.m_weaponAnchor = l.WEAPON_ANCHOR;
           // MP.GetComponent<CapsuleCollider>().enabled = false;
            //MP.GetComponent<CapsuleCollider>().isTrigger = true;
            MP.GetComponent<PlayerController>().enabled = false;
            NetworkManager.Instance().NETWORK_PLAYERS.Add(p);
            
        }

        return MP;
    }

    #region NetworkInfoChange
    public void ChangeHP(float curHp , float prevHp , float maxHp,string reason = null)
    {
        m_playerInfo.m_hp = curHp;
        m_inGameUI.PlayerHPUpdate(curHp , prevHp , maxHp);

        // 얘넨 애니메이션 재생 필요 없음
        if (reason.Equals("oxy") || reason.Equals("Meteor"))
            return;

        // 애니메이션 재생
        if (m_playerInfo.m_hp > 0.0f)
            m_playerInfo.m_player.AnimationPlay("Damage");
        else
        {
            m_playerInfo.m_player.IS_MOVE_ABLE = false;
            m_playerInfo.m_player.AnimationPlay("Dead");
            m_playerInfo.m_player.Dead();

        }
       
    }

    public void ChangeOxy(float curOxy,float prevOxy,float maxOxy)
    {
        m_playerInfo.m_oxy = curOxy;
        m_inGameUI.PlayerOxyUpdate(curOxy , prevOxy , maxOxy);
    }
    #endregion

    #region EquipEvent
    public void EquipWeapon(string itemID , int cur , int max)
    {
        if (m_inGameUI == null)
            return;
        m_inGameUI.EquipWeapon(itemID , cur , max);
    }

    public void UpdateWeapon(int cur,int max)
    {
        m_inGameUI.UpdateWeapon(cur , max);
    }

    public void UnEquipWeapon(string itemID , int cur , int max)
    {
        if (m_inGameUI == null)
            return;
        m_inGameUI.UnEquipWeapon(itemID , cur , max);
    }
    #endregion

    #region Table Info Message
    public float GetGameTableValue(string id)
    {
        for(int i = 0; i < m_gameTable.dataArray.Length; i++)
        {
            GameTableData data = m_gameTable.dataArray[i];

            if(data.Id.Equals(id))
            {
                return data.Realvalue;
            }
        }
        return 0.0f;
    }
    #endregion

    #region Network Item Logic 

    // 서버에서 생성 명령이 왔다.
    public void RecvCreateItem(string itemCode,float angleX,float angleZ)
    {
        Vector3 targetPos = GravityManager.Instance().GetPlanetPosition(12.8f , angleX , angleZ);
    }
    #endregion 

}
