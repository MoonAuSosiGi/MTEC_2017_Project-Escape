// Server.cpp : �ܼ� ���� ���α׷��� ���� �������� �����մϴ�.
//

#include "stdafx.h"
#include "Server.h"

int main()
{
	Server server;

	server.ServerRun();
    return 0;
}


void Server::ServerRun()
{
	m_netServer = shared_ptr<CNetServer>(CNetServer::Create());

	// -- ���� �Ķ���� ���� ( �������� / ��Ʈ ���� ) ---------------------------//
	CStartServerParameter pl;
	pl.m_allowServerAsP2PGroupMember = true;
	pl.m_protocolVersion = g_protocolVersion;
	pl.m_tcpPorts.Add(g_serverPort);

	// -- Stub , Proxy Attach ���� ������ ���� �� �ִ� --------------------------//
	m_netServer->AttachStub(this);
	m_netServer->AttachProxy(&m_proxy);

	// -- ���� ���ٽ� -----------------------------------------------------------//

	//-- ���� ���� --------------------------------------------------------------//
	try
	{
		m_netServer->Start(pl);
		m_playerP2PGroup = m_netServer->CreateP2PGroup();
		m_netServer->JoinP2PGroup(HostID_Server, m_playerP2PGroup);

	}
	catch (Exception &e)
	{
		cout << "Server Start Failed : " << e.what() << endl;
		return;
	}
	// Ŭ���̾�Ʈ�� ���Դ� 
	m_netServer->OnClientJoin = [this](CNetClientInfo *clientInfo) { OnClientJoin(clientInfo); };

	// Ŭ���̾�Ʈ�� ������
	m_netServer->OnClientLeave = [this](CNetClientInfo *clientInfo, ErrorInfo* errinfo, const ByteArray& comment) {
		OnClientLeave(clientInfo, errinfo, comment);
	};

	// ���� ����
	m_netServer->OnError = [](ErrorInfo *errorInfo) {
		cout << "type : " << errorInfo->m_errorType << endl;
		printf("OnError : %s\n", StringT2A(errorInfo->ToString()).GetString());
	};

	// ���� ���
	m_netServer->OnWarning = [](ErrorInfo *errorInfo) {
		printf("OnWarning : %s\n", StringT2A(errorInfo->ToString()).GetString());
	};

	//
	m_netServer->OnInformation = [](ErrorInfo *errorInfo) {
		cout << "type : " << errorInfo->m_errorType << endl;
		printf("OnInformation : %s\n", StringT2A(errorInfo->ToString()).GetString());
	};
	m_netServer->OnException = [](const Exception &e) {
		printf("OnInformation : %s\n", e.what());
	};

	m_netServer->OnP2PGroupJoinMemberAckComplete = [](HostID groupHostID, HostID memberHostID, ErrorType result)
	{
		if (result == ErrorType_Ok)
		{
			// ���������� �߰��Ǿ���
			cout << "Server : P2P Member JoinAckComplete " << memberHostID << endl;
		}
		else
		{
			cout << "Server : P2P Member JoinAckComplete failed " << endl;
		}
	};

	string line;
	getline(std::cin, line);
}

void Server::OnClientJoin(CNetClientInfo* clientInfo)
{
	cout << "OnClientJoin " << clientInfo->m_HostID << endl;

	m_netServer->JoinP2PGroup(clientInfo->m_HostID, m_playerP2PGroup);
}

void Server::OnClientLeave(CNetClientInfo* clientInfo, ErrorInfo* errorInfo, const ByteArray& comment)
{
	cout << "OnClientLeave " << clientInfo->m_HostID << endl;

	auto iter = m_clientMap.find((clientInfo->m_HostID));
	m_netServer->LeaveP2PGroup(clientInfo->m_HostID, m_playerP2PGroup);

	if (iter == m_clientMap.end())
	{
		cout << "�α������� ���� Ŭ���̾�Ʈ�� �������ϴ�. " << clientInfo->m_HostID << endl;

		return;
	}

	m_proxy.NotifyPlayerLost(m_playerP2PGroup,RmiContext::ReliableSend,clientInfo->m_HostID);
	
	m_clientMap.erase(iter);

}

#pragma region C2S Method
DEFRMI_SpaceWar_RequestServerConnect(Server)
{
	cout << "C2S-- RequestServerConnect " << id << " remote " << remote << endl;

	// �ߺ� �������� üũ
	auto iter = m_clientMap.begin();
	while (iter != m_clientMap.end())
	{
		if (iter->second->m_userName == id)
		{
			// �ߺ�
			m_proxy.NotifyLoginFailed(remote, RmiContext::ReliableSend,
				"�ߺ� �����Դϴ�.");

		}
		iter++;
	}


	{
		// �߰�����
		CriticalSectionLock lock(m_critSec, true);
		auto newClient = make_shared<Client>();
		newClient->m_hostID = remote;
		newClient->m_userName = id;
		m_clientMap[remote] = newClient;
	}
	cout << "Server : LoginSuccess" << endl;
	m_proxy.NotifyLoginSuccess(remote, RmiContext::ReliableSend,(int)remote);
	return true;
}

