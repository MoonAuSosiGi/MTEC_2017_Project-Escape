#include "stdafx.h"
#include "Shelter.h"

/**
 * @brief	���� �⺻ ������ / ���� �ʱ�ȭ
*/
Shelter::Shelter()
{
	m_doorState = false;
	m_lightState = false;
	m_shelterHumanCounter = 0;
}

#pragma region InGame Shelter Method =============================================
/**
 * @brief	���Ϳ� Ŭ���̾�Ʈ�� ������ �� ó��
 * @detail	���� ��� ���� �� ����Ʈ ���� ó��
*/
void Shelter::ShelterEnter()
{
	m_shelterHumanCounter++;

	///< ����� ��� ������ ���� �Ҵ�.
	m_lightState = true;
}

/**
 * @brief	���Ϳ��� Ŭ���̾�Ʈ�� ������ �� ó��
 * @detail	���� ��� ���� �� ����Ʈ ���� ó��
*/
void Shelter::ShelterExit()
{
	m_shelterHumanCounter--;

	// ���Ͱ� ����� ��
	if (m_shelterHumanCounter <= 0)
	{
		m_shelterHumanCounter = 0;
		m_lightState = false;
	}
}

/**
 * @brief	���� �� ���� �ݰ��� ó��
 * @param	state true�� ��� ���� / false �� ��� ����
*/
void Shelter::ShelterDoorStateChange(bool state)
{
	m_doorState = state;
}
#pragma endregion ================================================================

#pragma region Get / Set Method ==================================================
/**
 * @brief	���� ���̵� ����
 * @param	shelterID ������ ������ �ε���
*/
void Shelter::SetShelterID(int shelterID)
{
	m_shelterID = shelterID;
}

/**
 * @brief	���� ���̵� ���
 * @return	������ ��Ʈ��ũ ���̵�
*/
int Shelter::GetShelterID()
{
	return m_shelterID;
}

/**
 * @brief	���� �� ��� �� ���
 * @return	���� �ȿ� �ִ� ��� ��
*/
int Shelter::GetShelterHuman()
{
	return m_shelterHumanCounter;
}

/**
 * @brief	���� ���� �����ִ���
 * @return	true �� ������ ��� ��������
*/
bool Shelter::IsDoorOpen()
{
	return m_doorState;
}

/**
 * @brief	���� ����Ʈ ���� ���
 * @retutrn true �� ������ ��� ��������
*/
bool Shelter::IsLightOn()
{
	return m_lightState;
}
#pragma endregion ================================================================