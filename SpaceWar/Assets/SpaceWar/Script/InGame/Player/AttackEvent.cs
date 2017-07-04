using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEvent : MonoBehaviour {

    

    public GameObject Weapon;
    public Transform Character;

    public GameObject WeaponAnchor;

    public void Attack(int Type)
    {
        if (Character.GetComponent<Player>().enabled == true)
        {
            this.transform.localRotation = Quaternion.Euler(Vector3.zero);
            Weapon.GetComponent<Weapon>().Attack(Weapon.GetComponent<Weapon>().Myname, Type, Character);
            Character.GetComponent<Player>().IsMoveable = false;
        }
    }


    public void RemoteAttack(int Type, Vector3 Positon, Quaternion Rotation)
    {
        
        Weapon.GetComponent<Weapon>().RemoteAttack(Weapon.GetComponent<Weapon>().Myname, Type, Positon, Rotation);
        
    }

    public void CanMove()
    {
        Character.GetComponent<Player>().IsMoveable = true;
    }

}
