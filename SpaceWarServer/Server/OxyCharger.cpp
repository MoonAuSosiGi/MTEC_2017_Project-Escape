#include "stdafx.h"
#include "OxyCharger.h"


/**
 * @brief	�⺻ ������ / ��� ���� ���·� �����.
*/
OxyCharger::OxyCharger()
{
	m_oxyChargerID = -1;
	UnLockOxyCharger();
}


#pragma region  Get / Set Method ===============================================================

/**
 * @brief	��� ������ �ĺ� ���̵� ����
 * @param	oxyChargerID ��� ������ ��Ʈ��ũ �ĺ� ���̵�
*/
void OxyCharger::SetOxyChargerID(int oxyChargerID)
{
	m_oxyChargerID = oxyChargerID;
}

/**
 * @brief	��� ������ �ĺ� ���̵� ���
 * @return	��� ������ �ĺ� ���̵� ��ȯ�Ѵ�. ( -1 �� ��� ������ �ȵǾ��ִ� �� )
*/
int OxyCharger::GetOxyChargerID()
{
	return m_oxyChargerID;
}
/**
 * @brief	����ִ��� ����
 * @return	true �� ��� ��� ������ �������
*/
bool OxyCharger::IsLocked() 
{ 
	return m_isLocked; 
}

/**
 * @brief	���� �������� Ŭ���̾�Ʈ HostID ����
 * @detail	-1 �ϰ�� �ƹ��� �������� �ƴϴ�
 * @return	���� �������� Ŭ���̾�Ʈ�� HostID �� int ������ ����
*/
int OxyCharger::GetUseHostID() 
{ 
	return m_useHostID; 
}
#pragma endregion ==============================================================================
