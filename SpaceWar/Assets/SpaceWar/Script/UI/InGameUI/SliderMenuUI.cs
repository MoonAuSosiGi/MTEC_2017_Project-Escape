using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderMenuUI : MonoBehaviour {

    // 이친구는 모든 슬라이드 메뉴에 활용이 되는 친구다. 
    // GameManager 로 접근하도록 하자
    #region SliderMenuUI INFO
    [SerializeField] private UISlider m_slider = null;
    #endregion

    #region SliderMenuUI Method
    public void Reset()
    {
        m_slider.value = 0.0f;
    }

    public void SliderProcess(float sliderValue)
    {
        m_slider.value = sliderValue;
    }

    public void ShowSlider()
    {
        gameObject.SetActive(true);
    }

    public void HideSlider()
    {
        gameObject.SetActive(false);
    }
    #endregion
}
