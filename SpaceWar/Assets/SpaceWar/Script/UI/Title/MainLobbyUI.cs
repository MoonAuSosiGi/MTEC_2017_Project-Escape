using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLobbyUI : MonoBehaviour {

    #region MainLobbyUI_INFO
    // 추후 사라지겠지만 지금은 일단 있어야 함
    public GameObject m_loginUI = null;

    // 유저정보
    public UILabel m_userName = null;
    
    // 버튼 Tween을 위한 리스트
    public List<UISprite> m_mainButtonList = new List<UISprite>();
    private int m_currentOpenButton = -1;
    private bool m_currentAniPlay = false;
    private bool m_currentHideAniPlay = false;
    #endregion

    #region UnityMethod

    void Start()
    {
        // 게임 설정 --
        Application.targetFrameRate = -1;
        Screen.SetResolution(1920 , 1080 , false);
        Application.runInBackground = true;
    }
    #endregion

    #region UserInfo

    public void OnChangeUserName(string name)
    {
        NetworkManager.Instance().USER_NAME = name;
    }
    #endregion

    #region MainLobbyButton

    public void NetworkPlayButton(UISprite bt)
    {
        if (m_loginUI.activeSelf)
            return;
        NetworkManager.Instance().USER_NAME = m_userName.text;
        m_loginUI.SetActive(!m_loginUI.activeSelf);
    }

    public void PressButton(UISprite bt)
    {
        if (m_loginUI.activeSelf)
            return;
        //TODO 재생중일때 다른거 누르면 닫히게 할 것
        if (iTween.Count() == 0 && m_mainButtonList.Contains(bt))
        {
            int index = m_mainButtonList.IndexOf(bt);

            if (m_currentOpenButton != -1)
                HideButton();
            // 같은걸 눌렀을땐 닫히기만 함
            if (m_currentOpenButton == index)
                return;
            if(m_currentAniPlay == false)
            {
                m_currentOpenButton = index;
                m_currentAniPlay = true;
                DefaultOpenTween(bt);
            }
            
        }
    }

    // 디폴트 색상 변경용
    public void MouseDownButton(UIButton bt)
    {
        bt.defaultColor = bt.pressed;
    }
    public void MouseUpButton(UIButton bt)
    {
        bt.defaultColor = bt.hover;
    }
    #endregion

    #region TweenMethod

    struct TweenData
    {
        public int index;
        public UISprite targetButton;
    }

    //d
    void DefaultOpenTween(UISprite targetButton)
    {
        //UIButton bt = targetButton.GetComponent<UIButton>();
        iTween.MoveBy(targetButton.gameObject , iTween.Hash("x" , 0.05f,
            "time" , 0.3f ,
            "oncompletetarget" ,gameObject,
            "oncomplete", "DefaultOpenTweenEnd",
            "oncompleteparams",targetButton));
    }
    

    // 서브 메뉴를 열어야함
    void DefaultOpenTweenEnd(UISprite targetButton)
    {
        if (targetButton.transform.childCount > 1)
        {
            Transform t = targetButton.transform.GetChild(1);
            t.gameObject.SetActive(true);
            SubMenuOpenTweenRight(t.GetComponent<UISprite>());
        }
        else
            m_currentAniPlay = false;
    }

    // 서브메뉴 오른쪽으로 열어라 
    void SubMenuOpenTweenRight(UISprite targetButton)
    {
        iTween.MoveBy(targetButton.gameObject , iTween.Hash("x" , 0.656f ,
            "oncompletetarget" , gameObject ,
            "time" , 0.3f ,
            "oncomplete" , "SubMenuOpenTweenRightEnd" ,
            "oncompleteparams" , targetButton));
    }

    // 다른 서브메뉴가 있다면 걔도 열어야함 체크
    void SubMenuOpenTweenRightEnd(UISprite targetButton)
    {
        if (targetButton.transform.childCount > 1)
        {
            Transform t = targetButton.transform.GetChild(1);
            t.gameObject.SetActive(true);
            SubMenuOpenTweenDown(t.GetComponent<UISprite>(),1);
        }
        else
            m_currentAniPlay = false;
    }

    // 다른 서브메뉴 열어라
    void SubMenuOpenTweenDown(UISprite targetButton, int index)
    {
        TweenData data = new TweenData();
        data.index = index + 1;
        data.targetButton = targetButton;
        iTween.MoveBy(targetButton.gameObject , iTween.Hash("y" , -0.13f * index,
           "oncompletetarget" , gameObject ,
           "time" , 0.3f ,
           "oncomplete" , "SubMenuOpenTweenDownEnd" ,
           "oncompleteparams" , data));
    }

    // 다 열었다
    void SubMenuOpenTweenDownEnd(TweenData data)
    {
        if (data.targetButton.transform.childCount > 1)
        {
            Transform t = data.targetButton.transform.GetChild(1);
            t.gameObject.SetActive(true);
            SubMenuOpenTweenDown(t.GetComponent<UISprite>(),data.index);
        }
        else
            m_currentAniPlay = false;
    }

    // 다른 버튼을 클릭하거나 하면 닫게 함
    void HideButton()
    {
        if (iTween.Count() > 0 || m_currentHideAniPlay == true)
            return;
        m_currentHideAniPlay = true;
        // 서브메뉴가 있는지 체크
        if (m_mainButtonList[m_currentOpenButton].transform.childCount > 1)
        {
            // 서브메뉴 부터 닫아야함
            Transform subMenu = m_mainButtonList[m_currentOpenButton].transform.GetChild(1);
            //서브메뉴의 서브메뉴가 있는지 체크
            if(subMenu.transform.childCount > 1)
            {
                // 있다
                //맨 밑으로 감
                Transform t = subMenu;
                
                int index = 1;
                while(true)
                {
                    if (t.childCount > 1)
                    {
                        index++;
                        t = t.GetChild(1);
                    }
                    else
                        break;
                }
                Debug.Log(" test " + index);
               
                SubMenuCloseTweenUp(t.GetComponent<UISprite>() , index);
            }
            else
            {
                // 서브메뉴 밖에 없으므로 얘는 왼쪽으로 바로
                SubMenuCloseTweenLeft(subMenu.GetComponent<UISprite>());
            }
        }else
        {
            // 메인메뉴뿐임
            DefaultCloseTween(m_mainButtonList[m_currentOpenButton]);
        }
    }
    // 위로 올리면서 닫아라
    void SubMenuCloseTweenUp(UISprite targetButton,int index)
    {
        TweenData data = new TweenData();
        data.index = index - 1;
        data.targetButton = targetButton;

        iTween.MoveBy(targetButton.gameObject , iTween.Hash("y" , 0.13f ,
           "oncompletetarget" , gameObject ,
           "time" , 0.3f ,
           "oncomplete" , "SubMenuCloseTweenUpEnd" ,
           "oncompleteparams" , data));
    }

    void SubMenuCloseTweenUpEnd(TweenData data)
    {
        data.targetButton.gameObject.SetActive(false);
        if (data.index != 1)
        {
            SubMenuCloseTweenUp(data.targetButton.transform.parent.GetComponent<UISprite>() , data.index);
        }
       else
        {
            SubMenuCloseTweenLeft(data.targetButton.transform.parent.GetComponent<UISprite>());
        }
    }

    void SubMenuCloseTweenLeft(UISprite targetButton)
    {
        iTween.MoveBy(targetButton.gameObject , iTween.Hash("x" , -0.656f ,
            "oncompletetarget" , gameObject ,
            "time" , 0.3f ,
            "oncomplete" , "SubMenuCloseTweenLeftEnd" ,
            "oncompleteparams" , targetButton));
    }

    // 최종으로 닫아야함
    void SubMenuCloseTweenLeftEnd(UISprite targetButton)
    {
        targetButton.gameObject.SetActive(false);
        DefaultCloseTween(targetButton.transform.parent.GetComponent<UISprite>());
    }
    // 마지막으로 닫힘
    void DefaultCloseTween(UISprite targetButton)
    {
        //UIButton bt = targetButton.GetComponent<UIButton>();
        iTween.MoveBy(targetButton.gameObject , iTween.Hash("x" , -0.05f ,
            "time" , 0.3f ,
            "oncompletetarget" , gameObject ,
            "oncomplete" , "DefaultCloseTweenEnd" ,
            "oncompleteparams" , targetButton));
    }

    // 다 닫힘
    void DefaultCloseTweenEnd(UISprite targetButton)
    {
        m_currentHideAniPlay = false;

        if (m_currentOpenButton == m_mainButtonList.IndexOf(targetButton))
            m_currentOpenButton = -1;
    }

    #endregion
}
