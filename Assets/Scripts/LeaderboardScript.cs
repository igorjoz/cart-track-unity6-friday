using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Car
{
    public string name;
    public int position;

    public Car(string name, int position)
    {
        this.name = name;
        this.position = position;
    }
}


public class LeaderboardScript : MonoBehaviour
{
    static Dictionary<int, Car> board = new Dictionary<int, Car>();
    static int carsRegisterd = -1;

    public static void Reset()
    {
        carsRegisterd = -1;
        board.Clear();
    }

    public static int RegisterCar(string name)
    {
        carsRegisterd++;
        board.Add(carsRegisterd, new Car(name, 0));
        return carsRegisterd;
    }

    public static void SetPosition(int registrationNumber, int lap, int checkPoint)
    {
        int positionScore = lap * 1000 + checkPoint;
        board[registrationNumber] = new Car(board[registrationNumber].name, positionScore);
    }

    public static List<string> GetPlaces()
    {
        List<string> places = new List<string>();

        foreach (var positionScore in board.OrderByDescending(key => key.Value.position))
        {
            places.Add(positionScore.Value.name);
        }

        return places;
    }
}
