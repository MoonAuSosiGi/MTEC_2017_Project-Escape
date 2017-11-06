#ifndef ITEM_H
#define ITEM_H
#include "stdafx.h"

/**
* @brief		Item :: 네트워크 오브젝트
* @details		서버 내에 뿌려져있는 아이템 클래스
* @author		이훈 (MoonAuSosiGi@gmail.com)
* @date			2017-11-04
* @file			Iem.h
* @version		0.0.1
*/
class Item
{
private:
	string m_itemID;	///< 아이템 아이디
	string m_networkID;	///< 아이템의 네트워크 아이디
	Vector3 m_pos;		///< 아이템의 네트워크 좌표
	Vector3 m_rot;		///< 아이템의 네트워크 회전값
public:

	Item(string itemID,string networkID); ///< 생성자

#pragma region Get / Set Method ==================================================
	string GetItemID(); ///< 아이템 아이디 얻기
	string GetNetworkID(); ///< 아이템의 네트워크 아이디 얻기
	void SetPosition(Vector3 pos); ///< 아이템 포지션 세팅 (벡터)
	void SetPosition(float x, float y, float z); ///< 아이템 포지션 세팅
	Vector3 GetPosition(); ///< 아이템 좌표 얻기
	void SetRotation(Vector3 rot); ///< 아이템 회전값 세팅 (벡터)
	void SetRotation(float rotx, float roty, float rotz); ///< 아이템 회전값 세팅
	Vector3 GetRotation(); ///< 아이템 회전값 얻기
#pragma endregion ================================================================

};

#endif