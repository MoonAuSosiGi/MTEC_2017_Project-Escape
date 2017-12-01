using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singletone<GameManager> {

    #region GameManager_INFO
    
    public enum GameMode
    {
        DEATH_MATCH = 100,
        SURVIVAL
    }

    #region Table
    [SerializeField] GameTable m_gameTable = null;
 
    #region GameTable Tag
    public const string FULL_HP = "FullHP";
    public const string FULL_OXY = "FullOXY";
    public const string OXY_DAMAGE = "OxyDamage";
    public const string WALK_SPEED = "WalkSpeed";
    public const string RUN_SPEED = "RunSpeed";
    public const string DASH_TICK = "DashTick";
    public const string DASH_SPEED = "DashSpeed";
    public const string DASH_USE_OXY = "DashUseOxy";
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
    public const string RUN_SPEED_LEVEL = "RunSpeedLevel";
    public const string WALK_SPEED_LEVEL = "WalkSpeedLevel";
    public const string WEIGHT_LIMIT = "WeightLimit";
    public const string METEOR_CAMERA_ANI_DISTANCE = "MeteorCameraAniDistance";
    #endregion
    #endregion

    #region Game Manager Main
    // -- 게임 매니저 전반적인 것들을 관리합니다 ------------------------------------------//
    public GameObject PlayerPref;
    public InGameUI m_inGameUI;
    public GameObject m_meteorParent = null;
    
    public Transform ItemCreaterAnchor;
    public int CreateItemNum;

    public GameObject m_playersCreateParent = null;

    private PlayerInfo m_playerInfo = new PlayerInfo();

    public GameObject m_meteorPrefab = null;

    public PlayerInfo PLAYER
    {
        get { return m_playerInfo; }
        set { m_playerInfo = value; }
    }
    // 게임모드
    private static GameMode m_curGameMode = GameMode.SURVIVAL;
    public static GameMode CURRENT_GAMEMODE
    {
        get { return m_curGameMode; }
        set { m_curGameMode = value; }
    }
    
    #endregion

    #region NetworkObject
    public GameObject m_spaceShipParent = null;
    public GameObject m_shelterParent = null;
    public GameObject m_oxyChargerParent = null;
    public GameObject m_itemBoxParent = null;
    #endregion

    #region Meteor Info
    // 메테오 떨어지는 시간
    private int m_meteorTime = 30;
    // 메테오 아이디
    private string m_meteorID = null;

    private SortedList<string , float> m_meteorList = new SortedList<string , float>();

    // 메테오 거리별 흔들리기 효과를 주기위한 체크용 배열
    private float[] m_meteorEffectDistance;
    public float[] METEOR_EFFECT_DISTANCE { get { return m_meteorEffectDistance; } }

    #endregion
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
    [SerializeField]
    private AlertUI m_alertUI = null;
    public AlertUI ALERT { get { return m_alertUI; } }
    #endregion
    #region PlayerINFO
    public class PlayerInfo
    {
        public string m_name = "";
        public float m_hp = 100.0f;
        public float m_oxy = 100.0f;
        public float m_fullHp = 100.0f;
        public float m_fullOxy = 100.0f;
        private float m_weight = 0.0f;
        public PlayerController m_player = null;

        // 차차 다른것도 이 형태로 변경
        public float WEIGHT {
            get { return m_weight; }
            set {
                m_weight = value;

                float walkSpeed = 0.0f, runSpeed = 0.0f;
                GameManager.Instance().GetWeightSpeed(m_weight , out walkSpeed , out runSpeed);

                GameManager.Instance().m_inGameUI.ShowDebugLabel("WEIGHT " + m_weight + " speed " + walkSpeed + " run " + runSpeed);

                m_player.WALK_SPEED = walkSpeed;
                m_player.RUN_SPEED = runSpeed;
            }
        }
        public string NAME { get { return m_name; } set { m_name = value; } }
        public float HP { get { return m_hp; } set { m_hp = value; } }
        public float OXY { get { return m_oxy; } set { m_oxy = value; } }
    }
    #endregion

    #region UnityMethod
    private void Start()
    {
        // DontDestroyOnLoad(this.gameObject);

        if (GravityManager.Instance() == null || NetworkManager.Instance() == null)
            return;

        // 플레이어 정보 세팅!
        m_playerInfo.m_name = NetworkManager.Instance().USER_NAME;
        m_playerInfo.m_hp = GetGameTableValue(FULL_HP);
        m_playerInfo.m_oxy = GetGameTableValue(FULL_OXY);
        m_playerInfo.m_fullHp = GetGameTableValue(FULL_HP);
        m_playerInfo.m_fullOxy = GetGameTableValue(FULL_OXY);

        // 메테오 정보 세팅
        string[] split = GetGameTableStringValue(METEOR_CAMERA_ANI_DISTANCE).Split(',');
        m_meteorEffectDistance = new float[split.Length - 1];

        for(int i = 0; i < m_meteorEffectDistance.Length; i++)
        {
            m_meteorEffectDistance[i] = float.Parse(split[i]);
        }

        float planetScale = GravityManager.Instance().CurrentPlanet.transform.localScale.x + 20.8f;

        OnJoinedRoom(m_playerInfo.m_name , true , new Vector3(9.123454f , 48.63797f , -32.4867f));
            //GetPlanetPosition(planetScale , Random.Range(-360.0f , 360.0f) , Random.Range(-360.0f , 360.0f)));
    }

    public float PLANET_XANGLE = 0.0f;
    public float PLANET_ZANGLE = 0.0f;

    #endregion

    #region Network Start 

    public GameObject OnJoinedRoom(string name , bool me , Vector3 startPos)
    {

        GameObject MP = GameObject.Instantiate(this.PlayerPref);
        MP.transform.parent = m_playersCreateParent.transform;

        MP.transform.position = new Vector3(startPos.x , startPos.y , startPos.z);
        MP.transform.rotation = Quaternion.identity;

        MP.name = name;

        if (me)
        {
            // 네트워크 매니저 세팅 --
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
            player.TableSetup();
            player.SetUserName(NetworkManager.Instance().USER_NAME);

            GameManager.Instance().PLAYER.m_player = player;

            NetworkManager.Instance().RequestGameSceneJoin(startPos);

            // --- 옵저버용 카메라 세팅 ----
            var observer = Camera.main.GetComponent<TimeForEscape.Object.NetworkObject>();
            WeaponManager.Instance().AddObserverCamera(
                (int)NetworkManager.Instance().HOST_ID , observer);
            // -----------------------------
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

            p.NetworkPlayerColorSetup(NetworkManager.Instance().NETWORK_PLAYERS.Count - 1);
        }

        return MP;
    }
    #endregion

    #region Meteor Logic
    // 메테오 생성
    public void CreateMeteor(float anglex,float anglez,string meteorID)
    {
        Vector3 pos =  GravityManager.Instance().GetPlanetPosition(anglex , anglez);

        Debug.Log("METEOR " + pos + " ax " + anglex + " az " + anglez);

        m_meteorList.Add(meteorID , 30.0f);
        
        GameObject obj = Instantiate(m_meteorPrefab , new Vector3(pos.x,pos.y,pos.z),Quaternion.Euler(0.0f,0.0f,0.0f));
        
        obj.transform.rotation = Quaternion.LookRotation((pos - Vector3.zero).normalized);
        Vector3 r = obj.transform.eulerAngles;
        obj.transform.eulerAngles =new Vector3( r.x + 90.0f,r.y,r.z);

        //obj.transform.SetParent(m_meteorParent.transform,false);

        m_alertUI.AlertShow(AlertUI.AlertType.METEOR_ATTACK , meteorID , m_meteorTime , "Meteor Attack");
        //m_inGameUI.RecvMeteorInfo(m_meteorTime);
        //m_inGameUI.StartMeteor();

        if(IsInvoking("MeteorTimer") == false)
            InvokeRepeating("MeteorTimer" , Time.deltaTime , Time.deltaTime);
    }

    public void CreateMeteor(Vector3 pos, string id)
    {
        m_meteorList.Add(id , 30.0f);

        GameObject obj = Instantiate(m_meteorPrefab , new Vector3(pos.x , pos.y +1.5f , pos.z) , Quaternion.Euler(0.0f , 0.0f , 0.0f));

        obj.transform.rotation = Quaternion.LookRotation((pos - Vector3.zero).normalized);
        Vector3 r = obj.transform.eulerAngles;
        obj.transform.eulerAngles = new Vector3(r.x + 90.0f , r.y , r.z);

        //obj.transform.SetParent(m_meteorParent.transform,false);

        m_alertUI.AlertShow(AlertUI.AlertType.METEOR_ATTACK , id , m_meteorTime , "Meteor Attack");
        //m_inGameUI.RecvMeteorInfo(m_meteorTime);
        //m_inGameUI.StartMeteor();

        if (IsInvoking("MeteorTimer") == false)
            InvokeRepeating("MeteorTimer" , Time.deltaTime , Time.deltaTime);
    }
    void MeteorTimer()
    {
        for(int i = 0; i < m_meteorList.Keys.Count; i++)
        {
            string id = m_meteorList.Keys[i];
            m_meteorList[id] -= Time.deltaTime;
            float time = m_meteorList[id];
            m_alertUI.AlertShow(AlertUI.AlertType.METEOR_ATTACK , id , Mathf.RoundToInt(time) , "Meteor Attack");

            if (time < 0.0f )
            {
                m_meteorList.Remove(id);
                m_alertUI.AlertHide(id);
                
                if(m_meteorList.Keys.Count <= 0)
                {
                    CancelInvoke("MeteorTimer");
                }
            }
        }
    }
    #endregion

    #region NetworkInfoChange
    public void ChangeHP(float curHp , float prevHp , float maxHp,string reason = null)
    {
        m_playerInfo.m_hp = curHp;
        m_inGameUI.PlayerHPUpdate(curHp , prevHp , maxHp);

        //// 얘넨 애니메이션 재생 필요 없음
        //if (reason.Equals("oxy") || reason.Equals("Meteor"))
        //    return;

        // 애니메이션 재생
        if (m_playerInfo.m_hp <= 0.0f)
        {
            m_playerInfo.m_player.IS_MOVE_ABLE = false;
            m_playerInfo.m_player.AnimationPlay("Dead");
            m_playerInfo.m_player.Dead();
            if(reason.Equals("DeathZone"))
            {
                m_playerInfo.m_player.IS_DEATHZONE_DEAD = true;
            }
            CameraManager.Instance().ShowDeadCameraEffect();

        }
       
    }

    public void ChangeOxy(float curOxy,float prevOxy,float maxOxy)
    {
        m_playerInfo.m_oxy = curOxy;
        m_inGameUI.PlayerOxyUpdate(curOxy , prevOxy , maxOxy);
    }
    #endregion

    #region EquipEvent
    public void EquipWeapon(string itemID, int cur , int max)
    {
        if (m_inGameUI == null)
            return;
        m_inGameUI.EquipWeapon(itemID ,PLAYER.m_player.CUR_EQUIP_INDEX, cur , max);
    }

    public void UpdateWeapon(int cur,int max)
    {
        m_inGameUI.UpdateWeapon(cur , max);
    }

    public void UnEquipWeapon(int index,bool iconHide=false)
    {
        if (m_inGameUI == null)
            return;
        m_inGameUI.UnEquipWeapon(index,iconHide);
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

    public string GetGameTableStringValue(string id)
    {
        for (int i = 0; i < m_gameTable.dataArray.Length; i++)
        {
            GameTableData data = m_gameTable.dataArray[i];

            if (data.Id.Equals(id))
            {
                return data.Realvalues;
            }
        }
        return null;
    }
    
    // 값이 변동될 때만 부를 것
    public void GetWeightSpeed(float weight,out float walkSpeed, out float runSpeed)
    {
        string[] weights = GetGameTableStringValue(WEIGHT_LIMIT).Split(',');
        string[] walkSpeeds = GetGameTableStringValue(WALK_SPEED_LEVEL).Split(',');
        string[] runSpeeds = GetGameTableStringValue(RUN_SPEED_LEVEL).Split(',');

        // 거꾸로 훑어야 함
        for(int i = weights.Length -1; i >= 0; i--)
        {
            if(float.Parse(weights[i]) >= weight)
            {
                walkSpeed = float.Parse(walkSpeeds[i]);
                runSpeed = float.Parse(runSpeeds[i]);
                return;
            }
        }
        walkSpeed = 5;
        runSpeed = 8;
    }
    #endregion

    #region Network Item Logic 

    public GameObject CommandItemCreate(string itemID , string networkID , Vector3 pos , Vector3 rot)
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
            RaycastHit SponeHitInfo;
            Physics.Raycast(ItemCreaterAnchor.GetChild(0).position , ItemCreaterAnchor.rotation * Vector3.forward , out SponeHitInfo , 30.0f);



            while (SponeHitInfo.collider.gameObject.CompareTag("NonSpone")
                || SponeHitInfo.collider.gameObject.CompareTag("PlayerCharacter"))
            {
                ItemCreaterAnchor.rotation = Quaternion.Euler(Random.Range(-360f , 360f) ,
                    Random.Range(-360f , 360f) , Random.Range(-360f , 360f));

                ItemCreaterAnchor.GetChild(0).LookAt(ItemCreaterAnchor);

                Physics.Raycast(ItemCreaterAnchor.GetChild(0).position ,
                    ItemCreaterAnchor.rotation * Vector3.forward , out SponeHitInfo , 30.0f);

            }

            // 웨폰 생성 리스트의 관리는 ?
            string id = WeaponManager.Instance().GetRandomWeaponID();
            GameObject weapon = WeaponManager.Instance().CreateWeapon(id);
            weapon.transform.position = SponeHitInfo.point; //new Vector3(SponeHitInfo.normal.x + 45.0f , SponeHitInfo.normal.y + 45.0f , SponeHitInfo.normal.z + 90.0f); //ItemCreaterAnchor.GetChild(0).position;
            Item item = weapon.GetComponent<Item>();
            try
            {
                item.ITEM_NETWORK_ID = NetworkManager.Instance().m_hostID + "_" + i + "_" + id;
                NetworkManager.Instance().ITEM_DICT.Add(
                    item.ITEM_NETWORK_ID , item);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
                Debug.Log("item ID " + item.ITEM_NETWORK_ID);
            }
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
    #region Network - ItemBox
    public void RecvItem(string itemID , string networkID , GameObject box)
    {
        Vector3 boxPos = box.transform.position;
        GameObject weapon = WeaponManager.Instance().CreateWeapon(itemID);

        weapon.transform.position = boxPos;
      //  Vector3 scale = weapon.transform.localScale;
        //  weapon.transform.localScale = new Vector3(0.0f , 0.0f , 0.0f);
        Item item = weapon.GetComponent<Item>();
        item.ITEM_NETWORK_ID = networkID;

        NetworkManager.Instance().ITEM_DICT.Add(networkID , item);
        NetworkManager.Instance().C2SRequestItemCreate(
            item.ITEM_ID ,
            networkID ,
            item.transform.position ,
            item.transform.localEulerAngles);

        //iTween.ScaleTo(weapon , iTween.Hash(
        //    "x" , scale.x , "y" , scale.y , "z" , scale.z ,
        //    "oncompletetarget" , gameObject ,
        //    "easeType","easeOutCubic",
        //    "speed",300.0f,
        //    "oncomplete" , "RecvItemTweenEnd", 
        //    "oncompleteparams", networkID));


    }

    void RecvItemTweenEnd(string networkID)
    {
        // 딱히 하는거 없음 
        Item item = NetworkManager.Instance().ITEM_DICT[networkID];
        NetworkManager.Instance().C2SRequestItemCreate(
            item.ITEM_ID ,
            networkID ,
            item.transform.position ,
            item.transform.localEulerAngles);
    }
    #endregion

    #endregion 

}
