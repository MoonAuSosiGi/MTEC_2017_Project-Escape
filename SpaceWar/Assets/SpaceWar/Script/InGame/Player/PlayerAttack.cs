using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerAttack{

    public enum WeaponName
    {
        Gun_01
    }

    public static void Attack(string AnimationName)
    {

    }

    public static void Attack(WeaponName WeaponName, GameObject Bullet, Vector3 FirePos, Quaternion FireRotatin)
    {
        switch (WeaponName)
        {
            case (WeaponName.Gun_01):

                break;

        }
    }

    public static void AttackAnimPlay(Animator TargetAnim, string AnimationName, int AnimLayer)
    {
        

        if (PlayerMove.CheckAnim(TargetAnim, AnimationName))
        {
            TargetAnim.Play(AnimationName, AnimLayer);
        }

    }
}
