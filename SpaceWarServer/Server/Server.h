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
int s_meteorCommingSec = 11;


class Server : public SpaceWar::Stub
{
public:
	Server() { m_gameRoom = make_shared <GameRoom>(); }
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

	// ���� �̺�Ʈ ����
	void OnClientJoin(CNetClientInfo* clientInfo);
	void OnClientLeave(CNetClientInfo* clientInfo, ErrorInfo* errorInfo, const ByteArray& comment);

	// ���׿� ����
//	void MeteorLoop(void*);

private:
	// ������ �ڽ� �� ��ȣ�ۿ� ������Ʈ�� ��� ���������� ���ƾ� �ϹǷ�
	// ���� �����ϰ� �ִ��� üũ�ϴ� ������ �ʿ���, �̿� ���� üũ�� ������
	unordered_map<int, int> m_itemBoxMap;
	unordered_map<int, int> m_oxyChargerMap;

	// ������ ��쿣 ����ؾ���
	unordered_map<int, shared_ptr<Shelter>> m_shelterMap;

	// �÷��� Ÿ��
	int m_gameStartTime;
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
	unordered_map<int, shared_ptr<Item>> m_itemMap;

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