DEFRMI_SpaceWar_RequestClientJoin(Server)
{
	cout << "Server : RequestClientJoin "<<name << endl;
	
	auto iter = m_clientMap.find((HostID)hostID);
	if (iter == m_clientMap.end())
	{
		cout << "�߸��� Ŭ���̾�Ʈ " << endl;
		return true;
	}
	// ��ο��� ��Ÿ���� �ؾ��� 
	for each (auto c in m_clientMap)
	{
		if (c.first != iter->first)
		{
			auto otherClient = c.second;
			m_proxy.NotifyOtherClientJoin(c.first, 
				RmiContext::ReliableSend,
				iter->second->m_hostID,
				iter->second->m_userName,
				iter->second->x,
				iter->second->y,
				iter->second->z);

			m_proxy.NotifyOtherClientJoin(iter->first,
				RmiContext::ReliableSend,
				c.second->m_hostID,
				c.second->m_userName,
				c.second->x,
				c.second->y,
				c.second->z);
		}
	}

	// ������ ���� ����
	for each(auto item in m_itemMap)
	{
		m_proxy.NotifyCreateItem((HostID)hostID, RmiContext::ReliableSend, (int)HostID_Server,
			item.second->m_itemCID, item.second->m_itemID, item.second->pos, item.second->rot);
	}
	
	return true;
}

DEFRMI_SpaceWar_RequestWorldCreateItem(Server)
{
	cout << "RequestWorldCreateItem  item CID : " << itemCID << " x : "<< pos.x << " y : " << pos.y << " z : "<<pos.z  << endl;
	auto iter = m_clientMap.find((HostID)hostID);

	{
		// �߰�����
		CriticalSectionLock lock(m_critSec, true);
		auto newItem = make_shared<Item>();
		newItem->m_itemCID = itemCID;
		newItem->m_itemID = itemID;
		newItem->pos.x = pos.x;
		newItem->pos.y = pos.y;
		newItem->pos.z = pos.z;
		newItem->rot.x = rot.x;
		newItem->rot.y = rot.y;
		newItem->rot.z = rot.z;
		m_itemMap[itemID] = newItem;
	}

	if (iter == m_clientMap.end())
	{
		cout << "�߸��� ��û" << endl;
	}

	// ���� ��� �ܿ� ���� ��û ������
	for each(auto c in m_clientMap)
	{
		if (c.first != iter->first)
		{
			m_proxy.NotifyCreateItem(c.first, RmiContext::ReliableSend, 
				(int)(iter->first), itemCID,itemID, pos, rot);
		}
	}
	return true;
}

DEFRMI_SpaceWar_NotifyDeleteItem(Server)
{
	cout << "NotifyDeleteItem " << itemID << endl;

	auto iter = m_itemMap.find(itemID);
	
	if (iter == m_itemMap.end())
	{
		cout << "�߸��� ��û" << endl;
	}
	{
		// �߰�����
		CriticalSectionLock lock(m_critSec, true);
		m_itemMap.erase(iter);
	}
	return true;
}

DEFRMI_SpaceWar_NotifyPlayerMove(Server)
{
	m_clientMap[(HostID)hostID]->x = curX;
	m_clientMap[(HostID)hostID]->y = curY;
	m_clientMap[(HostID)hostID]->z = curZ;
	return true;
}

DEFRMI_SpaceWar_NotifyPlayerEquipItem(Server)
{
	cout << "NotifyPlayerEquipItem " << hostID << " item " << itemID << endl;
	return true;
}

DEFRMI_SpaceWar_RequestPlayerDamage(Server)
{
	cout << "Request Player Damage ������ " << sendHostID << " ������ " << targetHostID << " ������ " << damage << endl;
	
	float prevHp = m_clientMap[(HostID)targetHostID]->hp;
	m_clientMap[(HostID)targetHostID]->hp -= damage;

	m_proxy.NotifyPlayerChangeHP(m_playerP2PGroup, RmiContext::ReliableSend, targetHostID, m_clientMap[(HostID)targetHostID]->m_userName, m_clientMap[(HostID)targetHostID]->hp, prevHp, MAX_HP);
	return true;
}

DEFRMI_SpaceWar_RequestPlayerUseOxy(Server)
{
	cout << "RequestPlayerUseOxy " << useOxy << endl;
	
	float prevOxy =	m_clientMap[(HostID)sendHostID]->oxy;
	m_clientMap[(HostID)sendHostID]->oxy -= useOxy;

	m_proxy.NotifyPlayerChangeOxygen(m_playerP2PGroup, RmiContext::ReliableSend, sendHostID, m_clientMap[(HostID)sendHostID]->m_userName, m_clientMap[(HostID)sendHostID]->oxy, prevOxy, MAX_OXY);

	return true;
}

DEFRMI_SpaceWar_RequestUseOxyCharger(Server)
{
	cout << "RequestUseOxyCharger " << endl;

	CriticalSectionLock lock(m_critSec, true);
	
	float prevOxy = m_clientMap[(HostID)sendHostID]->oxy;
	m_clientMap[(HostID)sendHostID]->oxy += userOxy;

	m_proxy.NotifyPlayerChangeOxygen(m_playerP2PGroup, RmiContext::ReliableSend, sendHostID, m_clientMap[(HostID)sendHostID]->m_userName, m_clientMap[(HostID)sendHostID]->oxy, prevOxy, MAX_OXY);

	m_proxy.NotifyUseOxyCharger(m_playerP2PGroup, RmiContext::ReliableSend, sendHostID,
		oxyChargerIndex, userOxy);
	return true;
}

DEFRMI_SpaceWar_RequestUseItemBox(Server)
{
	cout << "RequestUseItemBox " << itemBoxIndex << endl;

	// TESTCODE
	if (m_itemBoxMap.find(itemBoxIndex) == m_itemBoxMap.end())
	{
		
		// ù ���
		m_itemBoxMap[itemBoxIndex] = sendHostID;

		int itemCode = 0; // ���⼭ �����
		m_proxy.NotifyUseItemBox(m_playerP2PGroup, RmiContext::ReliableSend,
			sendHostID, itemBoxIndex, itemCode);
		return true;
	}

	// �̹� �����
	
	

	return true;
}

#pragma endregion




