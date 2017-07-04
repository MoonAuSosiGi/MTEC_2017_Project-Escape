using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{

    public Rigidbody TargetObject;
    public Vector3 GravityUp;
    public float Power;


    private void FixedUpdate()
    {
        GravityShell();
    }

    void GravityShell()
    {
        if (TargetObject == null)
            return;
        GravityUp = (TargetObject.transform.position - this.transform.position).normalized;

        TargetObject.AddForce(GravityUp * -1 * Power);
        Quaternion targetRotation = Quaternion.FromToRotation(TargetObject.transform.up , GravityUp) * TargetObject.transform.rotation;

        TargetObject.transform.rotation = targetRotation;

    }
}
