using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectOff : MonoBehaviour {

    public Bullet m_target = null;

    public void EffectOffCall()
    {
        m_target.gameObject.SetActive(false);
    }

}
