#ifndef SPACESHIP_H
#define SPACESHIP_H

class SpaceShip
{
private:
	int m_spaceShipID;
	int m_targetHostID;
	bool m_isLocked;
public:
	SpaceShip();
	~SpaceShip();

	void LockSpaceShip(int hostID) { m_isLocked = true; m_targetHostID = hostID; }
	void UnLockSpaceShip() { m_isLocked = false; m_targetHostID = -1; }
};

#endif