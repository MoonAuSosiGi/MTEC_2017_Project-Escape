#ifndef SERVER_H
#define SERVER_H

#include "Client.h"
#include "Item.h"
#include "Shelter.h"
#include "GameRoom.h"
#include "OxyCharger.h"
#include "SpaceShip.h"
#include "SP_Marshaler.h"
#include "../Common/SpaceWar_stub.h"
#include "../Common/SpaceWar_proxy.h"

#include "../Common/Common.h"
#include "../Common/Common.cpp"
#include "../Common/SpaceWar_stub.cpp"
#include "../Common/SpaceWar_proxy.cpp"
#include "../Common/SpaceWar_common.cpp"

// ���ּ� ��� �������� ���� �ð�
int s_spaceShipLockTime = 60;

// ���׿����� �����ð�
int s_meteorCommingSec = 90;
// ������ ���� ���� �ð�
int s_deathZoneCommingSec = 30;

// �ٲ�� ���׿� ����
int m_meteorCommingTime[10] = { 30 ,50,70,90,110,130,150,170,190,210 };

// �������� �����ϰ� �ִ� �ε���
int s_deathZoneIndex = 0;

// �������� �����̴� ��ü
int s_deathZoneHostID = -1;

//Meteor ID 
int s_meteorID = 0;

// Death Zone ID
int s_deathZoneID = 0;

class Server : public SpaceWar::Stub
{
public:
	Server() 
	{ 
		m_gameRoom = make_shared <GameRoom>(); 
	}
	~Server() {}

public:
	// P2P �׷� �̸� --------------------------
	HostID m_playerP2PGroup = HostID_None; 
public:

#pragma region Server Class Logic -------------------------------------------------------------------
	// ���� ���� ����
	void ServerRun();
	// ���� �̺�Ʈ ����
	// Ŭ�� ���ӽ�
	void OnClientJoin(CNetClientInfo* clientInfo);
	// Ŭ�� ���� ������
	void OnClientLeave(CNetClientInfo* clientInfo, ErrorInfo* errorInfo, const ByteArray& comment);

	// ���� ����
	void ServerReset();
#pragma endregion

#pragma region C2S  ---------------------------------------------------------------------------------
#pragma region �ʱ� ���� ========================
#pragma region ���� ���� ------------------------

	// Connect �ʱ� ���� ��û
	DECRMI_SpaceWar_RequestServerConnect;

	// �� ���� �ٲ�
	DECRMI_SpaceWar_RequestNetworkGameTeamSelect;

	//�������� ���� ��ƾ // ���� �������� �˸� ( ȣ��Ʈ�� ������ ))
	DECRMI_SpaceWar_RequestGameExit;

#pragma endregion

#pragma region ȣ��Ʈ �ƴ� ���� -----------------
	// ����
	DECRMI_SpaceWar_RequestNetworkGameReady;
	// ���� �κ� ���� // �κ� ȭ�鿡 ���Դ�.
	DECRMI_SpaceWar_RequestLobbyConnect;

	// ������ ���۵ǰ� ���� ������ �Ѿ���� �� �ٵ� ��û�Ѵ�.
	DECRMI_SpaceWar_RequestGameSceneJoin;
#pragma endregion

#pragma region ���� ȣ��Ʈ ���� -----------------
	// ������ ���� �ٲ�
	DECRMI_SpaceWar_RequestNetworkChangeMap;
	// ������ �÷��̾� ���� �ٲ�
	DECRMI_SpaceWar_RequestNetworkPlayerCount;
	// ������ ���� ��带 �ٲ�
	DECRMI_SpaceWar_RequestNetworkGameModeChange;
	// ������ ���� ������ ����
	DECRMI_SpaceWar_RequestNetworkGameStart;
	// ������ �κ� ������.
	DECRMI_SpaceWar_RequestNetworkHostOut;
#pragma endregion
#pragma endregion

#pragma region �ΰ��� ===========================

#pragma region �ΰ��� :: �÷��̾� �޽��� --------
#pragma region [�÷��̾� ���� ���� ����]
	// ü�� ȸ�� �޽���
	DECRMI_SpaceWar_RequestHpUpdate;
	// �÷��̾ �¾Ҵ�.
	DECRMI_SpaceWar_RequestPlayerDamage;
	// �÷��̾ ���� ������.
	DECRMI_SpaceWar_RequestPlayerUseOxy;
	// �÷��̾� ������ ó�� 
	DECRMI_SpaceWar_NotifyPlayerMove;
	//������ ��� // ���� ���� �� ����
	DECRMI_SpaceWar_NotifyPlayerEquipItem;
#pragma endregion
#pragma endregion

#pragma region �ΰ��� :: ��� ������ ------------
	// ó���� ��� �����⸦ ����Ѵ�
	DECRMI_SpaceWar_RequestOxyChargerStartSetup;
	// ��� ������ ���� ��û 
	DECRMI_SpaceWar_RequestUseOxyChargerStart;
	// ��� ������ ������
	DECRMI_SpaceWar_RequestUseOxyCharger;
	// ��� ������ ���� ��
	DECRMI_SpaceWar_RequestUseOxyChargerEnd;
#pragma endregion

#pragma region �ΰ��� :: ������ �ڽ� ���� -------
	// ������ �ڽ� ����
	DECRMI_SpaceWar_RequestUseItemBox;
#pragma endregion

#pragma region �ΰ��� :: ���� ���� --------------
	// ���� ���
	DECRMI_SpaceWar_RequestShelterStartSetup;
	// ���� �� ����
	DECRMI_SpaceWar_RequestShelterDoorControl;

