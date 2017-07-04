#ifndef ITEM_H
#define ITEM_H
#include "stdafx.h"

class Item
{
public:
	int m_itemCID;
	int m_itemID;
	Proud::Vector3 pos;
	Proud::Vector3 rot;
public:
	Item();
	~Item();
};

#endif