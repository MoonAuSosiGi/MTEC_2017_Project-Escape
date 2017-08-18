using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZonEffectHit : MonoBehaviour {

	public void EffectEnd()
    {
        GameObject.Destroy(gameObject);
    }
}
