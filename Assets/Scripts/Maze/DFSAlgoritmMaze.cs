using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DFSAlgoritmMaze : MonoBehaviour {


    public void SetRandoMSeed(int _seed)
    {
        Random.InitState(_seed);
    }
    public bool CheckPositionInMatrixAreValid(int x, int y, int width, int height)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public List<int> generateRandomDirections(List<int> directions)
    {
        return ShuffleArray(directions);
    }

    public List<int> ShuffleArray(List<int> list)
    {

        int n = list.Count;
        while (n > 1)
        {
            int k = (Random.Range(0, n) % n);
            n--;
            int value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }

    public IEnumerator GenerateMaze(Cell[] cellsVector, int width, int height)
    {
        //choose the start cell
        List<int> cellsStack = new List<int>();

        int totalCells = cellsVector.Length;
        int visitedCells = 0;
        int curentCellIndex = 0;
        cellsVector[curentCellIndex].visited = true;
        visitedCells++;

       
        while (visitedCells <totalCells)
        {
            List<int> neighbors = GetNeighboursUnvisited(cellsVector, curentCellIndex, height);
            if (neighbors.Count > 0)
            {
                if (cellsVector[curentCellIndex].visited == true)
                {
                    List<int> cacealmaList = generateRandomDirections(neighbors);
                    int chosenOne = neighbors[Random.Range(0,cacealmaList.Count)];
                    DestroyWallsBetweenCells(cellsVector, curentCellIndex, chosenOne);
                    yield return new WaitForSeconds(0.01f);
                    cellsVector[chosenOne].visited = true;
                    visitedCells++;
                    cellsStack.Add(curentCellIndex);
                    curentCellIndex = chosenOne;
                }
            }
            else
            {
                int recentCell = (cellsStack.Count != 0) ? cellsStack[cellsStack.Count - 1] : 0;
                if (cellsStack.Count != 0)
                    cellsStack.RemoveAt(cellsStack.Count - 1);
                

                curentCellIndex = recentCell;
            }
        }
        yield return null;
    }

    public void DestroyWallsBetweenCells(Cell[] cellsVector, int index1, int index2)
    {
        if (index2 > index1)
        {
            if (index2 - index1 == 1)
            {
                Destroy(cellsVector[index1].south);
            }
            else
            {
                Destroy(cellsVector[index1].east);
            }
        }
        else
        {
            if (index1 - index2 == 1)
            {
                Destroy(cellsVector[index1].north);
            }
            else
            {
                Destroy(cellsVector[index1].west);
            }
        }
    }

    List<int> GetNeighboursUnvisited(Cell[] cellsVector, int indexOfChild, int heightOfMatrix)
    {
        List<int> neighbors = new List<int>();
        if (CheckIndexIsCorect(cellsVector, indexOfChild, indexOfChild + 1))
        {
            if (CheckIfCellIsUnvisitedAndExist(cellsVector, indexOfChild + 1) &&  cellsVector[indexOfChild].isDown == false)
            {
                neighbors.Add(indexOfChild + 1);
            }
        }
        if (CheckIndexIsCorect(cellsVector, indexOfChild, indexOfChild - 1))
        {
            if (CheckIfCellIsUnvisitedAndExist(cellsVector, indexOfChild - 1) && cellsVector[indexOfChild].isUp == false)
            {
                neighbors.Add(indexOfChild - 1);
            }
        }
        if (CheckIndexIsCorect(cellsVector, indexOfChild, indexOfChild + heightOfMatrix))
        {
            if (CheckIfCellIsUnvisitedAndExist(cellsVector, indexOfChild + heightOfMatrix))
            {
                neighbors.Add(indexOfChild + heightOfMatrix);
            }
        }
        if (CheckIndexIsCorect(cellsVector, indexOfChild, indexOfChild - heightOfMatrix))
        {
            if (CheckIfCellIsUnvisitedAndExist(cellsVector, indexOfChild - heightOfMatrix))
            {
                neighbors.Add(indexOfChild - heightOfMatrix);
            }
        }
        return neighbors;
    }
    

    bool CheckIfCellIsUnvisitedAndExist(Cell[] cellsVector, int index)
    {
        if (cellsVector[index].visited == false)
            return true;
        else
            return false;
    }
    bool CheckIndexIsCorect(Cell[] cellsVector,int indexCurent, int indexNou)
    {
        return indexNou >= 0 && indexNou < cellsVector.Length;
    }
}
