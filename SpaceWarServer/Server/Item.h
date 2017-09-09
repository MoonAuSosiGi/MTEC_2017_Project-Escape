#ifndef ITEM_H
#define ITEM_H
#include "stdafx.h"

class Item
{
public:
	string m_itemID;
	string m_networkID;
	
	//test
	int m_count;
	Proud::Vector3 pos;
	Proud::Vector3 rot;
public:
	Item();
	~Item();
};

#endif