using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour {

    #region Meteor_INFO

    #endregion

    public void MeteorAnimationEvent()
    {
        Camera.main.GetComponent<UBER_GlobalParams>().enabled = true;
    }

    public void MeteorAnimationEnd()
    {
        Camera.main.GetComponent<UBER_GlobalParams>().enabled = false;

        GameObject.Destroy(transform.parent.gameObject);
    }
}
