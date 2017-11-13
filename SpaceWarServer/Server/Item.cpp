#include "stdafx.h"
#include "Item.h"

/**
 * @brief	아이템의 생성자. 아이디와 네트워크 아이디 세팅
 * @param	itemID 아이템의 아이디
 * @param	networkID 아이템의 네트워크 식별 아이디
*/
Item::Item(string itemID, string networkID)
{
	m_itemID = itemID;
	m_networkID = networkID;
}

#pragma region Get / Set Method ==================================================
/**
 * @brief	아이템 아이디 얻기
 * @return	아이템의 아이디를 리턴한다. null일 수 없다.
*/
string Item::GetItemID()
{
	return m_itemID;
}
/**
 * @brief	아이템의 네트워크 아이디 얻기
 * @return	아이템의 네트워크 아이디를 리턴한다. null일 수 없다.
*/
string Item::GetNetworkID()
{
	return m_networkID;
}
/**
 * @brief	아이템의 위치를 세팅한다.
 * @detail	아이템의 네트워크 상 포지션을 세팅한다.
 * @param	pos 아이템의 포지션값
*/
void Item::SetPosition(Vector3 pos)
{
	m_pos = pos;
}
/**
 * @brief	아이템의 위치를 세팅한다.
 * @detail	아이템의 네트워크 상 포지션을 세팅한다.
 * @param	x 아이템의 x좌표
 * @param	y 아이템의 y좌표
 * @param	z 아이템의 z좌표
*/
void Item::SetPosition(float x, float y, float z)
{
	m_pos.x = x;
	m_pos.y = y;
	m_pos.z = z;
}
/**
 * @brief	아이템의 좌표를 얻어온다.
 * @return	아이템의 좌표를 리턴
*/
Vector3 Item::GetPosition()
{
	return m_pos;
}

/**
 * @brief	아이템의 회전값을 세팅한다.
 * @detail	아이템의 네트워크 상 회전값을 세팅한다.
 * @param	rot 아이템의 x,y,z의 회전값이 들어있는 벡터
*/
void Item::SetRotation(Vector3 rot)
{
	m_rot = rot;
}

/**
 * @brief	아이템의 회전값을 세팅한다.
 * @detail	아이템의 네트워크 상 회전값을 세팅한다.
 * @param	rotx 아이템의 x회전값
 * @param	roty 아이템의 y회전값
 * @param	rotz 아이템의 z회전값
*/
void Item::SetRotation(float rotx, float roty, float rotz)
{
	m_rot.x = rotx;
	m_rot.y = roty;
	m_rot.z = rotz;
}

/**
 * @brief	아이템의 회전값을 얻어온다.
 * @return	아이템의 회전값을 리턴
*/
Vector3 Item::GetRotation()
{
	return m_rot;
}
#pragma endregion ================================================================