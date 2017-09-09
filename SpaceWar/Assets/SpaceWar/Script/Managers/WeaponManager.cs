using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : Singletone<WeaponManager> {

    // WeaponManager - 총알 및 기타 무기의 동기화에 관한 처리를 함
    // 네트워크 생성 요청 / 이동 / 삭제 또한 여기서 하도록
    #region Weapon Manager INFO ----------------------------------------------------------------------------

    public enum WeaponType
    {
        GUN = 0,
        RIFLE,
        ROCKET_LAUNCHER,
        SWORD

    }

    #region Weapon Gun Bullet 

    #region Bullet List Info
    // 살아있는 총알의 부모
    [SerializeField] Transform m_aliveNetworkBulletParent = null;
    // 죽어있는 총알의 부모
    [SerializeField] Transform m_deadNetworkBulletParent = null;

    // 살아있는 내 총알의 부모
    [SerializeField] Transform m_myAliveBulletParent = null;
    // 죽어있는 내 총알의 부모
    [SerializeField] Transform m_myDeadBulletParent = null;


    // 총알 리스트 / 내가 쏜 총알 리스트 [ 살아있음 ]
    [SerializeField]  private List<Bullet> m_myAlivebulletList = new List<Bullet>();
    // 총알 리스트 / 내가 쏜 총알 리스트 [ 죽어있음 ]
    [SerializeField]  private List<Bullet> m_myDeadBulletList = new List<Bullet>();
    // 네트워크 총알 리스트 [ 살아있음 ]
    [SerializeField]  private List<Bullet> m_networkAliveBulletList = new List<Bullet>();
    // 네트워크 총알 리스트 [ 죽음 - 재사용 ]
    [SerializeField]  private List<Bullet> m_networkDeadBulletList = new List<Bullet>();
    #endregion


    #endregion

    #region Weapon Grenade 
    // 수류탄에 관련된 처리는 여기서 한다.
    #endregion

    #region Table Data 
    [SerializeField] private WeaponTable m_weaponTable = null;
    Dictionary<string , WeaponTableData> m_weaponDict = new Dictionary<string , WeaponTableData>();
    #endregion

    #endregion

    // 임시로 여기서 테이블을 불러옴
    void Start()
    {
       // LoadTable();
    }

    #region Table Data Getting ------------------------------------------------------------------------------

    public void LoadTable()
    {
        for (int i = 0; i < m_weaponTable.dataArray.Length; i++)
            m_weaponDict.Add(m_weaponTable.dataArray[i].Id , m_weaponTable.dataArray[i]);
    }
    // 모든 무기 종류 수 반환
    public int GetTotalWeaponCount()
    {
        return m_weaponTable.dataArray.Length;
    }

    private WeaponTableData GetWeaponData(string id)
    {
        if (m_weaponDict.Count == 0)
            LoadTable();
        return m_weaponDict[id];   
    }
    #endregion


    // 여기 메소드들은 NetworkManager 에서만 호출이 가능하다
    #region Network Message Recv Function ------------------------------------------------------------------
    // 생성 요청
    public void NetworkBulletCreateRequest(string bulletID,string weaponID ,Vector3 pos,Vector3 rot)
    {
        // 생성을 할때 기존 네트워크 사망 총알리스트에서 가용한 게 있는지 확인
        // 가용한 것이라 함은 ALIVE 가  FALSE 인 총알들

        Bullet bullet = null;
        int index = -1;
        for(int i = 0; i < m_networkDeadBulletList.Count; i++)
        {
            if (m_networkDeadBulletList[i].WEAPON_ID.Equals(weaponID))
                continue;

            bullet = m_networkDeadBulletList[i];
            index = i;
            break;
        }

        // 가용한 것이 없을 경우 생성
        if (bullet == null)
        {
            GameObject prefab = CreateBullet(weaponID,false);

            if (prefab == null)
            {
                Debug.Log("ERROR 총알 prefab 이 등록되어있지 않습니다. 확인 요망  WeaponID " + weaponID + " bullet ID " + bulletID);
                return;
            }
            // 프리팹에서 사용할 Bullet Component 를 꺼냄
            bullet = prefab.GetComponent<Bullet>();

            // 네트워크 기본 셋업을 진행
            bullet.NetworkBulletEnable();
        }

        
        bullet.NETWORK_ID = bulletID;

        bullet.IS_REMOTE = true;
        bullet.IS_ALIVE = true;
        m_networkAliveBulletList.Add(bullet);

        // 위치 변경
        
        bullet.NetworkReset(new Nettention.Proud.Vector3(pos.x , pos.y , pos.z));
        bullet.transform.localEulerAngles = rot;
        bullet.transform.parent = m_aliveNetworkBulletParent;
        bullet.gameObject.SetActive(true);
        // 정상 생성 되었는지 확인용
        Debug.Log("총알 정상 생성 여부 " + (bullet != null));
    }

    // 이동 
    public void NetworkBulletMoveRecv(string bulletID, Vector3 pos,Vector3 velo,Vector3 rot)
    {
        // 단순 이동로직
        for (int i = 0; i < m_networkAliveBulletList.Count; i++)
        {
            Bullet b = m_networkAliveBulletList[i];

            if (b.NETWORK_ID.Equals(bulletID))
            {
                b.NetworkMoveRecv(pos , velo , rot);
                return;
            }
        }
    }

    // 삭제 요청
    public void NetworkBulletRemoveRequest(string bulletID)
    {
        // 삭제 요청이 들어올 경우 Bullet의 ALIVE 를 False 로 두고 현 리스트에서
        
        int deleteIndex = -1;
        for(int i = 0; i < m_networkAliveBulletList.Count; i ++)
        {
            Bullet b = m_networkAliveBulletList[i];
            if(b.NETWORK_ID.Equals(bulletID))
            {
                b.IS_ALIVE = false;
                deleteIndex = i;
                break;
            }
        }
        // 사망 리스트로 옮김
        if (deleteIndex != -1)
        {
            Bullet b = m_networkAliveBulletList[deleteIndex];
            m_networkDeadBulletList.Add(b);
            m_networkAliveBulletList.RemoveAt(deleteIndex);
            b.transform.parent = m_deadNetworkBulletParent;
        }

    }

    #endregion

    // 여기 메소드들은 로컬에서 호출하나 네트워크 메시지에 관련되어 있음 (당연히 무기에 관련)
    #region Network Message Request --------------------------------------------------------------------

    // 총알 관련된 메소드 
    #region Bullet Request Message
    // 총알 생성 요청
    public void RequestBulletCreate(string bulletID,string weaponID,Vector3 pos, Vector3 rot)
    {
        // 메시지도 보내고 실 생성도 하고 ( 메시지는 자신한테 안옴 )

        // 메시지 보내는 부분
        if(NetworkManager.Instance() != null)
            NetworkManager.Instance().C2SRequestBulletCreate(bulletID , weaponID , pos , rot);

        // 생성하는 부분을 넣읍시다 TODO
        Bullet bullet = null;

        //재사용 가능한게 있는지 체크
        for(int i = 0; i < m_myDeadBulletList.Count; i++)
        {
            if (m_myDeadBulletList[i].WEAPON_ID.Equals(weaponID))
            {
                bullet = m_myDeadBulletList[i];
                break;
            }
        }
    
        // 재사용 로직
        if(bullet == null)
        {
            GameObject prefab = CreateBullet(weaponID);

            if (prefab == null)
            {
                Debug.Log("ERROR 총알 prefab 이 등록되어있지 않습니다. 확인 요망  Weapon ID " + weaponID + " bullet ID " + bulletID);
                return;
            }
            // 프리팹에서 사용할 Bullet Component 를 꺼냄
            bullet = prefab.GetComponent<Bullet>();
        }
        
        bullet.BULLET_TRAIL_EFFECT.GetComponentInChildren<TrailRenderer>().Clear();
        bullet.DAMAGE = m_weaponDict[weaponID].Damage;

        // 부모 바꿈
        bullet.transform.parent = m_myAliveBulletParent;
        bullet.NETWORK_ID = bulletID;
        bullet.IS_REMOTE = false;
        m_myAlivebulletList.Add(bullet);
        

        // 위치 변경
        bullet.transform.position = pos;
        bullet.transform.localEulerAngles = rot;

        bullet.BulletSetup();
        bullet.gameObject.SetActive(true);
    }

    // 총알 삭제 요청
    public void RequestBulletRemove(Bullet b)
    {
        // 삭제 메시지도 보내고 실 삭제도 하고
        // 메시지 보내는 부분
        if(NetworkManager.Instance() != null)
          NetworkManager.Instance().C2SRequestBulletRemove(b.NETWORK_ID);

        // 실 삭제하는 부분
        m_myAlivebulletList.Remove(b);
        m_myDeadBulletList.Add(b);
        b.transform.parent = m_myDeadBulletParent;
    }

    // 총알 이동 메시지 요청 ( 이친구는 P2P로 이동됨 )
    public void RequestBulletMove(string bulletID,Vector3 pos,Vector3 velo,Vector3 rot)
    {
        NetworkManager.Instance().C2SRequestBulletMove(bulletID , pos , velo , rot);
    }
    #endregion

    #endregion

    #region Util Method -------------------------------------------------------------------------------------------------

    // 랜덤으로 무기 ID 뽑기
    public string GetRandomWeaponID()
    {
        return m_weaponTable.dataArray[Random.Range(0 , m_weaponTable.dataArray.Length)].Id;
    }

    // 총알 만들기
    public GameObject CreateBullet(string weaponID,bool isME = true)
    {
        WeaponTableData data = GetWeaponData(weaponID);
        if (string.IsNullOrEmpty(data.Bulletpath))
        {
            Debug.Log("ERROR Bullet Path 가 제대로 들어가있지 않음");
            return null;
        }
        
        GameObject bullet = new GameObject("Bullet");
        bullet.tag = "Bullet";
        // 총알 생성 후 bullet 밑에 자식으로 붙임
        GameObject trail = (GameObject.Instantiate(Resources.Load("Art/Resource/Effect/" + data.Bulletpath)) as GameObject);
        trail.transform.parent = bullet.transform;
        trail.SetActive(true);

        #region Bullet Hit Effect 
        // 이것들은 필요시 활성화 됨
        // 없을 수 있으니 먼저 체크
        GameObject hit = null;
        GameObject otherHit = null;
        if (string.IsNullOrEmpty(data.Basehiteffect) == false)
            hit = GameObject.Instantiate(Resources.Load("Art/Resource/Effect/" + data.Basehiteffect)) as GameObject;
        if(string.IsNullOrEmpty(data.Otherhiteffect) == false)
            otherHit = GameObject.Instantiate(Resources.Load("Art/Resource/Effect/" + data.Otherhiteffect)) as GameObject;

        if (hit != null)
        {
            hit.SetActive(false);
            hit.transform.parent = bullet.transform;
        }
        if (otherHit != null)
        {
            otherHit.SetActive(false);
            otherHit.transform.parent = bullet.transform;
        }
        #endregion

        #region Bullet Collider Attach
        SphereCollider col = bullet.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = data.Bulletcolliderradius;
        Rigidbody rb = bullet.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        #endregion
        // 타입 비교 후 스크립트 넣기
        switch ((WeaponType)data.Type)
        {
            case WeaponType.GUN:
            case WeaponType.RIFLE:
                {
                    // 기본 총기들은 비슷함
                    Bullet b = bullet.AddComponent<Bullet>();
                    b.SPEED = data.Bulletspeed;
                    b.BULLET_HIT_EFFECT = hit;
                    b.BULLET_OTHER_HIT_EFFECT = otherHit;
                    b.BULLET_TRAIL_EFFECT = trail;
                    b.WEAPON_ID = weaponID;
                }
                break;
            case WeaponType.ROCKET_LAUNCHER:
                {
                    RocketBullet r = bullet.AddComponent<RocketBullet>();
                    r.SPEED = data.Bulletspeed;
                    r.BULLET_HIT_EFFECT = hit;
                    r.BULLET_OTHER_HIT_EFFECT = otherHit;
                    r.BULLET_TRAIL_EFFECT = trail;
                    r.GRAVITY_POWER = data.Launchergravity;
                    r.WEAPON_ID = weaponID;

                    if(isME)
                    {
                        // 폭발 콜라이더 넣기
                        col = r.BULLET_HIT_EFFECT.AddComponent<SphereCollider>();
                        col.isTrigger = true;
                        col.radius = data.Boomeffectcolliderradius;
                        col = r.BULLET_OTHER_HIT_EFFECT.AddComponent<SphereCollider>();
                        col.isTrigger = true;
                        col.radius = data.Boomeffectcolliderradius;

                        WeaponItem item = GameManager.Instance().PLAYER.m_player.CURRENT_WEAPON;

                        r.BULLET_HIT_EFFECT.AddComponent<RocketBulletExplosion>().ROCKET = item;
                        r.BULLET_OTHER_HIT_EFFECT.AddComponent<RocketBulletExplosion>().ROCKET = item;
                    }
                }
                break;
            case WeaponType.SWORD:
                break;
        }

        // TODO
        // 이부분에서 사운드 지정

        return bullet;
    }

    // 무기 만들기
    public GameObject CreateWeapon(string id)
    {
        WeaponTableData data = GetWeaponData(id);
        GameObject weapon = GameObject.Instantiate(Resources.Load("Art/Resource/Item/Weapon/" + data.Prefabpath)) as GameObject;
        
        // 공통구간
        WeaponItem item = weapon.AddComponent<WeaponItem>();

        // 아이디 등록
        item.ITEM_ID = id;
        // 무게 등록
        item.ITEM_WEIGHT = data.Weight;
        // 이름 등록 
        item.ITEM_NAME = data.Name_kr;
        //데미지
        item.DAMAGE = data.Damage;
        // 장착 포지션
        item.LOCAL_SET_POS = new Vector3(data.Equippos_x , data.Equippos_y , data.Equippos_z);
        // 장착 회전
        item.LOCAL_SET_ROT = new Vector3(data.Equiprot_x , data.Equiprot_y , data.Equiprot_z);
        // 장착 스케일
        item.LOCAL_SET_SCALE = new Vector3(data.Equipscale_x , data.Equipscale_y , data.Equipscale_z);
        // 스폰 각도
        item.SPONE_ROTATITON = new Vector3(data.Sponerot_x , data.Sponerot_y , data.Sponerot_z);
        // 쿨타임
        item.COOL_TIME = data.Cooltime;
        // 어택 타이밍
        item.ATTACK_TIMING = (WeaponItem.AttackTiming)data.Attacktiming;
        // 콜라이더 삽입
        SphereCollider col = weapon.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = data.Weapongetcolliderradius;

        if((WeaponType)data.Type != WeaponType.SWORD)
        {
            // 파이어 포인트 삽입
            GameObject firePoint = new GameObject("firePoint");
            firePoint.transform.position = new Vector3(data.Firepoint_x , data.Firepoint_y , data.Firepoint_z);
            firePoint.transform.SetParent(weapon.transform , true);
            item.FIRE_POS = firePoint.transform;
            #region Shot Effect Create
            GameObject shotEffect = GameObject.Instantiate(Resources.Load("Art/Resource/Effect/" + data.Shoteffectpath)) as GameObject;
            shotEffect.transform.parent = weapon.transform;
            shotEffect.gameObject.SetActive(false);
            item.SHOT_EFFECT = shotEffect;
            #endregion
        }

        weapon.tag = "Weapon";
      
        
        switch ((WeaponType)data.Type)
        {
            case WeaponType.GUN:             item.WEAPON_TYPE = WeaponItem.WeaponType.GUN;   break;
            case WeaponType.RIFLE:           item.WEAPON_TYPE = WeaponItem.WeaponType.RIFLE;  break;
            case WeaponType.ROCKET_LAUNCHER: item.WEAPON_TYPE = WeaponItem.WeaponType.ROCKETLAUNCHER; break;
            case WeaponType.SWORD:
                {
                    #region Sword Setup
                    item.WEAPON_TYPE = WeaponItem.WeaponType.MELEE;
                    BoxCollider boxCol = weapon.AddComponent<BoxCollider>();
                    boxCol.isTrigger = true;
                    boxCol.center = new Vector3(data.Swordcollider_center_x , data.Swordcollider_center_y , data.Swordcollider_center_z);
                    boxCol.size = new Vector3(data.Swordcollider_x , data.Swordcollider_y , data.Swordcollider_z);

                    string[] check = data.Shoteffectpath.Split(',');

                    if (check.Length > 1)
                    {
                        for(int i = 0; i < check.Length; i++)
                        {
                            GameObject shotEffect = GameObject.Instantiate(Resources.Load("Art/Resource/Effect/" + check[0])) as GameObject;
                            shotEffect.transform.parent = item.transform;
                            item.SWORD_ANIMATOR_LIST.Add(shotEffect.GetComponent<Animator>());
                            shotEffect.SetActive(false);

                            // plane 에 넣어야함 // shotEffect - AllPosition - Rotation - plane
                            Transform plane = shotEffect.transform.GetChild(0).GetChild(1).GetChild(0);

                            BoxCollider hitCol = plane.gameObject.AddComponent<BoxCollider>();
                            hitCol.isTrigger = true;
                            hitCol.center = new Vector3(data.Swordcollider_center_x , data.Swordcollider_center_y , data.Swordcollider_center_z);
                            hitCol.size = new Vector3(data.Swordcollider_x , data.Swordcollider_y , data.Swordcollider_z);

                            SwordCollider sc = plane.gameObject.AddComponent<SwordCollider>();
                            sc.TARGET_SWORD = item;
                            sc.BASE_HIT_EFFECT = data.Basehiteffect;
                            sc.OTHER_HIT_EFFECT = data.Otherhiteffect;
                        }
                    }
                    else
                    {
                        GameObject shotEffect = GameObject.Instantiate(Resources.Load("Art/Resource/Effect/" + data.Shoteffectpath)) as GameObject;
                        shotEffect.transform.parent = item.transform;
                        item.SWORD_ANIMATOR_LIST.Add(shotEffect.GetComponent<Animator>());
                        shotEffect.SetActive(false);

                        // plane 에 넣어야함 // shotEffect - AllPosition - Rotation - plane
                        Transform plane = shotEffect.transform.GetChild(0).GetChild(1).GetChild(0);

                        BoxCollider hitCol = plane.gameObject.AddComponent<BoxCollider>();
                        hitCol.isTrigger = true;
                        hitCol.center = new Vector3(data.Swordcollider_center_x , data.Swordcollider_center_y , data.Swordcollider_center_z);
                        hitCol.size = new Vector3(data.Swordcollider_x , data.Swordcollider_y , data.Swordcollider_z);

                        SwordCollider sc = plane.gameObject.AddComponent<SwordCollider>();
                        sc.TARGET_SWORD = item;
                        sc.BASE_HIT_EFFECT = data.Basehiteffect;
                        sc.OTHER_HIT_EFFECT = data.Otherhiteffect;
                    }

                    

                    #endregion

                }
                break;
        }


        return weapon;
    }

    

    #endregion
}
