using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneHitEffect : MonoBehaviour {

    // 이 친구는 한번 사용하고 삭제될 오브젝트에 생기는 것이고
    // 애니메이션에 이 함수를 호출해야함

    public void EffectEnd()
    {
        GameObject.Destroy(gameObject);
    }
}
