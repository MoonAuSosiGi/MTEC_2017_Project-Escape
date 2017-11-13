#ifndef ROOM_CLIENT_H
#define ROOM_CLIENT_H
#include "stdafx.h"

/**
* @brief		RoomClient :: ���� , ���ӹ濡 �����ִ� Ŭ���̾�Ʈ
* @details		���� / ���� �� ��� Ŭ���̾�Ʈ�� ������ ����ִ�.
* @author		���� (MoonAuSosiGi@gmail.com)
* @date			2017-11-04
* @file			RoomClient.h
* @version		0.0.1
*/
class RoomClient
{
private:

	HostID m_hostID;	///< Ŭ���̾�Ʈ�� HostID
	string m_userName;	///< Ŭ���̾�Ʈ�� ���� �̸�

	bool m_redTeam;		///< ���� ������ (������ ��쿡�� �ش�)
	bool m_isReady;		///< ���� �������� ( ����, ������ �ƴҶ� )
	bool m_isHost;		///< �������� �ƴ���
	bool m_isGameScene;	///< ���� ���Ӿ� ������

	Vector3 m_pos;		///< ���� ��ġ
	float m_hp;			///< ���� Ŭ���̾�Ʈ�� ü��		
	float m_oxy;		///< ���� Ŭ���̾�Ʈ�� ��ҷ�

	int m_state;		///< ����ִ��� �׾��ִ��� ���ּ����� � ���� PlayerState
	int m_deathCount;	///< ���� Ƚ�� 
	int m_killCount;	///< ���� Ƚ��
	int m_assistCount;	///< ��ý�Ʈ Ƚ��

	unordered_map<int, float> m_assistCheck; ///< ��ý�Ʈ ���� üũ�� ��
	
public:

	RoomClient(HostID hostID, string userName, bool host = false); ///< ������, HostID / ���� �̸� / host ����

#pragma region Get / Set Method Waiting Room ========================================================================
	void SetTeamColor(bool red);	///< �� �÷� ���� 
	void SetReady(bool ready);		///< ���� ���� ����
	void SetHost(bool host);		///< ���� ���� ����
	void SetGameScene(bool gameScene); ///< ���� ���Ӿ������� ���� ���� ����
	void SetAssistCount(int assist); ///< ��ý�Ʈ ���� ����
	void SetKillCount(int killCount);	///< ų ī��Ʈ ����
	void SetDeathCount(int death); ///< ���� ī��Ʈ ����
	int GetDeathCount(); ///< ���� ī��Ʈ ���
	int GetKillCount(); ///< ų ī��Ʈ ���
	int GetAssistCount(); ///< ��ý�Ʈ ���� ���
	bool IsHost(); ///< �� Ŭ���̾�Ʈ�� ��������
	bool IsReady(); ///< ���� ������ 
	bool IsRedTeam(); ///< ���������� 
	bool IsGameScene(); ///< ���Ӿ����� �ƴ���

	HostID GetHostID(); ///< Host ID ���
	string GetName(); ///< ���� �̸� ��� 
#pragma endregion ===================================================================================================

#pragma region Get / Set Method InGame ==============================================================================
	void SetPosition(Vector3 pos);	///< ���� ������ ����
	void SetPosition(float x, float y, float z); ///< ���� ������ ����
	Vector3 GetPosition(); ///< ���� ������ ���

	void SetHp(float newHp); ///< Hp �� ����
	void HpUpdate(float val); ///< ������ hp
	float GetHp(); ///< hp ����

	void SetOxy(float newOxy); ///< Oxy �� ����
	void OxyUpdate(float val); ///< ������ oxy
	float GetOxy(); ///< Oxy ����

	void SetState(int state); ///< State ����
	int GetState();	///< State ����
#pragma endregion ===================================================================================================
	
#pragma region InGame Method ========================================================================================
	void GameReset(); ///< ���� ���½� ȣ��
	void DamageClient(int hostID, float time); ///< ��ý�Ʈ ����� ���� ���� �༮ ���
	void PlayerDead(float deadTime); ///< �÷��̾ �׾��� ���� ó��
	void PlayerWin(); ///< �� �÷��̾ �¸����� ��쿡 ���� ó��
	forward_list<int> GetAssistClientList(); ///< �� Ŭ���̾�Ʈ�� �׾�����, ��ý�Ʈ�� Ŭ���̾�Ʈ ����Ʈ
#pragma endregion
};

#endif