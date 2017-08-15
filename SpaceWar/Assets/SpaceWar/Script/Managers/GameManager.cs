using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singletone<GameManager> {

    #region GameManager_INFO
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
    #region NetworkObject
    public GameObject m_spaceShipParent = null;
    public GameObject m_shelterParent = null;
    public GameObject m_oxyChargerParent = null;
    public GameObject m_itemBoxParent = null;
    #endregion

    // 메테오 떨어지는 시간
    private int m_meteorTime = 30;

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
        public PlayerController m_player = null;
    }
    #endregion

    #region UnityMethod
    private void Awake()
    {
        // DontDestroyOnLoad(this.gameObject);

        if (GravityManager.Instance() == null)
            return;

        m_playerInfo.m_name = NetworkManager.Instance().USER_NAME;
       

        float planetScale = GravityManager.Instance().CurrentPlanet.transform.localScale.x + 12.8f;
        OnJoinedRoom(m_playerInfo.m_name , true , //new Vector3(9.123454f , 48.63797f , -32.4867f));
            GetPlanetPosition(planetScale , Random.Range(-360.0f , 360.0f) , Random.Range(-360.0f , 360.0f)));
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

    public GameObject CommandItemCreate(int itemCID,int itemID,Vector3 pos,Vector3 rot)
    {
        Debug.Log("CommandItemCreate.. " + itemCID + " pos " + rot);
        int index = CreateWeaponList.Count;
        CreateWeaponList.Add(Instantiate(Item[itemCID-1] , pos ,
                Quaternion.Euler(rot.x , rot.y , rot.z)));
        CreateWeaponList[index].transform.position = pos;
        CreateWeaponList[index].transform.eulerAngles = rot;
        
        CreateWeaponList[index].GetComponent<WeaponItem>().ITEM_NETWORK_ID = itemID;
        
        CreateWeaponList[index].layer = 2;

        return CreateWeaponList[index];
    }

    IEnumerator CreateItem(int Num)
    {
        float SponHeight = 0.15f;

       // System.Array.Resize(ref CreateWeaponList , Num);

        for (int i = 0; i < Num; i++)
        {
            int CID =  Random.Range(1 ,Item.Length+1);

            Debug.Log("Item Create " + CID + " item Name " +Item[CID-1]);
            //ItemCreaterAnchor.Rotate(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));


            ItemCreaterAnchor.rotation = Quaternion.Euler(Random.Range(-360f , 360f) , Random.Range(-360f , 360f) , Random.Range(-360f , 360f));
            

            ItemCreaterAnchor.GetChild(0).LookAt(ItemCreaterAnchor);

            Physics.Raycast(ItemCreaterAnchor.GetChild(0).position , ItemCreaterAnchor.rotation * Vector3.forward , out SponeHitInfo , 30f);



            while (SponeHitInfo.collider.gameObject.CompareTag("NonSpone") 
                || SponeHitInfo.collider.gameObject.CompareTag("PlayerCharacter"))
            {
                ItemCreaterAnchor.rotation = Quaternion.Euler(Random.Range(-360f , 360f) ,
                    Random.Range(-360f , 360f) , Random.Range(-360f , 360f));
                
                ItemCreaterAnchor.GetChild(0).LookAt(ItemCreaterAnchor);

                Physics.Raycast(ItemCreaterAnchor.GetChild(0).position , 
                    ItemCreaterAnchor.rotation * Vector3.forward , out SponeHitInfo , 30f);

            }
            
            CreateWeaponList.Add(Instantiate(Item[CID-1] , SponeHitInfo.point , 
                Quaternion.Euler(SponeHitInfo.normal.x + 45 , SponeHitInfo.normal.y + 45 , 
                SponeHitInfo.normal.z + 90)));
          //  CreateWeaponList[i].transform.parent = m_itemParent.transform;
          //  CreateWeaponList[i].GetComponent<WeaponItem>().ITEM_ID = CID;
            CreateWeaponList[i].layer = 2;

            CreateWeaponList[i].GetComponent<WeaponItem>().ITEM_NETWORK_ID = i;

            Vector3 SponeRot = (CreateWeaponList[i].transform.position - GravityManager.Instance().CurrentPlanet.transform.position).normalized;

            //SponeRot += CreateWeaponList[i].GetComponent<Weapon>().SponeRot;

            Quaternion targetRotation = Quaternion.FromToRotation(CreateWeaponList[i].transform.up , SponeRot) * CreateWeaponList[i].transform.rotation;



       //    CreateWeaponList[i].transform.rotation = targetRotation;

            CreateWeaponList[i].transform.Rotate(CreateWeaponList[i].GetComponent<WeaponItem>().SPONE_ROTATITON);

            CreateWeaponList[i].transform.Translate(Vector3.right * SponHeight);

            

            yield return new WaitForSeconds(0.001f);

        }


        for (int i = 0; i < CreateWeaponList.Count; i++)
        {
            NetworkManager.Instance().C2SRequestItemCreate(CreateWeaponList[i].GetComponent<WeaponItem>().ITEM_ID ,
                i , CreateWeaponList[i].transform.position ,
                CreateWeaponList[i].transform.eulerAngles);
            NetworkManager.Instance().m_networkItemList.Add(i, 
                CreateWeaponList[i]);
        }

    }

    public GameObject test;

    // 메테오 생성
    public void CreateMeteor(float anglex,float anglez)
    {
        float planetScale = Plant.transform.localScale.x + 12.8f;

        Vector3 pos = GetPlanetPosition(planetScale , anglex , anglez);
        Vector3 pos2 = GetPlanetPosition(planetScale + 30.0f , anglex , anglez);

        Physics.Raycast(Vector3.zero , (pos - Vector3.zero).normalized , out SponeHitInfo , 40.0f);


        GameObject obj = Instantiate(m_meteorPrefab , new Vector3(pos.x,pos.y,pos.z),Quaternion.Euler(0.0f,0.0f,0.0f));
        
        obj.transform.rotation = Quaternion.LookRotation((pos - Vector3.zero).normalized);
        Vector3 r = obj.transform.eulerAngles;
        obj.transform.eulerAngles =new Vector3( r.x + 90.0f,r.y,r.z);

        obj.transform.parent = m_meteorParent.transform;

        // 테스트용 위치 확인
        test.transform.position = pos;
        test.transform.rotation = Quaternion.LookRotation((pos - Vector3.zero).normalized);

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
    
    public void RecvItem(int itemCID,GameObject box)
    {
        Vector3 boxPos = box.transform.position;
        CreateWeaponList.Add(Instantiate(
                Item[itemCID] ,SponeHitInfo.point ,
                Quaternion.Euler(SponeHitInfo.normal.x + 45 , SponeHitInfo.normal.y + 45 ,
                SponeHitInfo.normal.z + 90)));

        GameObject obj = CreateWeaponList[CreateWeaponList.Count - 1];

        //obj.transform.parent = box.transform ;
        obj.transform.position = boxPos;

        Vector3 scale = obj.transform.localScale;
        obj.transform.localScale = new Vector3(0.0f , 0.0f , 0.0f);
        CreateWeaponList[CreateWeaponList.Count - 1].GetComponent<WeaponItem>().ITEM_NETWORK_ID = CreateWeaponList.Count - 1;
        
        CreateWeaponList[CreateWeaponList.Count - 1].layer = 2;

        
        iTween.ScaleTo(obj , iTween.Hash(
            "x" , scale.x , "y" , scale.y , "z" , scale.z ,
            "oncompletetarget" , gameObject ,
            "easeType","easeOutCubic",
            "speed",300.0f,
            "oncomplete" , "RecvItemTweenEnd", 
            "oncompleteparams", (CreateWeaponList.Count - 1)));
        

    }
    
    void RecvItemTweenEnd(int index)
    {
        
        // 딱히 하는거 없음 

        NetworkManager.Instance().C2SRequestItemCreate(
            CreateWeaponList[index]
            .GetComponent<WeaponItem>().ITEM_ID , index ,
            CreateWeaponList[index].transform.position ,
                CreateWeaponList[index].transform.eulerAngles);
        NetworkManager.Instance().m_networkItemList.Add(
            CreateWeaponList[index].GetComponent<WeaponItem>().ITEM_NETWORK_ID ,
            CreateWeaponList[index]);
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
    public void ChangeHP(float curHp , float prevHp , float maxHp)
    {
        m_playerInfo.m_hp = curHp;
        if (m_playerInfo.m_hp > 0.0f)
            m_playerInfo.m_player.AnimationPlay("Damage");
        else
        {
            m_playerInfo.m_player.IS_MOVE_ABLE = false;
            m_playerInfo.m_player.AnimationPlay("Dead");
        }
        m_inGameUI.PlayerHPUpdate(curHp , prevHp , maxHp);
    }

    public void ChangeOxy(float curOxy,float prevOxy,float maxOxy)
    {
        m_playerInfo.m_oxy = curOxy;
        m_inGameUI.PlayerOxyUpdate(curOxy , prevOxy , maxOxy);
    }
    #endregion

    #region EquipEvent
    public void EquipWeapon(int itemCID,int cur,int max)
    {
        m_inGameUI.EquipWeapon(itemCID , cur , max);
    }

    public void UnEquipWeapon(int itemCID,int cur,int max)
    {
        m_inGameUI.UnEquipWeapon(itemCID , cur , max);
    }
    #endregion
    


}
