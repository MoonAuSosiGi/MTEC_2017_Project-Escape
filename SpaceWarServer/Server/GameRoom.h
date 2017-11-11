#ifndef GameRoom_H
#define GameRoom_H
#include "RoomClient.h"

/**
 * @brief		GameRoom :: ������ ����Ǵ� �� ����
 * @details		����, ���ӹ� ���� Ŭ���� / ���� ���� / ���� ��� / �÷��̾� �� ���� / �� ����  � ���� ó���� �Ѵ�.
 * @author		���� (MoonAuSosiGi@gmail.com)
 * @date		2017-11-04
 * @file		GameRoom.h
 * @version		0.0.1
*/
class GameRoom
{
private:

	int m_limitPlayerCount;		///< �÷��̾� �� ���� (�⺻�� 10)
	int m_currentPlayerCount;	///< ���� ������ �÷��̾� ��
	int m_gameMode;				///< ���� ���� ���
	bool m_teamMode;			///< ���� ������ �������� ����������
	bool m_isGameRunning;		///< ���� ����������
	unordered_map<HostID,shared_ptr<RoomClient>> m_clientMap; ///< ������ Ŭ���̾�Ʈ ��
public:

	GameRoom();					///< �⺻ ������, �ʱ갪 ����
	unordered_map<HostID, shared_ptr<RoomClient>> GetClientMap(); ///< Ŭ���̾�Ʈ �� ���

#pragma region Room Setting Method =====================================================================================
	void SetPlayerLimitCount(int count);	///< �÷��̾� �� ���ѵα�
	int GetPlayerLimitCount();				///< �÷��� ������ �÷��̾� �� ���
	void SetGameMode(int gameMode);			///< ���� ��� ����
	void SetTeamMode(bool teamMode);		///< �� ��� ����
	bool GetTeamMode();						///< �� ��� ���
	int GetGameMode();						///< ���� ��� ���
	void ClearRoom();						///< ���� �� Ŭ����
#pragma endregion ======================================================================================================

#pragma region Client Method ===========================================================================================
	bool NewClientConnect(HostID hostID, string userName, CriticalSection& critSection); ///< �� Ŭ���̾�Ʈ ����
	void LogOutClient(HostID hostID); ///< Ŭ���̾�Ʈ�� ������ ������.
	bool IsRoomClient(HostID hostID); ///< �濡 ����� �����ߴ��� üũ
	void SetReady(HostID hostID, bool ready); ///< Ŭ���̾�Ʈ ���� ���� ����
	void TeamChange(HostID hostID, bool red); ///< Ŭ���̾�Ʈ �� ���� ���� ����
#pragma endregion ======================================================================================================

#pragma region Game Method =============================================================================================
	bool GameStartCheck(); ///< ���� ��ŸƮ�� �������� üũ 
	bool IsGameSceneAllReady(); ///< ���Ӿ��� ���ͼ� ���� ������ �������� üũ
	bool IsGameRunning(); ///< ���� ���� �������� ����
	void SetGameRunning(bool running); ///< ���� ���� �������� ����

	forward_list<HostID> GetOtherClients(HostID hostID); ///< Ư�� Ŭ���̾�Ʈ ������ HostID ����Ʈ ���
	forward_list<RoomClient> GetOtherClientInfos(HostID hostID); ///< Ư�� Ŭ���̾�Ʈ ������ ����Ʈ ���
	forward_list<HostID> GetAllClient(); ///< �������ִ� ��� Ŭ���̾�Ʈ�� HostID ����Ʈ ���
	shared_ptr<RoomClient> GetClient(HostID hostID); ///< Ư�� Ŭ���̾�Ʈ�� ���� ���

#pragma endregion ======================================================================================================

};

#endif