	// ���� ���� ����
	DECRMI_SpaceWar_RequestShelterEnter;
#pragma endregion

#pragma region �ΰ��� :: ������ ���� ------------
	// ���忡 ������ ���� ��û
	DECRMI_SpaceWar_RequestWorldCreateItem;
	// ������ ���� ��û
	DECRMI_SpaceWar_RequestItemDelete;
	// ���� ������ ���� �˸�
	DECRMI_SpaceWar_NotifyDeleteItem;
#pragma endregion

#pragma region �ΰ��� :: ���ּ� -----------------
	// ���ּ� � �ִ��� ����
	DECRMI_SpaceWar_RequestSpaceShipSetup;
	// ���ּ� ���� 
	DECRMI_SpaceWar_RequestSpaceShip;
	// ���ּ� ��� ��û
	DECRMI_SpaceWar_RequestUseSpaceShip;
	// ���ּ� ��� ���
	DECRMI_SpaceWar_RequestUseSpaceShipCancel;
#pragma endregion

#pragma region �ΰ��� :: ������ -----------------
	// �������� �̵��ϰ� �ִ� �ܰ踦 �뺸�����
	DECRMI_SpaceWar_RequestDeathZoneMoveIndex;
#pragma endregion

#pragma endregion

#pragma region ���â ==========================
	// ��ο� ���ӽ� ���� ��û
	DECRMI_SpaceWar_RequestDrawGameResult;

	// ����� �޾ƿ��� ��
	DECRMI_SpaceWar_RequestGameEnd;
#pragma endregion
#pragma endregion

	//DECRMI_SpaceWar_RequestClientJoin;

private:
	// ������ �ڽ� �� ��ȣ�ۿ� ������Ʈ�� ��� ���������� ���ƾ� �ϹǷ�
	// ���� �����ϰ� �ִ��� üũ�ϴ� ������ �ʿ���, �̿� ���� üũ�� ������
	unordered_map<int, int> m_itemBoxMap;

	// ������ ��쿣 ����ؾ���
	unordered_map<int, shared_ptr<Shelter>> m_shelterMap;

	// �÷��� Ÿ��
	int m_gameStartTime;

	// �����۹ڽ� �ε���
	int m_itemBoxCreateItemIndex;
public:
	// ���� ���Ͻ�
	SpaceWar::Proxy m_proxy;
	// ���� ��ü
	shared_ptr<CNetServer> m_netServer;
	// ������ lock �� ���� 
	CriticalSection m_critSec;

	// ��������� ����Ʈ
	unordered_map<int, shared_ptr<OxyCharger>> m_oxyChargerMap;

	// ���ּ� ����Ʈ
	unordered_map<int, shared_ptr<SpaceShip>> m_spaceShipMap;

	// Ŭ���̾�Ʈ ����Ʈ
	unordered_map<HostID, shared_ptr<Client>> m_clientMap;
	
	// ������ ����Ʈ
	unordered_map<string, shared_ptr<Item>> m_itemMap;

	// ���� ��
	shared_ptr<GameRoom> m_gameRoom;

	// ���� ���� ����
	void ResetUsers();
	
	//���ּ� �� ī��Ʈ ���
	int GetSpaceShipCount()
	{
		return m_spaceShipMap.size();
	}
};

// �����Լ�
float RandomRange(float min, float max)
{
	return ((float(rand()) / float(RAND_MAX)) * (max - min)) + min;
}
#endif