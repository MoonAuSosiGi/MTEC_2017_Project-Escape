using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nettention.Proud;

public class NetworkPlayer : MonoBehaviour {

    #region NetworkPlayer_INFO
    // -- Network Player -----------------------------------------------------------//
    public Animator PlayerAnim;
    public HostID m_hostID;
    public string m_userName = "";
    public WeaponItem m_weapon = null;
    public GameObject m_weaponAnchor;

    public WeaponItem HAS_WEAPON
    {
        get { return m_weapon; }
        set {
            m_weapon = value;

            if(m_weapon == null)
            {
                Debug.Log("설마");
                // 수류탄이었으므로 애니메이션 변경
                PlayerController p = this.GetComponent<PlayerController>();
                PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_BAREHAND);
            }
        }
    }

    PositionFollower m_positionFollower = new PositionFollower();
    AngleFollower m_playerCamSeeX = new AngleFollower();
    AngleFollower m_playerCamSeeY = new AngleFollower();
    AngleFollower m_playerCamSeeZ = new AngleFollower();

    AngleFollower m_playerSeeX = new AngleFollower();
    AngleFollower m_playerSeeY = new AngleFollower();
    AngleFollower m_playerSeeZ = new AngleFollower();

    public HostID HOST_ID { get { return m_hostID; } }
    #endregion

    public void NetworkPlayerSetup(HostID hostID,string userName)
    {
        m_hostID = hostID;
        m_userName = userName;
    }

    public void RecvNetworkMove(UnityEngine.Vector3 pos,UnityEngine.Vector3 velocity, UnityEngine.Vector3 charrot, UnityEngine.Vector3 rot)
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
        //test code
        if (aniValue == 1234)
        {
            PlayerAnim.Play(animationName);
        }
        else
        {
            PlayerAnim.SetInteger(animationName , aniValue);

            if (m_weapon == null)
                return;

       //     if (m_weapon.ATTACK_TIMING == WeaponItem.AttackTiming.SCRIPT_ONLY)
                m_weapon.SoundPlay();

            // 근거리 무기용
            if(animationName.Equals("ATTACK") && aniValue == 1 && m_weapon.WEAPON_TYPE == WeaponItem.WeaponType.MELEE)
            {
                m_weapon.PLAYER = transform;
                m_weapon.AttackSword();
            }
            else if (animationName.Equals("ATTACK") && aniValue == 0 && m_weapon.WEAPON_TYPE == WeaponItem.WeaponType.MELEE)
            {
                m_weapon.AttackSwordEnd();
            }
        }
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
      
    }

    public void EquipWeapon(GameObject weapon)
    {
        if(weapon != null)
        {
            m_weapon = weapon.GetComponent<WeaponItem>() ;
            Debug.Log("Weapon " + m_weapon.ITEM_NETWORK_ID + " type " + m_weapon.WEAPON_TYPE + " name " + m_weapon.name);
            m_weapon.transform.parent = m_weaponAnchor.transform;
            m_weapon.transform.localPosition = m_weapon.LOCAL_SET_POS;
            m_weapon.transform.localRotation = Quaternion.Euler(m_weapon.LOCAL_SET_ROT);
            m_weapon.transform.localScale = m_weapon.LOCAL_SET_SCALE;
            m_weapon.GetComponent<SphereCollider>().enabled = false;

            PlayerController p = this.GetComponent<PlayerController>();
            
            Debug.Log("prev " + PlayerAnim.runtimeAnimatorController.name);
            switch (m_weapon.WEAPON_TYPE)
            {
                case WeaponItem.WeaponType.GUN:   PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_GUN01); break;
                case WeaponItem.WeaponType.RIFLE: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_GUN02); break;
                case WeaponItem.WeaponType.MELEE: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_MELEE); break;
                case WeaponItem.WeaponType.ROCKETLAUNCHER: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_ROCKETLAUNCHER); break;
                case WeaponItem.WeaponType.ETC_GRENADE: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_ETC); break;
                case WeaponItem.WeaponType.ETC_RECOVERY: PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_ETC); break;
            }
            Debug.Log("result " + PlayerAnim.runtimeAnimatorController.name);
        }
    }

    public void UnEquipWeapon(UnityEngine.Vector3 pos, UnityEngine.Vector3 rot)
    {
        if(m_weapon != null)
        {
            // 버리는 로직
            m_weapon.transform.parent = null;
            m_weapon.transform.position = pos;
            m_weapon.transform.eulerAngles = rot;
            m_weapon.GetComponent<SphereCollider>().enabled = true;

            PlayerController p = this.GetComponent<PlayerController>();

            Debug.Log("UnEquip ");
            PlayerAnim.runtimeAnimatorController = p.GetCurrentAnimator(PlayerController.AnimationType.ANI_BAREHAND);
        }
    }
}
