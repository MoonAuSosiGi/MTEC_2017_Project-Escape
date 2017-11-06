#include "stdafx.h"
#include "Shelter.h"

/**
 * @brief	쉘터 기본 생성자 / 변수 초기화
*/
Shelter::Shelter()
{
	m_doorState = false;
	m_lightState = false;
	m_shelterHumanCounter = 0;
}

#pragma region InGame Shelter Method =============================================
/**
 * @brief	쉘터에 클라이언트가 들어왔을 때 처리
 * @detail	내부 사람 증가 및 라이트 켜짐 처리
*/
void Shelter::ShelterEnter()
{
	m_shelterHumanCounter++;

	///< 사람이 들어 왔으니 불을 켠다.
	m_lightState = true;
}

/**
 * @brief	쉘터에서 클라이언트가 나갔을 때 처리
 * @detail	내부 사람 감소 및 라이트 꺼짐 처리
*/
void Shelter::ShelterExit()
{
	m_shelterHumanCounter--;

	// 쉘터가 비었을 때
	if (m_shelterHumanCounter <= 0)
	{
		m_shelterHumanCounter = 0;
		m_lightState = false;
	}
}

/**
 * @brief	쉘터 문 열고 닫고의 처리
 * @param	state true일 경우 열림 / false 일 경우 닫힘
*/
void Shelter::ShelterDoorStateChange(bool state)
{
	m_doorState = state;
}
#pragma endregion ================================================================

#pragma region Get / Set Method ==================================================
/**
 * @brief	쉘터 아이디 세팅
 * @param	shelterID 세팅할 쉘터의 인덱스
*/
void Shelter::SetShelterID(int shelterID)
{
	m_shelterID = shelterID;
}

/**
 * @brief	쉘터 아이디 얻기
 * @return	쉘터의 네트워크 아이디
*/
int Shelter::GetShelterID()
{
	return m_shelterID;
}

/**
 * @brief	쉘터 안 사람 수 얻기
 * @return	쉘터 안에 있는 사람 수
*/
int Shelter::GetShelterHuman()
{
	return m_shelterHumanCounter;
}

/**
 * @brief	쉘터 문이 열려있는지
 * @return	true 를 리턴할 경우 열려있음
*/
bool Shelter::IsDoorOpen()
{
	return m_doorState;
}

/**
 * @brief	쉘터 라이트 상태 얻기
 * @retutrn true 를 리턴할 경우 켜져있음
*/
bool Shelter::IsLightOn()
{
	return m_lightState;
}
#pragma endregion ================================================================