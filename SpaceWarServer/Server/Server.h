#ifndef SERVER_H
#define SERVER_H

#include "Client.h"
#include "Item.h"
#include "Shelter.h"
#include "GameRoom.h"
#include "SP_Marshaler.h"
#include "../Common/SpaceWar_stub.h"
#include "../Common/SpaceWar_proxy.h"

#include "../Common/Common.h"
#include "../Common/Common.cpp"
#include "../Common/SpaceWar_stub.cpp"
#include "../Common/SpaceWar_proxy.cpp"
#include "../Common/SpaceWar_common.cpp"

// ���׿����� �����ð�
int s_meteorCommingSec = 90;
// ������ ���� ���� �ð�
int s_deathZoneCommingSec = 180;

// �ٲ�� ���׿� ����
int m_meteorCommingTime[10] = { 30,32,34,36,38,40,42,44,46,48 };

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
		m_spaceshipCount = 0;
	}
	~Server() {}

public:
	// P2P �׷� �̸� --------------------------
	HostID m_playerP2PGroup = HostID_None; 
public:
	// �⺻ ���� ���� -------------------------
	void ServerRun();

	DECRMI_SpaceWar_RequestServerConnect;
	DECRMI_SpaceWar_RequestClientJoin;
	DECRMI_SpaceWar_RequestWorldCreateItem;
	DECRMI_SpaceWar_NotifyDeleteItem;
	DECRMI_SpaceWar_NotifyPlayerMove;

	// ������ ���� ��û
	DECRMI_SpaceWar_RequestItemDelete;
	//������ ���
	DECRMI_SpaceWar_NotifyPlayerEquipItem;

	// ������ ��û
	DECRMI_SpaceWar_RequestPlayerDamage;

	// ���� ������.
	DECRMI_SpaceWar_RequestPlayerUseOxy;

	// ��� ������ ���
	DECRMI_SpaceWar_RequestUseOxyCharger;

	// ������ �ڽ� ���
	DECRMI_SpaceWar_RequestUseItemBox;

	// ���� ���
	DECRMI_SpaceWar_RequestShelterStartSetup;
	// ���� �� ����
	DECRMI_SpaceWar_RequestShelterDoorControl;

	// ���� ���� ����
	DECRMI_SpaceWar_RequestShelterEnter;

	// ������ �� ü�� ȸ���߾�
	DECRMI_SpaceWar_RequestHpUpdate;
	
	// ��ο� ��� ��û
	DECRMI_SpaceWar_RequestDrawGameResult;
	// ���ּ� ���� 
	DECRMI_SpaceWar_RequestSpaceShip;
	//���� ��
	DECRMI_SpaceWar_RequestGameEnd;

	// ���� �κ� ����
	DECRMI_SpaceWar_RequestLobbyConnect;
	// �� ���� ��û
	DECRMI_SpaceWar_RequestNetworkGameTeamSelect;
	// ����
	DECRMI_SpaceWar_RequestNetworkGameReady;
	// �� �ٲ� ��û
	DECRMI_SpaceWar_RequestNetworkChangeMap;
	// ������ �ο� ���� ����
	DECRMI_SpaceWar_RequestNetworkPlayerCount;
	// ������ ���� ��带 �ٲ�
	DECRMI_SpaceWar_RequestNetworkGameModeChange;
	// ������ ���� ������ ����
	DECRMI_SpaceWar_RequestNetworkGameStart;
	// ������ �κ� ������.
	DECRMI_SpaceWar_RequestNetworkHostOut;

	// ���� ���� ���Դٴ� ���� �˷�����
	DECRMI_SpaceWar_RequestGameSceneJoin;

	// ���ּ� � �ִ��� ����
	DECRMI_SpaceWar_RequestSpaceShipSetup;

	// �������� �̵��ϰ� �ִ� �ܰ踦 �뺸�����
	DECRMI_SpaceWar_RequestDeathZoneMoveIndex;

	// ���� �������� �˸� ( ȣ��Ʈ�� ������ ))
	DECRMI_SpaceWar_RequestGameExit;

	// ���� �̺�Ʈ ����
	void OnClientJoin(CNetClientInfo* clientInfo);
	void OnClientLeave(CNetClientInfo* clientInfo, ErrorInfo* errorInfo, const ByteArray& comment);

	int GetSpaceShipCount() { return m_spaceshipCount; }

private:
	// ������ �ڽ� �� ��ȣ�ۿ� ������Ʈ�� ��� ���������� ���ƾ� �ϹǷ�
	// ���� �����ϰ� �ִ��� üũ�ϴ� ������ �ʿ���, �̿� ���� üũ�� ������
	unordered_map<int, int> m_itemBoxMap;
	unordered_map<int, int> m_oxyChargerMap;

	// ������ ��쿣 ����ؾ���
	unordered_map<int, shared_ptr<Shelter>> m_shelterMap;

	// �÷��� Ÿ��
	int m_gameStartTime;

	// ���� ������ ���ּ� ����
	int m_spaceshipCount;
	
	// �����۹ڽ� �ε���
	int m_itemBoxCreateItemIndex;
public:
	// ���� ���Ͻ�
	SpaceWar::Proxy m_proxy;
	// ���� ��ü
	shared_ptr<CNetServer> m_netServer;
	// ������ lock �� ���� 
	CriticalSection m_critSec;

	// Ŭ���̾�Ʈ ����Ʈ
	unordered_map<HostID, shared_ptr<Client>> m_clientMap;
	
	// ������ ����Ʈ
	unordered_map<string, shared_ptr<Item>> m_itemMap;

	// ���� ��
	shared_ptr<GameRoom> m_gameRoom;

	// ���� ���� ����
	void ResetUsers();
};

// �����Լ�
float RandomRange(float min, float max)
{
	return ((float(rand()) / float(RAND_MAX)) * (max - min)) + min;
}
#endif