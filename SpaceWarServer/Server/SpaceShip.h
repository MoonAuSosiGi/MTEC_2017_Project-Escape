#ifndef SPACESHIP_H
#define SPACESHIP_H

/**
* @brief		SpaceShip :: ��Ʈ��ũ ������Ʈ
* @details		���ּ� ��� ���� / ���� ���������� � ���� ó��
* @author		���� (MoonAuSosiGi@gmail.com)
* @date			2017-11-04
* @file			SpaceShip.h
* @version		0.0.1
*/
class SpaceShip
{
private:
	int m_spaceShipID;	///< ���ּ� ��Ʈ��ũ �ε��� ( ���� )
	int m_targetHostID; ///< ���� ���ּ��� �������� Ŭ���̾�Ʈ�� HostID
	bool m_isLocked; ///< ���ּ� ���� ���ɿ��� ( ��� ���� ���� )
public:
	SpaceShip();	///< �⺻ ������

	void LockSpaceShip(int hostID); ///< Ư�� Ŭ���̾�Ʈ�� ����� �õ��ߴ�.
	void UnLockSpaceShip();	///< ���ּ� ����� ������ ���� ó��
	bool IsLock();	///< ���� ���ּ��� ����ִ���
	int GetTargetHostID(); ///< ���� ���ּ��� �������� Ŭ���̾�Ʈ�� HostID
};

#endif