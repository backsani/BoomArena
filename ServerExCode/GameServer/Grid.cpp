#include "pch.h"
#include "Grid.h"
#include "GameObject.h"
#include <algorithm>


Grid::Grid()
{
    this->bounds = AABB(0, 0, 0, 0);
}

Grid::Grid(const AABB& bounds)
{
    this->bounds = bounds;
}

Grid::~Grid()
{
}

void Grid::Add(GameObjectRef obj)
{
	objects[obj->GetId()] = obj;
}

void Grid::Remove(GameObjectRef obj)
{
	objects.erase(obj->GetId());
}

GridManager::GridManager(float worldWidth, float worldHeight, int cols, int rows) : gridCols(cols), gridRows(rows)
{
	gridWidth = worldWidth / cols;
	gridHeight = worldHeight / rows;

	grids.resize(rows, vector<GridRef>(cols));

    for (int y = 0; y < rows; ++y) 
    {
        for (int x = 0; x < cols; ++x) 
        {
            float minX = x * gridWidth;
            float minY = y * gridHeight;
            float maxX = minX + gridWidth;
            float maxY = minY + gridHeight;

            AABB cellBounds(minX, minY, maxX, maxY);

            grids[y][x] = MakeShared<Grid>(cellBounds);
        }
    }
}

GridManager::~GridManager()
{
}

/// <summary>
/// GameObject�� �ش��ϴ� grid ������ �߰��ϴ� �Լ�
/// </summary>
/// <param name="obj"> GameObject ��ü </param>
void GridManager::AddObject(GameObjectRef obj)
{
    vector<pair<int, int>> cells;
    GetOverlappingCells(obj->bounds, cells);
    for (const auto& grid : cells) 
    {
        grids[grid.second][grid.first]->Add(obj);
    }
}

/// <summary>
/// GameObject�� ��� grid �������� ����� �Լ�
/// </summary>
/// <param name="obj"> GameObject ��ü </param>
void GridManager::RemoveObject(GameObjectRef obj)
{
    vector<pair<int, int>> cells;
    GetOverlappingCells(obj->bounds, cells);
    for (const auto& grid : cells) 
    {
        grids[grid.second][grid.first]->Remove(obj);    
    }
}

/// <summary>
/// GameObject�� �������� �� grid�� �����ϴ� �Լ�.
/// </summary>
/// <param name="obj"> GameObject ��ü </param>
/// <param name="bounds"> ������ �� AABB���� </param>
void GridManager::MoveObject(GameObjectRef obj, const AABB& bounds)
{
    RemoveObject(obj);    
    obj->bounds = bounds;
    AddObject(obj);
}

/// <summary>
/// �־��� AABB ������ ���Ե� ��� grid ������ ã�Ƽ� ��ȯ���ִ� �Լ�
/// </summary>
/// <param name="bounds"> Ž���� ���� </param>
/// <param name="outCells"> ��ȯ�� grid ������ x,y ��ǥ </param>
void GridManager::GetOverlappingCells(const AABB& bounds, vector<pair<int, int>>& outCells)
{
    int startX = max(0, (int)(bounds.minX / gridWidth));
    int endX = min(gridCols - 1, (int)(bounds.maxX / gridWidth));
    int startY = max(0, (int)(bounds.minY / gridHeight));
    int endY = min(gridRows - 1, (int)(bounds.maxY / gridHeight));

    for (int y = startY; y <= endY; ++y) 
    {
        for (int x = startX; x <= endX; ++x) 
        {
            outCells.emplace_back(x, y);
        }
    }
}

void GridManager::FindAllColliders(vector<pair<GameObjectRef, GameObjectRef>>& collisions)
{
    std::set<std::pair<GameObjectRef, GameObjectRef>> uniquePairs;

    for (int y = 0; y < gridRows; ++y)
    {
        for (int x = 0; x < gridCols; ++x)
        {
            const auto& objs = grids[y][x]->objects;

            for (auto it1 = objs.begin(); it1 != objs.end(); ++it1)
            {
                for (auto it2 = std::next(it1); it2 != objs.end(); ++it2)
                {
                    GameObjectRef a = it1->second;
                    GameObjectRef b = it2->second;

                    if (a->bounds.Collider(b->bounds))
                    {
                        auto codeA = a->GetObjectCode();
                        auto codeB = b->GetObjectCode();

                        if ((codeA == GameObjectCode::BULLET && codeB == GameObjectCode::BULLET) ||
                            (codeA == GameObjectCode::PLAYER && codeB == GameObjectCode::PLAYER))
                            continue;

                        auto pair = minmax(a, b); // ���� ����
                        if (uniquePairs.insert(pair).second)
                        {
                            collisions.emplace_back(pair);
                        }
                    }
                }
            }
        }
    }
}

