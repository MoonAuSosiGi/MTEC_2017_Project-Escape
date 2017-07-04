using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singletone<GameManager> {

    #region GameManager_INFO
    // -- 게임 매니저 전반적인 것들을 관리합니다 ------------------------------------------//
    public GameObject PlayerPref;
    public GameObject MainCam;
    public GameObject Plant;

    public Transform PlanetAnchor;


    public GameObject[] Item;
    public Transform ItemCreaterAnchor;
    public int CreateItemNum;

    public List<GameObject> CreateWeaponList = new List<GameObject>();

    public RaycastHit SponeHitInfo;

    float deltaTime = 0.0f;

    public GameObject m_playersCreateParent = null;

    private PlayerInfo m_playerInfo = new PlayerInfo();

    public PlayerInfo PLAYER
    {
        get { return m_playerInfo; }
        set { m_playerInfo = value; }
    }

    // -------------------------------------------------------------------------------------//
    #endregion
    #region PlayerINFO
    public class PlayerInfo
    {
        public string m_name = "";
        public int m_hp = 0;
    }
    #endregion


    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        m_playerInfo.m_name = "Test";
        //PhotonNetwork.playerName = "Test" + Random.Range(0 , 22);
        //PhotonNetwork.ConnectUsingSettings("0.1");

        Application.targetFrameRate = 100;
        Screen.SetResolution(1920 / 2 , 1080 / 2 , false);

        AnchorPlanet.PlanetAnchor = PlanetAnchor;
        AnchorPlanet.Planet = Plant.transform;
        AnchorPlanet.GM = this.transform;
        Application.runInBackground = true;
    }

    public GameObject CommandItemCreate(int itemCID,int itemID,Vector3 pos,Vector3 rot)
    {
        Debug.Log("CommandItemCreate.. " + itemCID + " pos " + rot);
        int index = CreateWeaponList.Count;
        CreateWeaponList.Add(Instantiate(Item[itemCID] , pos ,
                Quaternion.Euler(rot.x , rot.y , rot.z)));
        CreateWeaponList[index].transform.position = pos;
        CreateWeaponList[index].transform.eulerAngles = rot;
        CreateWeaponList[index].GetComponent<Weapon>().WeaponID = itemID;
        CreateWeaponList[index].GetComponent<Weapon>().CID = itemCID;
        CreateWeaponList[index].layer = 2;

        return CreateWeaponList[index];
    }

    IEnumerator CreateItem(int Num)
    {
        float SponHeight = 0.15f;

       // System.Array.Resize(ref CreateWeaponList , Num);

        for (int i = 0; i < Num; i++)
        {
            int CID = Random.Range(0 , Item.Length - 1);

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
            
            CreateWeaponList.Add(Instantiate(Item[CID] , SponeHitInfo.point , 
                Quaternion.Euler(SponeHitInfo.normal.x + 45 , SponeHitInfo.normal.y + 45 , 
                SponeHitInfo.normal.z + 90)));

            CreateWeaponList[i].GetComponent<Weapon>().WeaponID = i;
            CreateWeaponList[i].GetComponent<Weapon>().CID = CID;
            CreateWeaponList[i].layer = 2;

            Vector3 SponeRot = (CreateWeaponList[i].transform.position - AnchorPlanet.PlanetAnchor.position).normalized;

            //SponeRot += CreateWeaponList[i].GetComponent<Weapon>().SponeRot;

            Quaternion targetRotation = Quaternion.FromToRotation(CreateWeaponList[i].transform.up , SponeRot) * CreateWeaponList[i].transform.rotation;



            CreateWeaponList[i].transform.rotation = targetRotation;

            CreateWeaponList[i].transform.Rotate(CreateWeaponList[i].GetComponent<Weapon>().SponeRot);

            CreateWeaponList[i].transform.Translate(Vector3.right * SponHeight);

            

            yield return new WaitForSeconds(0.001f);

        }


        for (int i = 0; i < CreateWeaponList.Count; i++)
        {
            NetworkManager.Instance().C2SRequestItemCreate(CreateWeaponList[i].GetComponent<Weapon>().CID , i , CreateWeaponList[i].transform.position ,
                CreateWeaponList[i].transform.eulerAngles);
            NetworkManager.Instance().m_networkItemList.Add(CreateWeaponList[i].GetComponent<Weapon>().WeaponID , CreateWeaponList[i]);
        }

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
            if((int)NetworkManager.Instance().m_hostID == 4)
                StartCoroutine(CreateItem(CreateItemNum));

            GameManager.Instance().PLAYER.m_name = name;
            
            MP.GetComponent<Player>().enabled = true;

            MP.AddComponent<Rigidbody>();
            MP.GetComponent<Rigidbody>().freezeRotation = true;
            MP.GetComponent<Rigidbody>().useGravity = false;

            MainCam.GetComponent<CamRotate>().Player = MP.transform;
            MainCam.GetComponent<CamRotate>().CamAnchor[0] = MP.transform.GetChild(1);
            MainCam.GetComponent<CamRotate>().CamAnchor[1] = MP.transform.GetChild(1).GetChild(0);
            MainCam.GetComponent<CamRotate>().CamAnchor[2] = MP.transform.GetChild(1).GetChild(0).GetChild(0);
            MainCam.GetComponent<CamRotate>().enabled = true;


            Plant.GetComponent<Gravity>().TargetObject = MP.GetComponent<Rigidbody>();

            AnchorPlanet.PlayerCharacter = MP.transform;

            NetworkManager.Instance().C2SRequestClientJoin(
                GameManager.Instance().PLAYER.m_name , MP.transform.position);
        }
        else
        {
            // 네트워크 플레이일 경우 
            Player l = MP.GetComponent<Player>();
            NetworkPlayer p = MP.AddComponent<NetworkPlayer>();
            p.PlayerAnim = MP.GetComponent<Player>().PlayerAnim;
            p.m_weaponAnchor = l.WeaponAnchor;
            MP.GetComponent<Player>().enabled = false;
            NetworkManager.Instance().NETWORK_PLAYERS.Add(p);
        }

        return MP;
        //TODO NETWORK
        //if (PhotonNetwork.playerList.Length - 1 == 0)
        //{
        //    StartCoroutine(CreateItem(CreateItemNum));
        //}
        //else
        //{
        //    MP.GetComponent<TestStram>().SenddataCall("00/0");
        //}
    }


}
