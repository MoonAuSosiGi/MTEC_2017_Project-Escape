#ifndef SERVER_H
#define SERVER_H

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

// �ٲ�� ���׿� ����
int s_meteorCommingTime[10] = { 30 ,50,70,90,110,130,150,170,190,210 };

/**
* @brief		Server :: ���� ���� Ŭ����
* @details		��������� ���� ��� ���� ���� ������ �����Ǿ� �ִ� Ŭ����
* @author		���� (MoonAuSosiGi@gmail.com)
* @date			2017-11-04
* @file			Server.h
* @version		0.0.1
*/
class Server : public SpaceWar::Stub
{
public:
	Server();

#pragma region C2S  --------------------------------------------------------------------------------------------------------------------
#pragma region �ʱ� ���� ========================
#pragma region ���� ���� ------------------------
	DECRMI_SpaceWar_RequestServerConnect; ///< Connect �ʱ� ���� ��û
	DECRMI_SpaceWar_RequestNetworkGameTeamSelect; ///< �� ���� ��û
	DECRMI_SpaceWar_RequestGameExit; ///< �������� ���� ��ƾ // ���� �������� �˸� ( ȣ��Ʈ�� ������ ))
#pragma endregion 

#pragma region ȣ��Ʈ �ƴ� ���� -----------------
	DECRMI_SpaceWar_RequestNetworkGameReady; ///< ���� ������ ���ƿ´�.
	DECRMI_SpaceWar_RequestLobbyConnect; ///< ���� �κ� ���� // �κ� ȭ�鿡 ���Դ�.
	DECRMI_SpaceWar_RequestGameSceneJoin; ///< ������ ���۵ǰ� ���� ������ �Ѿ���� �� �ٵ� ��û�Ѵ�.
#pragma endregion -------------------------------

#pragma region ���� ȣ��Ʈ ���� -----------------	
	DECRMI_SpaceWar_RequestNetworkChangeMap; ///< ������ ���� �ٲ�
	DECRMI_SpaceWar_RequestNetworkPlayerCount;///< ������ �÷��̾� ���� �ٲ�
	DECRMI_SpaceWar_RequestNetworkGameModeChange; ///< ������ ���� ��带 �ٲ�
	DECRMI_SpaceWar_RequestNetworkGameStart; ///< ������ ���� ������ ����
	DECRMI_SpaceWar_RequestNetworkHostOut; ///< ������ �κ� ������.
#pragma endregion -------------------------------
#pragma endregion 

#pragma region �ΰ��� ===========================

#pragma region �ΰ��� :: �÷��̾� �޽��� --------
#pragma region [�÷��̾� ���� ���� ����]
	DECRMI_SpaceWar_RequestHpUpdate; ///< ü�� ȸ�� �޽���
	DECRMI_SpaceWar_RequestPlayerDamage; ///< �÷��̾ �¾Ҵ�.
	DECRMI_SpaceWar_RequestPlayerUseOxy; ///< �÷��̾ ���� ������.
	DECRMI_SpaceWar_NotifyPlayerMove; ///< �÷��̾� ������ ó�� 
	DECRMI_SpaceWar_NotifyPlayerEquipItem; ///< ������ ��� // ���� ���� �� ����
#pragma endregion
#pragma endregion

#pragma region �ΰ��� :: ��� ������ ------------
	DECRMI_SpaceWar_RequestOxyChargerStartSetup; ///< ó���� ��� �����⸦ ����Ѵ�
	DECRMI_SpaceWar_RequestUseOxyChargerStart; ///< ��� ������ ���� ��û 
	DECRMI_SpaceWar_RequestUseOxyCharger; ///< ��� ������ ������
	DECRMI_SpaceWar_RequestUseOxyChargerEnd;///< ��� ������ ���� ��
#pragma endregion

#pragma region �ΰ��� :: ������ �ڽ� ���� -------
	// ������ �ڽ� ����
	DECRMI_SpaceWar_RequestUseItemBox;
#pragma endregion

#pragma region �ΰ��� :: ���� ���� --------------
	DECRMI_SpaceWar_RequestShelterStartSetup; ///< ���� ���
	DECRMI_SpaceWar_RequestShelterDoorControl; ///< ���� �� ����
	DECRMI_SpaceWar_RequestShelterEnter; ///< ���� ���� ����
#pragma endregion

#pragma region �ΰ��� :: ������ ���� ------------
	DECRMI_SpaceWar_RequestWorldCreateItem; ///< ���忡 ������ ���� ��û
	DECRMI_SpaceWar_RequestItemDelete; ///< ������ ���� ��û
	DECRMI_SpaceWar_NotifyDeleteItem; ///< ���� ������ ���� �˸�
#pragma endregion

#pragma region �ΰ��� :: ���ּ� -----------------
	DECRMI_SpaceWar_RequestSpaceShipSetup; ///< ���ּ� � �ִ��� ����
	DECRMI_SpaceWar_RequestSpaceShip; ///< ���ּ� ���� 
	DECRMI_SpaceWar_RequestUseSpaceShip; ///< ���ּ� ��� ��û
	DECRMI_SpaceWar_RequestUseSpaceShipCancel; ///< ���ּ� ��� ���
#pragma endregion

#pragma region �ΰ��� :: ������ -----------------
	DECRMI_SpaceWar_RequestDeathZoneMoveIndex; ///< �������� �̵��ϰ� �ִ� �ܰ踦 �뺸�����
#pragma endregion
#pragma endregion

#pragma region ���â ==========================
	DECRMI_SpaceWar_RequestDrawGameResult; ///< ��ο� ���ӽ� ���� ��û
	DECRMI_SpaceWar_RequestGameEnd; ///< ����� �޾ƿ��� ��
#pragma endregion
#pragma endregion

private:
	HostID m_playerP2PGroup = HostID_None; ///< P2P �׷��� HostID
	string m_serverPropertiesData; ///< server.properties json ������

	int m_gameStartTime; ///< �÷��� Ÿ�� ���� ����
	int m_itemBoxCreateItemIndex; ///< �����۹ڽ� �ε���

	SpaceWar::Proxy m_proxy; ///< ���� ���Ͻ�
	shared_ptr<CNetServer> m_netServer; ///< ���� ��ü
	CriticalSection m_critSec; ///< ������ lock �� ���� 

	unordered_map<int, int> m_itemBoxMap; ///< ������ �ڽ� ��
	unordered_map<int, shared_ptr<Shelter>> m_shelterMap; ///< ���� ��
	unordered_map<int, shared_ptr<OxyCharger>> m_oxyChargerMap; ///< ��� ������ ��
	unordered_map<int, shared_ptr<SpaceShip>> m_spaceShipMap; ///< ���ּ� ��

	unordered_map<string, shared_ptr<Item>> m_itemMap; ///< �༺�� �ѷ����ִ� ������
	shared_ptr<GameRoom> m_gameRoom; ///< ���ӷ�

#pragma region SpaceShip Lock / Meteor / DeathZone ====================================================================================
	int m_spaceShipLockTime = 0; ///< ���ּ� �ʱ� ��� �ð�
	int m_deathZoneCommingSec = 0; ///< ������ ���� ���� �ð�
	int m_deathZoneIndex = 0; ///< �������� �����ϰ� �ִ� �ε���
	int m_deathZoneHostID = -1; ///< �������� �����̴� ��ü
	int m_deathZoneID = 0; ///< Death Zone �� ��Ʈ��ũ ���̵�
	int m_meteorID = 0; ///< Meteor�� ��Ʈ��ũ ���̵�
#pragma endregion =====================================================================================================================

public:
#pragma region Server Class Logic -----------------------------------------------------------------------------------------------------
	void ServerFileLoad(); ///< ������ �������� �ε�
	bool ServerTableSetup(); ///< ���� �⺻ ���� ���̺� ����
	void ServerRun(); ///< ���� ���� ����
	void OnClientJoin(CNetClientInfo* clientInfo); ///< Ŭ���̾�Ʈ�� ���� ��û
	void OnClientLeave(CNetClientInfo* clientInfo, ErrorInfo* errorInfo, const ByteArray& comment); ///< Ŭ���̾�Ʈ�� �������� ������.
	void ServerReset(); ///< ���� ���� ������ �ʱ�ȭ �Ѵ�. 
#pragma region Get / Set Method --------------------------------------------------------------------------------------------------------
	shared_ptr<CNetServer> GetServer(); ///< ���� ��ü ���
	SpaceWar::Proxy* GetProxy(); ///< ���� ���Ͻ� ���
	HostID GetP2PID(); ///< P2P ���̵� ���
	shared_ptr<GameRoom> GetGameRoom(); ///< ���� �� ��ü ���
	void SetSpaceShipLockTime(int lockTime); ///< �ʱ� ���ּ� ��ݰ� ����
	int GetSpaceShipLockTime(); ///< �ʱ� ���ּ� ��ݰ� ���
	void SetDeathZoneCommingTime(int sec); ///< ���������� ���� �ð� ����
	int GetDeathZoneCommingTime(); ///< ���������� ���� �ð� ���
	void SetDeathZoneIndex(int curIndex); ///< �������� �����ϰ� �ִ� �ε��� ����
	int GetDeathZoneIndex(); ///< �������� �����ϰ� �ִ� �ε��� ���
	void SetDeathZoneHostID(int hostID); ///< �������� ���� �����̰� �ִ� Ŭ���̾�Ʈ ����
	int GetDeathZoneHostID(); ///< �������� ���� �����̰� �ִ� Ŭ���̾�Ʈ ���
	void SetDeathZoneID(int id); ///< �������� ��Ʈ��ũ ���̵� ����
	int GetDeathZoneID(); ///< ������ ��Ʈ��ũ ���̵� ��� 
	void SetMeteorID(int id); ///< ���׿��� ��Ʈ��ũ ���̵� ����
	int GetMeteorID(); ///< ���׿��� ��Ʈ��ũ ���̵� ���	
#pragma endregion ----------------------------------------------------------------------------------------------------------------------
#pragma endregion ----------------------------------------------------------------------------------------------------------------------


	
	
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

int RandomRange(int min, int max)
{
	return ((int(rand()) / int(RAND_MAX)) * (max - min)) + min;
}
#endif