#ifndef ITEM_H
#define ITEM_H
#include "stdafx.h"

/**
* @brief		Item :: ��Ʈ��ũ ������Ʈ
* @details		���� ���� �ѷ����ִ� ������ Ŭ����
* @author		���� (MoonAuSosiGi@gmail.com)
* @date			2017-11-04
* @file			Iem.h
* @version		0.0.1
*/
class Item
{
private:
	string m_itemID;	///< ������ ���̵�
	string m_networkID;	///< �������� ��Ʈ��ũ ���̵�
	Vector3 m_pos;		///< �������� ��Ʈ��ũ ��ǥ
	Vector3 m_rot;		///< �������� ��Ʈ��ũ ȸ����
public:

	Item(string itemID,string networkID); ///< ������

#pragma region Get / Set Method ==================================================
	string GetItemID(); ///< ������ ���̵� ���
	string GetNetworkID(); ///< �������� ��Ʈ��ũ ���̵� ���
	void SetPosition(Vector3 pos); ///< ������ ������ ���� (����)
	void SetPosition(float x, float y, float z); ///< ������ ������ ����
	Vector3 GetPosition(); ///< ������ ��ǥ ���
	void SetRotation(Vector3 rot); ///< ������ ȸ���� ���� (����)
	void SetRotation(float rotx, float roty, float rotz); ///< ������ ȸ���� ����
	Vector3 GetRotation(); ///< ������ ȸ���� ���
#pragma endregion ================================================================

};

#endif