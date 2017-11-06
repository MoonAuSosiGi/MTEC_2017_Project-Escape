#include "stdafx.h"
#include "SpaceShip.h"

/**
 * @brief	���ּ� �����ڿ��� �ʱ�ȭ ��ƾ ����
*/
SpaceShip::SpaceShip()
{
	m_spaceShipID = -1;
	m_targetHostID = -1;
	m_isLocked = false;
}

/**
 * @brief	Ŭ���̾�Ʈ�� ��û�� ���ּ��� ��ٴ�
 * @param	hostID ��û�� Ŭ���̾�Ʈ�� HostID
*/
void SpaceShip::LockSpaceShip(int hostID) 
{ 
	m_isLocked = true; 
	m_targetHostID = hostID; 
}
/**
 * @brief	Ŭ���̾�Ʈ�� ���ּ� ��ȣ�ۿ��� ���� ��� ����	
*/
void SpaceShip::UnLockSpaceShip() 
{
	m_isLocked = false; 
	m_targetHostID = -1; 
}
/**
 * @brief	����ִ��� ����
 * @return	��������� true / ����������� false;
*/
bool SpaceShip::IsLock() 
{ 
	return m_isLocked; 
}
/**
 * @brief	�������� Ŭ���̾�Ʈ HostID ����
 * @return	HostID ���� 
*/
int SpaceShip::GetTargetHostID() { return m_targetHostID; }