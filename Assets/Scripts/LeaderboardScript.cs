using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    static Dictionary<CheckPointController, int> controllerRegistrations = new Dictionary<CheckPointController, int>();
    static int carsRegistered = -1;

    [SerializeField] TextMeshProUGUI[] placesNumbers;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InitializeSceneWatcher()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        CreateScenePanel(SceneManager.GetActiveScene());
    }

    static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CreateScenePanel(scene);
    }

    static void CreateScenePanel(Scene scene)
    {
        if (scene.name != "Game")
        {
            return;
        }

        if (FindAnyObjectByType<LeaderboardScript>() != null)
        {
            return;
        }

        GameObject panelObject = new GameObject("LeaderboardRuntimePanel");
        panelObject.AddComponent<LeaderboardScript>();
    }

    void Awake()
    {
        FindPlacesIfNeeded();
        Reset();
    }

    public static void Reset()
    {
        carsRegistered = -1;
        board.Clear();
        controllerRegistrations.Clear();
    }

    public static int RegisterCar(string name)
    {
        carsRegistered++;
        board.Add(carsRegistered, new Car(string.IsNullOrWhiteSpace(name) ? "Player" : name, 0));
        return carsRegistered;
    }

    public static void SetPosition(int registrationNumber, int lap, int checkPoint)
    {
        if (!board.ContainsKey(registrationNumber))
        {
            return;
        }

        int positionScore = lap * 1000 + checkPoint;
        board[registrationNumber] = new Car(board[registrationNumber].name, positionScore);
    }

    public static void SetName(int registrationNumber, string name)
    {
        if (!board.ContainsKey(registrationNumber) || string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        board[registrationNumber] = new Car(name, board[registrationNumber].position);
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

    void LateUpdate()
    {
        FindPlacesIfNeeded();
        RegisterSceneCars();

        if (placesNumbers == null || placesNumbers.Length == 0)
        {
            return;
        }

        List<string> places = GetPlaces();

        for (int i = 0; i < placesNumbers.Length; i++)
        {
            if (placesNumbers[i] == null)
            {
                continue;
            }

            placesNumbers[i].text = i < places.Count ? places[i] : "";
        }
    }

    void FindPlacesIfNeeded()
    {
        if (placesNumbers != null && placesNumbers.Length > 0 && placesNumbers.All(place => place != null))
        {
            return;
        }

        placesNumbers = new TextMeshProUGUI[4];

        for (int i = 0; i < placesNumbers.Length; i++)
        {
            GameObject placeObject = GameObject.Find("Place" + (i + 1));
            if (placeObject != null)
            {
                placesNumbers[i] = placeObject.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    void RegisterSceneCars()
    {
        CheckPointController[] controllers = FindObjectsByType<CheckPointController>(FindObjectsInactive.Exclude);

        foreach (CheckPointController controller in controllers)
        {
            if (!controllerRegistrations.TryGetValue(controller, out int registrationNumber))
            {
                registrationNumber = RegisterCar(GetCarName(controller));
                controllerRegistrations.Add(controller, registrationNumber);
            }

            string carName = GetCarName(controller);
            SetName(registrationNumber, carName);
            SetPosition(registrationNumber, controller.lap, controller.checkPoint);
        }
    }

    string GetCarName(CheckPointController controller)
    {
        CarAppearance appearance = controller.GetComponent<CarAppearance>();

        if (appearance == null)
        {
            appearance = controller.GetComponentInParent<CarAppearance>();
        }

        if (appearance == null)
        {
            appearance = controller.GetComponentInChildren<CarAppearance>();
        }

        if (appearance != null && !string.IsNullOrWhiteSpace(appearance.playerName))
        {
            return appearance.playerName;
        }

        return "Player";
    }
}
