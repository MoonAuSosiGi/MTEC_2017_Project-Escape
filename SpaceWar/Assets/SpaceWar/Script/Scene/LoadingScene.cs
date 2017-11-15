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
        [SerializeField]
        private TextAsset m_infoTextAsset = null; ///< info 데이터가 들어갈 json
        [SerializeField]
        private TextMesh m_percentLabel = null; ///< 로딩 진행률을 알려줄 라벨
        [SerializeField]
        private TextMesh m_infoLabel = null; ///< Info 를 띄울 라벨
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
            JSONObject json = new JSONObject(m_infoTextAsset.text);
            var info = json["LoadingInfo"];
            
            int rand = Random.Range(0 , info.Count);

            m_infoLabel.text = info[rand].str;

            // 로드할 씬 이름이 잘못되었다면 에러 출력
            if (string.IsNullOrEmpty(m_loadSceneName))
            {
                Debug.LogError("ERROR Scene Name is Null.");
            }
            else
            {
                // 씬 로드
                StartCoroutine(LoadStart());
            }
            InvokeRepeating("LoadChecker" , 5.0f , 1.0f);
        }

        /**
         * @brief   실제로 씬을 부를 코루틴 함수
         */
        private IEnumerator LoadStart()
        {
            m_asyncOperation = SceneManager.LoadSceneAsync(m_loadSceneName);
            m_asyncOperation.allowSceneActivation = false;

            while (m_asyncOperation.isDone == false)
            {
                m_percentLabel.text = Mathf.Round(m_asyncOperation.progress * 100.0f).ToString() + "%";
                yield return null;
            }
        }

        /**
         * @brief   5초 후에 체크 한뒤, 로드가 되어있다면 씬전환
         * @detail  최초 체크 후엔 1초마다 체크
         */
        void LoadChecker()
        {
            Debug.Log("Loadchecker");

            // 로드가 다 되었다면 씬 체인지
            if (m_asyncOperation.progress >= 0.9f)
            {
                m_percentLabel.text = "100%";

                Invoke("LoadScene" , 1.0f);
            }
        }

        /**
         * @brief   씬을 강제 로드
         */
        void LoadScene()
        {
            SceneManager.LoadScene(m_loadSceneName);
        }
        #endregion ------------------------------------------------------------------------------------------------
    }
}