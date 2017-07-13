#ifndef SERVER_H
#define SERVER_H

#include "Client.h"
#include "Item.h"
#include "SP_Marshaler.h"
#include "../Common/SpaceWar_stub.h"
#include "../Common/SpaceWar_proxy.h"

#include "../Common/Common.h"
#include "../Common/Common.cpp"
#include "../Common/SpaceWar_stub.cpp"
#include "../Common/SpaceWar_proxy.cpp"
#include "../Common/SpaceWar_common.cpp"


class Server : public SpaceWar::Stub
{
public:
	Server() {}
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

	// ���� �̺�Ʈ ����
	void OnClientJoin(CNetClientInfo* clientInfo);
	void OnClientLeave(CNetClientInfo* clientInfo, ErrorInfo* errorInfo, const ByteArray& comment);

private:
	// ������ �ڽ� �� ��ȣ�ۿ� ������Ʈ�� ��� ���������� ���ƾ� �ϹǷ�
	// ���� �����ϰ� �ִ��� üũ�ϴ� ������ �ʿ���, �̿� ���� üũ�� ������
	unordered_map<int, int> m_itemBoxMap;
	unordered_map<int, int> m_oxyChargerMap;

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
};


#endif