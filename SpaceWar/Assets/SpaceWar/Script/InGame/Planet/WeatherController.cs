using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeForEscape.Object.Planet
{
    /**
    * @brief		WeatherController 날씨 관련 처리를 하는 컨트롤러
    * @details		낮과 밤을 바꾸는 계산도 함
    * @author		이훈 (MoonAuSosiGi@gmail.com)
    * @date		    2017-11-28
    * @file		    WeatherController.cs
    * @version		0.0.1
  */
    public class WeatherController : MonoBehaviour
    {
        #region WeatherController INFO ------------------------------------------------------------------
       
        private static string m_planetName = null; ///< 행성 이름 지정
        [SerializeField]
        private Transform m_distanceMaxObject = null; ///< 최대 거리값 기준 오브젝트
        [SerializeField]
        private Light m_mainLight = null; ///< 기준 라이트
        [SerializeField]
        private Material m_skyBox = null; ///< 스카이박스 라이팅
        [SerializeField]
        private float m_lightMax = 2.5f; ///< Light Intencity   
        [SerializeField]
        private float m_lightMin = 0.0f; ///< Light Intencity Min  
        [SerializeField]
        private float m_skyBoxSunSizeMax = 0.87f; ///< SkyBox 의 SunSize 최대값
        [SerializeField]
        private float m_skyBoxSunSizeMin = 0.87f; ///< SkyBox 의 SunSize 최소값
        [SerializeField]
        private float m_skyBoxAtmosphereThicknessMax = 0.87f; ///< SkyBox의 Atmosphere Thinkness 최대값
        [SerializeField]
        private float m_skyBoxAtmosphereThicknessMin = 0.87f; ///< SkyBox의 Atmosphere Thinkness 최소값
        [SerializeField]
        private Color m_skyColorDayLight; ///< 낮 skyColor
        [SerializeField]
        private Color m_equatorColorDayLight; ///< 낮 euqtorColor 
        [SerializeField]
        private Color m_groundColorDayLight; ///< 낮 groundColor
        [SerializeField]
        private Color m_skyColorNight; ///< 밤 skyColor
        [SerializeField]
        private Color m_equatorColorNight; ///< 밤 euqtorColor
        [SerializeField]
        private Color m_groundColorNight; ///< 밤 groundColor 

        #region WeatherController Property --------------------------------------------------------------
        /**
         * @brief   행성 이름에 대한 프로퍼티
         */
        public static string PLANET_NAME
        {
            get { return m_planetName; }
            set { m_planetName = value; }
        }
        #endregion --------------------------------------------------------------------------------------
        #endregion --------------------------------------------------------------------------------------
        #region WeatherController Method ----------------------------------------------------------------

        /**
         * @brief   시작시 테이블 값을 받고 계산 함수를 동작 시킨다.
         */
        void Start()
        {
            // 테이블 정보 세팅
            m_planetName = "Kepler";
            int targetIndex = -1;
            var planetTable = GravityManager.Instance().PLANET_TABLE;
            if (planetTable == null)
                return;

            for(int i = 0; i < planetTable.dataArray.Length; i++)
            {
                if(m_planetName.Equals(planetTable.dataArray[i].Planetname))
                {
                    targetIndex = i;
                    break;
                }
            }
            var table = planetTable.dataArray[targetIndex];
            
            m_lightMax = table.Directional_light_intensity_max;
            m_lightMin = table.Directional_light_intensity_min;

            m_skyBoxSunSizeMax = table.Skybox_sunsize_max;
            m_skyBoxSunSizeMin = table.Skybox_sunsize_min;

            m_skyBoxAtmosphereThicknessMax = table.Skybox_atmosphere_thickness_max;
            m_skyBoxAtmosphereThicknessMin = table.Skybox_atmosphere_thickness_min;

            m_skyColorDayLight = new Color(table.Ambientlight_skycolor_daylight_r ,
                table.Ambientlight_skycolor_daylight_g ,
                table.Ambientlight_skycolor_daylight_b);

            m_equatorColorDayLight = new Color(table.Ambientlight_equatorcolor_daylight_r ,
                table.Ambientlight_equatorcolor_daylight_g ,
                table.Ambientlight_equatorcolor_daylight_b);

            m_groundColorDayLight = new Color(table.Ambientlight_groundcolor_daylight_r ,
                table.Ambientlight_groundcolor_daylight_g ,
                table.Ambientlight_groundcolor_daylight_b);

            m_skyColorNight = new Color(table.Ambientlight_skycolor_night_r ,
                table.Ambientlight_skycolor_night_g ,
                table.Ambientlight_skycolor_night_b);

            m_equatorColorNight = new Color(table.Ambientlight_equatorcolor_night_r ,
                table.Ambientlight_equatorcolor_night_g ,
                table.Ambientlight_equatorcolor_night_b);

            m_groundColorNight = new Color(table.Ambientlight_groundcolor_night_r ,
                table.Ambientlight_groundcolor_night_g ,
                table.Ambientlight_groundcolor_night_b);

            StartCoroutine(DayProcess());
        }
        /**
         * @brief   낮과 밤을 계산하는 함수 ( 주기적 호출 )
         */
        IEnumerator DayProcess()
        {
            while(true)
            {
                if (GameManager.Instance().PLAYER.m_player == null)
                    yield return new WaitForSeconds(1.0f);
                float distanceMax = Vector3.Distance(transform.position , m_distanceMaxObject.position);
                float curDistance = Vector3.Distance(GameManager.Instance().PLAYER.m_player.transform.position , transform.position);
                float val = (m_lightMax / distanceMax) * curDistance;

               // Debug.LogFormat("Distance Max {0} Cur Distance {1} light {2}" , distanceMax , curDistance , val);
                // Main Light Process //

                if (val >= m_lightMax) val = m_lightMax;
                else if (val < m_lightMin) val = m_lightMin;

                m_mainLight.intensity = val;

                // SkyBox Process //
                val = (m_skyBoxSunSizeMax / distanceMax) * curDistance;

                Debug.LogFormat("Distance Max {0} Cur Distance {1} SkyBoxSize {2}" , distanceMax , curDistance , val);
                if (val >= m_skyBoxSunSizeMax) val = m_skyBoxSunSizeMax;
                else if (val < m_skyBoxSunSizeMin) val = m_skyBoxSunSizeMin;

                m_skyBox.SetFloat("_Sun Size" , val);

                val = (m_skyBoxAtmosphereThicknessMax / distanceMax) * curDistance;

                //Debug.LogFormat("Distance Max {0} Cur Distance {1} Atmosphere {2}" , distanceMax , curDistance , val);

                if (val >= m_skyBoxAtmosphereThicknessMax) val = m_skyBoxAtmosphereThicknessMax;
                else if (val < m_skyBoxAtmosphereThicknessMin) val = m_skyBoxAtmosphereThicknessMin;

                m_skyBox.SetFloat("_Atmosphere Thickness" , val);

               // Debug.LogFormat("MATT {0} " , m_skyBox.GetFloat("_Atmosphere Thickness"));
                // daylight night Process //
                val = (1.0f / distanceMax) * curDistance;

               // Debug.LogFormat("Distance Max {0} Cur Distance {1} Daylight {2}" , distanceMax , curDistance , val);

                if (val >= 1.0f) val = 1.0f;
                else if (val < 0.0f) val = 0.0f;

               
                var a = Color.Lerp(m_skyColorNight , m_skyColorDayLight , val);
                var b = Color.Lerp(m_equatorColorNight , m_equatorColorDayLight , val);
                var c = Color.Lerp(m_groundColorNight , m_equatorColorDayLight , val);
                RenderSettings.ambientSkyColor = a;
                RenderSettings.ambientEquatorColor = b;
                RenderSettings.ambientGroundColor = c;

                //Debug.LogFormat("Lerp Val {0} SkyColor {1} EquatorColor {2} GroundColor {3} " , val,a,b,c);

                yield return new WaitForSeconds(5.0f * 0.166f);
            }
           
        }

        /**
         * @brief   메테오 행성 Offset 조절값을 리턴함
         */
        public float GetMeteorOffset()
        {
            int targetIndex = -1;
            var planetTable = GravityManager.Instance().PLANET_TABLE;
            if (planetTable == null)
                return 0.0f;

            for (int i = 0; i < planetTable.dataArray.Length; i++)
            {
                if (m_planetName.Equals(planetTable.dataArray[i].Planetname))
                {
                    targetIndex = i;
                    break;
                }
            }
            if (targetIndex < 0)
                return 0.0f;
            var table = planetTable.dataArray[targetIndex];
            return table.Meteor_offset;
        }
        #endregion --------------------------------------------------------------------------------------
    }
}