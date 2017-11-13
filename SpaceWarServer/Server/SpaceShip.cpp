#include "stdafx.h"
#include "SpaceShip.h"

/**
 * @brief	우주선 생성자에서 초기화 루틴 수행
*/
SpaceShip::SpaceShip()
{
	m_spaceShipID = -1;
	m_targetHostID = -1;
	m_isLocked = false;
}

/**
 * @brief	클라이언트가 요청해 우주선을 잠근다
 * @param	hostID 요청한 클라이언트의 HostID
*/
void SpaceShip::LockSpaceShip(int hostID) 
{ 
	m_isLocked = true; 
	m_targetHostID = hostID; 
}
/**
 * @brief	클라이언트의 우주선 상호작용이 끝나 잠금 해제	
*/
void SpaceShip::UnLockSpaceShip() 
{
	m_isLocked = false; 
	m_targetHostID = -1; 
}
/**
 * @brief	잠겨있는지 유무
 * @return	잠겨있으면 true / 안잠겨있으면 false;
*/
bool SpaceShip::IsLock() 
{ 
	return m_isLocked; 
}
/**
 * @brief	조작중인 클라이언트 HostID 리턴
 * @return	HostID 리턴 
*/
int SpaceShip::GetTargetHostID() { return m_targetHostID; }