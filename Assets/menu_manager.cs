using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public GameObject startMenuPanel;
    public GameObject theoryMenuPanel;
    public GameObject gamePanel;

    public Button continueButton;
    public Button newGameButton;

    public Button startTheoryButton;

    public scr_checking_script gameScript;


    void Start()
    {
        // Изначально показываем только главное меню
        startMenuPanel.SetActive(true);
        theoryMenuPanel.SetActive(false);
        gamePanel.SetActive(false);

        // Подписываем кнопки
        continueButton.onClick.AddListener(OnContinueClicked);
        newGameButton.onClick.AddListener(OnNewGameClicked);
        startTheoryButton.onClick.AddListener(OnStartTheoryClicked);
    }

    

    void OnNewGameClicked()
    {
        Debug.Log(">>> Нажата кнопка Новая игра");

        if (gameScript == null) { Debug.LogError("ОШИБКА: gameScript не привязан в инспекторе!"); return; }
        if (startMenuPanel == null) { Debug.LogError("ОШИБКА: startMenuPanel не привязан!"); return; }
        if (theoryMenuPanel == null) { Debug.LogError("ОШИБКА: theoryMenuPanel не привязан!"); return; }

        int savedStreak = PlayerPrefs.GetInt("SavedStreak", 0);
        int savedTotal = PlayerPrefs.GetInt("SavedTotalCorrect", 0);
        PlayerPrefs.Save();

        gameScript.LoadSaveData(0, 0);

        startMenuPanel.SetActive(false);
        theoryMenuPanel.SetActive(true);
    }

    void OnContinueClicked()
    {
        Debug.Log(">>> Нажата кнопка Продолжить");

        if (gameScript == null) { Debug.LogError("ОШИБКА: gameScript не привязан!"); return; }
        if (startMenuPanel == null) { Debug.LogError("ОШИБКА: startMenuPanel не привязан!"); return; }
        if (gamePanel == null) { Debug.LogError("ОШИБКА: gamePanel не привязан!"); return; }

        // 1. Загружаем сохраненный стрик
        int savedStreak = PlayerPrefs.GetInt("SavedStreak", 0);
        int savedTotal = PlayerPrefs.GetInt("SavedTotalCorrect", 0);
        gameScript.LoadSaveData(savedStreak, savedTotal);

        // 2. Открываем игровую панель
        startMenuPanel.SetActive(false);
        gamePanel.SetActive(true);

        // 3. Очищаем колоду, чтобы не было обучающих заданий
        gameScript.ClearDeck();

        // 4. Запускаем урок. Так как колода пуста, сгенерируется случайное задание.
        gameScript.StartNewLesson();
    }

    void OnStartTheoryClicked()
    {
        theoryMenuPanel.SetActive(false);
        gamePanel.SetActive(true);

        // Очищаем старую колоду перед сбором новой
        gameScript.ClearDeck();
        gameScript.BuildDeck();
        gameScript.StartNewLesson();
    }
}