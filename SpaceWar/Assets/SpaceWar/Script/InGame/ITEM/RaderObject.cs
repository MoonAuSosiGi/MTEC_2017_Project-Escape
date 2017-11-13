using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaderObject : MonoBehaviour {

    #region RaderObject INFO
    [SerializeField] private Animator m_animator = null;
    [SerializeField] private float m_raderEnergy = 100.0f;
    [SerializeField] private float m_raderUseEnergy = 3.0f;
    [SerializeField] private GameObject m_raderCharge = null;

    private bool m_isShow = false;
    public bool IS_SHOW {  get { return m_isShow; } }
    #endregion

    // 레이더 보이기
    public void ShowRader()
    {
        gameObject.SetActive(true);
        m_animator.SetInteger("RADER" , 1);
        m_isShow = true;
    }

    // 레이더 감추기
    public void HideRader()
    {
        m_isShow = false;
        m_animator.SetInteger("RADER" , 2);
    }

    // 레이더 실 기능 작동 타이밍
    public void RaderStart()
    {
        ShowUsers();
    }

   // 레이더 실 끄는 타이밍
    public void RaderEnd()
    {
        HideUsers();
    }

    // 완전 종료 타이밍
    public void RaderExit()
    {
        gameObject.SetActive(false);
        m_animator.SetInteger("RADER" , 0);
    }

    // 모든 플레이어 보이기
    void ShowUsers()
    {
        List<NetworkPlayer> players = NetworkManager.Instance().NETWORK_PLAYERS;

        for(int i = 0; i < players.Count; i++)
        {
            SkinnedMeshRenderer renderer = players[i].GetComponentInChildren<SkinnedMeshRenderer>();
            renderer.gameObject.layer = LayerMask.NameToLayer("RaderCamera");
            players[i].ChangeRaderMode();
        }
    }

    // 모든 플레이어 원래대로 돌리기
    void HideUsers()
    {
        List<NetworkPlayer> players = NetworkManager.Instance().NETWORK_PLAYERS;

        for (int i = 0; i < players.Count; i++)
        {
            SkinnedMeshRenderer renderer = players[i].GetComponentInChildren<SkinnedMeshRenderer>();
            renderer.gameObject.layer = LayerMask.NameToLayer("ShadowLayer");
            players[i].ChangeOriginalMode();
        }
    }
}
