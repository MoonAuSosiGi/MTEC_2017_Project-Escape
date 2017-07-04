using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerMove {


    public enum MoveState{
        Idle,
        WalkFoward,
        WalkBack,
        WalkLeft,
        WalkRight,
        WalkFoward_Left,
        WalkFoward_Right,
        WalkBack_Left,
        WalkBack_Right,
        RunFoward,
        RunBack,
        RunLeft,
        RunRight,
        RunFoward_Left,
        RunFoward_Right,
        RunBack_Left,
        RunBack_Right
    }

    

    public static bool CheckAnim(Animator TargetAnim, string StateName)
    {
        if (!TargetAnim.GetNextAnimatorStateInfo(0).IsName(StateName))
        {
            if (!TargetAnim.GetCurrentAnimatorStateInfo(0).IsName(StateName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public static void MoveCode(Transform MoveTarget, MoveState Dir, float Speed,string name)
    {

        Player p = MoveTarget.GetComponent<Player>();
        Animator TargetAnim = MoveTarget.GetChild(0).GetComponent<Animator>();
        float RotateSpeed = 0.23f;

        Vector3 prevPos = MoveTarget.position;

        switch (Dir)
        {
            case MoveState.Idle:

                if (CheckAnim(TargetAnim, "Idle"))
                {
                    p.AnimationSettingAndSend("WalkState", 0);
                   // TargetAnim.CrossFade("Idle", 0.2f, 0);
                }


                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, 0, 0), RotateSpeed);
                
                break;

            case MoveState.WalkFoward:
                MoveTarget.Translate(0, 0, Speed);

                if (CheckAnim(TargetAnim, "Dash_1"))
                {
                    p.AnimationSettingAndSend("WalkState", 1);
                    //TargetAnim.CrossFade("Dash_1", 0.1f, 0);

                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, 0, 0), RotateSpeed);
                break;

            case MoveState.WalkBack:
                MoveTarget.Translate(0, 0, -Speed);

                if (CheckAnim(TargetAnim, "Dash_1"))
                {
                    p.AnimationSettingAndSend("WalkState", 1);
                    //TargetAnim.CrossFade("Dash_1", 0.1f, 0);

                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation,
                    Quaternion.Euler(0, 180, 0), RotateSpeed);
                break;

            case MoveState.WalkLeft:
                MoveTarget.Translate(-Speed, 0, 0);

                if (CheckAnim(TargetAnim, "Dash_1"))
                {
                    p.AnimationSettingAndSend("WalkState", 1);
                    //TargetAnim.CrossFade("Dash_1", 0.1f, 0);

                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, -90, 0), RotateSpeed);
                break;

            case MoveState.WalkRight:
                MoveTarget.Translate(Speed, 0, 0);

                if (CheckAnim(TargetAnim, "Dash_1"))
                {
                    p.AnimationSettingAndSend("WalkState", 1);
                    //TargetAnim.CrossFade("Dash_1", 0.1f, 0);
                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, 90, 0), RotateSpeed);
                

                break;
            case MoveState.WalkFoward_Left:
                MoveTarget.Translate(Mathf.Cos(135 * Mathf.Deg2Rad) * Speed, 0, Mathf.Sin(135 * Mathf.Deg2Rad) * Speed);

                if (CheckAnim(TargetAnim, "Dash_1"))
                {
                    p.AnimationSettingAndSend("WalkState", 1);
                    //TargetAnim.CrossFade("Dash_1", 0.1f, 0);
                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, -45, 0), RotateSpeed);

                
                break;

            case MoveState.WalkFoward_Right:
                MoveTarget.Translate(Mathf.Cos(45 * Mathf.Deg2Rad) * Speed, 0, Mathf.Sin(45 * Mathf.Deg2Rad) * Speed);

                if (CheckAnim(TargetAnim, "Dash_1"))
                {
                    p.AnimationSettingAndSend("WalkState", 1);
                    //TargetAnim.CrossFade("Dash_1", 0.1f, 0);
                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, 45, 0), RotateSpeed);
                

                break;

            case MoveState.WalkBack_Left:
                MoveTarget.Translate(Mathf.Cos(225 * Mathf.Deg2Rad) * Speed, 0, Mathf.Sin(225 * Mathf.Deg2Rad) * Speed);

                if (CheckAnim(TargetAnim, "Dash_1"))
                {
                    p.AnimationSettingAndSend("WalkState", 1);
                    //TargetAnim.CrossFade("Dash_1", 0.1f, 0);
                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, -135, 0), RotateSpeed);
                

                break;

            case MoveState.WalkBack_Right:
                MoveTarget.Translate(Mathf.Cos(315 * Mathf.Deg2Rad) * Speed, 0, Mathf.Sin(315 * Mathf.Deg2Rad) * Speed);

                if (CheckAnim(TargetAnim, "Dash_1"))
                {
                    p.AnimationSettingAndSend("WalkState", 1);
                    //TargetAnim.CrossFade("Dash_1", 0.1f, 0);
                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, 135, 0), RotateSpeed);
                

                break;

            case MoveState.RunFoward:
                MoveTarget.Translate(0, 0, Speed);

                if (CheckAnim(TargetAnim, "Dash_2"))
                {
                    if (!TargetAnim.GetCurrentAnimatorStateInfo(0).IsName("Dash_2"))
                    {
                        p.AnimationSettingAndSend("WalkState", 2);
                        //TargetAnim.CrossFade("Dash_2", 0.1f, 0);
                    }
                }
                    MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, 0, 0), RotateSpeed);



                break;

            case MoveState.RunBack:
                MoveTarget.Translate(0, 0, -Speed);


                if (CheckAnim(TargetAnim, "Dash_2"))
                {
                    p.AnimationSettingAndSend("WalkState", 2);
                    //TargetAnim.CrossFade("Dash_2", 0.1f, 0);
                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, 180, 0), RotateSpeed);
                

                break;

            case MoveState.RunLeft:
                MoveTarget.Translate(-Speed, 0, 0);

                if (CheckAnim(TargetAnim, "Dash_2"))
                {
                    p.AnimationSettingAndSend("WalkState", 2);
                    //TargetAnim.CrossFade("Dash_2", 0.1f, 0);
                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, -90, 0), RotateSpeed);
                

                break;

            case MoveState.RunRight:
                MoveTarget.Translate(Speed, 0, 0);

                if (CheckAnim(TargetAnim, "Dash_2"))
                {
                    p.AnimationSettingAndSend("WalkState", 2);
                    //TargetAnim.CrossFade("Dash_2", 0.1f, 0);
                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, 90, 0), RotateSpeed);
                

                break;
            case MoveState.RunFoward_Left:
                MoveTarget.Translate(Mathf.Cos(135 * Mathf.Deg2Rad) * Speed, 0, Mathf.Sin(135 * Mathf.Deg2Rad) * Speed);

                if (CheckAnim(TargetAnim, "Dash_2"))
                {
                    p.AnimationSettingAndSend("WalkState", 2);
                    //TargetAnim.CrossFade("Dash_2", 0.1f, 0);
                }
                MoveTarget.GetChild(0).localRotation =
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, -45, 0), RotateSpeed);

                
                break;

            case MoveState.RunFoward_Right:
                MoveTarget.Translate(Mathf.Cos(45 * Mathf.Deg2Rad) * Speed, 0, Mathf.Sin(45 * Mathf.Deg2Rad) * Speed);

                if (CheckAnim(TargetAnim, "Dash_2"))
                {
                    p.AnimationSettingAndSend("WalkState", 2);
                    //TargetAnim.CrossFade("Dash_2", 0.1f, 0);
                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, 45, 0), RotateSpeed);
                

                break;

            case MoveState.RunBack_Left:
                MoveTarget.Translate(Mathf.Cos(225 * Mathf.Deg2Rad) * Speed, 0, Mathf.Sin(225 * Mathf.Deg2Rad) * Speed);

                if (CheckAnim(TargetAnim, "Dash_2"))
                {
                    p.AnimationSettingAndSend("WalkState", 2);
                    //TargetAnim.CrossFade("Dash_2", 0.1f, 0);
                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, -135, 0), RotateSpeed);
                

                break;

            case MoveState.RunBack_Right:
                MoveTarget.Translate(Mathf.Cos(315 * Mathf.Deg2Rad) * Speed, 0, Mathf.Sin(315 * Mathf.Deg2Rad) * Speed);

                if (CheckAnim(TargetAnim, "Dash_2"))
                {
                    p.AnimationSettingAndSend("WalkState", 2);
                    //TargetAnim.CrossFade("Dash_2", 0.1f, 0);
                }
                MoveTarget.GetChild(0).localRotation = 
                    Quaternion.Slerp(MoveTarget.GetChild(0).localRotation, 
                    Quaternion.Euler(0, 135, 0), RotateSpeed);
                

                break;

        }

        //Vector3 velo = (MoveTarget.position - prevPos) / Time.deltaTime;

        //NetworkManager.Instance().C2SRequestPlayerMove(name , MoveTarget.position , velo , MoveTarget.GetChild(0).localRotation);
    }


}
