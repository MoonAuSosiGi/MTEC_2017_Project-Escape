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
	// P2P 그룹 이름 --------------------------
	HostID m_playerP2PGroup = HostID_None; 
public:
	// 기본 서버 로직 -------------------------
	void ServerRun();

	DECRMI_SpaceWar_RequestServerConnect;
	DECRMI_SpaceWar_RequestClientJoin;
	DECRMI_SpaceWar_RequestWorldCreateItem;
	DECRMI_SpaceWar_NotifyDeleteItem;
	DECRMI_SpaceWar_NotifyPlayerMove;

	//아이템 장비
	DECRMI_SpaceWar_NotifyPlayerEquipItem;
	

	// 서버 이벤트 로직
	void OnClientJoin(CNetClientInfo* clientInfo);
	void OnClientLeave(CNetClientInfo* clientInfo, ErrorInfo* errorInfo, const ByteArray& comment);

public:
	// 전송 프록시
	SpaceWar::Proxy m_proxy;
	// 서버 객체
	shared_ptr<CNetServer> m_netServer;
	// 스레드 lock 을 위함 
	CriticalSection m_critSec;

	// 클라이언트 리스트
	unordered_map<HostID, shared_ptr<Client>> m_clientMap;
	
	// 아이템 리스트
	unordered_map<int, shared_ptr<Item>> m_itemMap;
};


#endif