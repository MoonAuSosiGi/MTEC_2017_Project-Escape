#include "stdafx.h"
#include "Item.h"

/**
 * @brief	�������� ������. ���̵�� ��Ʈ��ũ ���̵� ����
 * @param	itemID �������� ���̵�
 * @param	networkID �������� ��Ʈ��ũ �ĺ� ���̵�
*/
Item::Item(string itemID, string networkID)
{
	m_itemID = itemID;
	m_networkID = networkID;
}

#pragma region Get / Set Method ==================================================
/**
 * @brief	������ ���̵� ���
 * @return	�������� ���̵� �����Ѵ�. null�� �� ����.
*/
string Item::GetItemID()
{
	return m_itemID;
}
/**
 * @brief	�������� ��Ʈ��ũ ���̵� ���
 * @return	�������� ��Ʈ��ũ ���̵� �����Ѵ�. null�� �� ����.
*/
string Item::GetNetworkID()
{
	return m_networkID;
}
/**
 * @brief	�������� ��ġ�� �����Ѵ�.
 * @detail	�������� ��Ʈ��ũ �� �������� �����Ѵ�.
 * @param	pos �������� �����ǰ�
*/
void Item::SetPosition(Vector3 pos)
{
	m_pos = pos;
}
/**
 * @brief	�������� ��ġ�� �����Ѵ�.
 * @detail	�������� ��Ʈ��ũ �� �������� �����Ѵ�.
 * @param	x �������� x��ǥ
 * @param	y �������� y��ǥ
 * @param	z �������� z��ǥ
*/
void Item::SetPosition(float x, float y, float z)
{
	m_pos.x = x;
	m_pos.y = y;
	m_pos.z = z;
}
/**
 * @brief	�������� ��ǥ�� ���´�.
 * @return	�������� ��ǥ�� ����
*/
Vector3 Item::GetPosition()
{
	return m_pos;
}

/**
 * @brief	�������� ȸ������ �����Ѵ�.
 * @detail	�������� ��Ʈ��ũ �� ȸ������ �����Ѵ�.
 * @param	rot �������� x,y,z�� ȸ������ ����ִ� ����
*/
void Item::SetRotation(Vector3 rot)
{
	m_rot = rot;
}

/**
 * @brief	�������� ȸ������ �����Ѵ�.
 * @detail	�������� ��Ʈ��ũ �� ȸ������ �����Ѵ�.
 * @param	rotx �������� xȸ����
 * @param	roty �������� yȸ����
 * @param	rotz �������� zȸ����
*/
void Item::SetRotation(float rotx, float roty, float rotz)
{
	m_rot.x = rotx;
	m_rot.y = roty;
	m_rot.z = rotz;
}

/**
 * @brief	�������� ȸ������ ���´�.
 * @return	�������� ȸ������ ����
*/
Vector3 Item::GetRotation()
{
	return m_rot;
}
#pragma endregion ================================================================