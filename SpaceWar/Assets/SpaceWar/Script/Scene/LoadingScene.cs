using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TimeForEscape.Util.Scene
{ 
    /**
    * @brief		Loading Scene
    * @details		로딩 전용 씬
    * @author		이훈 (MoonAuSosiGi@gmail.com)
    * @date			2017-11-11
    * @file			LoadingScene.cs
    * @version		0.0.1
    */
    public class LoadingScene : MonoBehaviour
    {
        #region Loading Scene INFO --------------------------------------------------------------------------------
        [SerializeField] private UISlider m_loadingBar = null; ///< 로딩 진행률을 나타낼 UI
        [SerializeField] private UILabel m_percentLabel = null; ///< 로딩 진행률을 알려줄 라벨
        private static string m_loadSceneName = null; ///< 로딩할 씬 이름
        private AsyncOperation m_asyncOperation = null; ///< Progress를 알아올 객체
        #region Loading Scene Property ----------------------------------------------------------------------------
        
        /**
         * @brief   로드할 씬의 이름
         * @detail  로드할 씬의 이름을 세팅하고 이 로딩 씬을 불러와야 한다.
         */
        public static string LOAD_SCENE_NAME { get { return m_loadSceneName; } set { m_loadSceneName = value; } }
        #endregion ------------------------------------------------------------------------------------------------
        #endregion ------------------------------------------------------------------------------------------------

        #region Loading Scene Method ------------------------------------------------------------------------------
        /**
         * @brief   부를 씬의 이름이 지정되어있다면 자동으로 씬 로드
         */
        void Start()
        {
            // 로드할 씬 이름이 잘못되었다면 에러 출력
            if(string.IsNullOrEmpty(m_loadSceneName))
            {
                Debug.LogError("ERROR Scene Name is Null.");
            }
            else
            {
                // 씬 로드
                StartCoroutine(LoadStart());
            }
        }

        /**
         * @brief   실제로 씬을 부를 코루틴 함수
         */
        private IEnumerator LoadStart()
        {
            m_asyncOperation = SceneManager.LoadSceneAsync(m_loadSceneName);
            m_asyncOperation.allowSceneActivation = true;

            while(m_asyncOperation.isDone == false)
            {
                Debug.Log("Load Start " + m_asyncOperation.progress.ToString());
                m_loadingBar.value = m_asyncOperation.progress;
                m_percentLabel.text = (m_asyncOperation.progress * 100.0f).ToString("F1") + "%";
                yield return null;
            }
       //     SceneManager.LoadScene(m_loadSceneName);
        }
        #endregion ------------------------------------------------------------------------------------------------
    }
}