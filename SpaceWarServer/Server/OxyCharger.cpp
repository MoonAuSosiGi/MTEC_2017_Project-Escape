#include "stdafx.h"
#include "OxyCharger.h"


/**
 * @brief	기본 생성자 / 사용 가능 상태로 만든다.
*/
OxyCharger::OxyCharger()
{
	m_oxyChargerID = -1;
	UnLockOxyCharger();
}


#pragma region  Get / Set Method ===============================================================

/**
 * @brief	산소 충전기 식별 아이디 세팅
 * @param	oxyChargerID 산소 충전기 네트워크 식별 아이디
*/
void OxyCharger::SetOxyChargerID(int oxyChargerID)
{
	m_oxyChargerID = oxyChargerID;
}

/**
 * @brief	산소 충전기 식별 아이디 얻기
 * @return	산소 충전기 식별 아이디를 반환한다. ( -1 일 경우 세팅이 안되어있는 것 )
*/
int OxyCharger::GetOxyChargerID()
{
	return m_oxyChargerID;
}
/**
 * @brief	잠겨있는지 리턴
 * @return	true 일 경우 산소 충전기 잠겨있음
*/
bool OxyCharger::IsLocked() 
{ 
	return m_isLocked; 
}

/**
 * @brief	현재 조작중인 클라이언트 HostID 리턴
 * @detail	-1 일경우 아무도 조작중이 아니다
 * @return	현재 조작중인 클라이언트의 HostID 가 int 형으로 리턴
*/
int OxyCharger::GetUseHostID() 
{ 
	return m_useHostID; 
}
#pragma endregion ==============================================================================
