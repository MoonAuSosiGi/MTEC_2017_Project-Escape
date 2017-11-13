#ifndef OXYCHARGER_H
#define OXYCHARGER_H

/**
* @brief		OxyCharger :: ��Ʈ��ũ ������Ʈ
* @details		��� ������ / ��� ó�� ��
* @author		���� (MoonAuSosiGi@gmail.com)
* @date			2017-11-04
* @file			OxyCharger.h
* @version		0.0.1
*/
class OxyCharger
{
private:
	int m_oxyChargerID; ///< ��� ������ ��Ʈ��ũ �ĺ� ���̵�
	bool m_isLocked;	///< ��� ����
	int m_useHostID;	///< ������� �÷��̾��� �ĺ� ���̵�
public:
	OxyCharger(); ///< �⺻ ������

#pragma region Get / Set Method =========================================================
	void SetOxyChargerID(int oxyChargerID); ///< ��� ������ �ĺ� ���̵� ����
	int GetOxyChargerID(); ///< ��� ������ �ĺ� ���̵� ���
	bool IsLocked();	///< ��� ���� ���
	int GetUseHostID();	///< ��� ���� ������ HostID ���
#pragma endregion =======================================================================

	
	void LockOxyCharger(int hostID) { m_isLocked = true; m_useHostID = hostID; } ///< ��� ������ ���
	void UnLockOxyCharger() { m_isLocked = false; m_useHostID = -1; } ///< ��� ������ ��� ����
private:


};

#endif