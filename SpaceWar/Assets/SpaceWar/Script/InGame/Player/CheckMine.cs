using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckMine : MonoBehaviour {

    private void OnEnable()
    {
        if (this.gameObject.name != "MyCharac")
        {
            Destroy(this.GetComponent<Rigidbody>());
        }
    }
}
