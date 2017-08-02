using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipResult : MonoBehaviour {

    public void ResultAnimationStart()
    {
        NetworkManager.Instance().C2SRequestGameEnd();
    }

    public void ResultUIAlready()
    {
        GameManager.Instance().ResultUIAlready();
    }
}
