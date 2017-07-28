#include "stdafx.h"
#include "Shelter.h"


Shelter::Shelter()
{
	m_doorState = false;
	m_lightState = false;
	m_shelterHumanCounter = 0;
}


Shelter::~Shelter()
{
}


void Shelter::ShelterEnter()
{
	m_shelterHumanCounter++;

	if (m_shelterHumanCounter > 0)
		m_lightState = true;
}

void Shelter::ShelterExit()
{
	m_shelterHumanCounter--;

	if (m_shelterHumanCounter <= 0)
	{
		m_shelterHumanCounter = 0;
		m_lightState = false;
	}
}

void Shelter::ShelterDoorStateChange(bool state)
{
	m_doorState = state;
}