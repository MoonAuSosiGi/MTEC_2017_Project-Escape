using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {

    public Weapon.WeaponName Myname;

    public int WeaponID;
    public int CID;

    public string AttackAnimName;

    public GameObject[] Effect;
    public GameObject[] Bullet;
    public int CurrentBulletNum = 0;
    public Transform FirePoint;

    public Vector3 LocalSetPos;
    public Vector3 LocalSetRot;
    public Vector3 LocalSetScale;

    public Vector3 ThrowSetScale;

    public Vector3 SponeRot;

    public float AttackCoolTime;

    public enum WeaponName
    {
        Gun_01
    }

    private void Start()
    {
        for (int i=0;i< Bullet.Length; i++)
        {
            Bullet[i].GetComponent<Gun01Bullet>().WeaponID = WeaponID;
            Bullet[i].GetComponent<Gun01Bullet>().Mynum = i;
        }
    }


    public void Attack(WeaponName Weapon,int Type, Transform Character)
    {
        switch (Weapon)
        {
            case (WeaponName.Gun_01):

                switch (Type)
                {
                    case (0):

                        Gun_01_Attack(Character);

                        break;
                }

                break;
        }

       
    }

    public void RemoteAttack(WeaponName Weapon, int Type, Vector3 Positon, Quaternion Rotation)
    {
        switch (Weapon)
        {
            case (WeaponName.Gun_01):

                switch (Type)
                {
                    case (0):

                        Remote_Gun_01_Attack(Positon,Rotation);

                        break;
                }

                break;
        }


    }

    private void Gun_01_Attack(Transform Character)
    {
        Effect[0].transform.position = FirePoint.position;
        Effect[0].SetActive(true);

        Bullet[CurrentBulletNum].transform.parent = null;
        Bullet[CurrentBulletNum].transform.position = FirePoint.position;
        Bullet[CurrentBulletNum].transform.rotation = Character.rotation;
        Bullet[CurrentBulletNum].GetComponent<Gun01Bullet>().IsRemote = false;
        Bullet[CurrentBulletNum].SetActive(true);
        Bullet[CurrentBulletNum].GetComponent<Gun01Bullet>().m_bulletID =
            GameManager.Instance().PLAYER.m_name + "_Gun01Bullet_" + CurrentBulletNum;
        // 생성 명령
        NetworkManager.Instance().C2SRequestBulletCreate(Bullet[CurrentBulletNum].GetComponent<Gun01Bullet>().m_bulletID ,
            FirePoint.position , Bullet[CurrentBulletNum].transform.localEulerAngles);

        if (CurrentBulletNum == Bullet.Length - 1)
        {
            CurrentBulletNum = 0;
        }
        else
        {
            CurrentBulletNum++;
        }

    }
    private void Remote_Gun_01_Attack( Vector3 Position, Quaternion Rotation)
    {
        Effect[0].transform.position = FirePoint.position;
        Effect[0].SetActive(true);

        Bullet[CurrentBulletNum].transform.parent = null;
        Bullet[CurrentBulletNum].transform.position = Position;
        Bullet[CurrentBulletNum].transform.rotation = Rotation;
       


        Bullet[CurrentBulletNum].GetComponent<Gun01Bullet>().IsRemote = true;
        Bullet[CurrentBulletNum].SetActive(true);
        Bullet[CurrentBulletNum].GetComponent<Gun01Bullet>().m_bulletID =
           GameManager.Instance().PLAYER.m_name + "_Gun01Bullet_" + CurrentBulletNum;
        // 생성 명령
        NetworkManager.Instance().C2SRequestBulletCreate(Bullet[CurrentBulletNum].GetComponent<Gun01Bullet>().m_bulletID ,
            Position , Bullet[CurrentBulletNum].transform.localEulerAngles);

        if (CurrentBulletNum == Bullet.Length - 1)
        {
            CurrentBulletNum = 0;
        }
        else
        {
            CurrentBulletNum++;
        }
    }
}
