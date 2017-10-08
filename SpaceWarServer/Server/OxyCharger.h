#ifndef OXYCHARGER_H
#define OXYCHARGER_H

class OxyCharger
{
public:
	int m_oxyChargerID;
	float m_oxyValue;
	
	OxyCharger();
	~OxyCharger();
public:
	bool IsLocked() { return m_isLocked; }
	int GetUseHostID() { return m_useHostID; }
	void LockOxyCharger(int hostID) { m_isLocked = true; m_useHostID = hostID; }
	void UnLockOxyCharger() { m_isLocked = false; m_useHostID = -1; }
private:
	bool m_isLocked;
	int m_useHostID;

};

#endif