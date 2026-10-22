using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class scr_checking_script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public TMP_Dropdown Dropdown_1;
    public TMP_Dropdown Dropdown_2;
    public TMP_Dropdown Dropdown_3;
    public TMP_InputField inputField_1;
    public TMP_InputField inputField_2;
    public TMP_InputField inputField_3;
    public Button ApprovalButton;
    public TMP_InputField Out_Txfield;
    // Out_Txfield.interactable = false;
    public bool flag = false;

    public float cooldown = 1.5f;

    private float lastClickTime = -999f;

    StringNormalizer Normalizer = new StringNormalizer();
    Settings Config = new Settings();

    // Текущий сгенерированный урок
    private LessonPackage currentLesson;
    // Индекс текущего шага в пошаговом разборе
    private int currentStepIndex = 0;
    // Флаг: мы сейчас в режиме самостоятельной задачи или в режиме разбора?
    private bool isIndependentTaskMode = false;

    private System.Collections.Generic.List<LessonPackage> lessonDeck = new System.Collections.Generic.List<LessonPackage>();
    private int deckIndex = 0;
    public int totalCorrect = 0;
    public int streak = 0;
    private bool lastAnswerWasCorrect = false;


    void Start()
    {
        ApprovalButton.onClick.AddListener(onClickApproval);
        Dropdown_1.onValueChanged.AddListener(onAnyDropdownChanged);
        Dropdown_2.onValueChanged.AddListener(onAnyDropdownChanged);
        Dropdown_3.onValueChanged.AddListener(onAnyDropdownChanged);
        ApprovalButton.onClick.AddListener(OnButtonCooldownClick);
        onAnyDropdownChanged(0);
        // CreateTask();

        inputField_1.onValueChanged.AddListener(text => ToBeNormalized(inputField_1, text));
        inputField_2.onValueChanged.AddListener(text => ToBeNormalized(inputField_2, text));
        inputField_3.onValueChanged.AddListener(text => ToBeNormalized(inputField_3, text));

        // BuildDeck();
        // StartNewLesson();
    }

    public void LoadSaveData(int savedStreak, int savedTotal)
    {
        streak = savedStreak;
        totalCorrect = savedTotal;
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt("SavedStreak", streak);
        PlayerPrefs.SetInt("SavedTotalCorrect", totalCorrect);
        PlayerPrefs.Save();
    }


    public void StartNewLesson()
    {
        ClearScreen();

        // Если в колоде есть уроки — берем следующий
        if (deckIndex < lessonDeck.Count)
        {
            currentLesson = lessonDeck[deckIndex];
            deckIndex++;
            Debug.Log($">>> Взят урок {deckIndex}/{lessonDeck.Count} из колоды");
        }
        else
        {
            // Колода закончилась — генерируем самостоятельную задачу
            Debug.Log(">>> Колода закончилась! Самостоятельный режим.");
            lessonDeck.Clear();
            currentLesson = GenerateRandomXToYTask();
        }

        currentStepIndex = 0;
        isIndependentTaskMode = false;

        LogToScreen($"=== НОВЫЙ УРОК: {currentLesson.TopicName} ===");
        LogToScreen($"Стрик: {streak} (всего верных: {totalCorrect})");

        if (!currentLesson.IsRandomTask)
        {
            if (!string.IsNullOrEmpty(currentLesson.TheoryText))
                LogToScreen($"Теория: {currentLesson.TheoryText}");
            if (!string.IsNullOrEmpty(currentLesson.ExampleIntro))
                LogToScreen($"Пример (вводный): {currentLesson.ExampleIntro}");
            if (!string.IsNullOrEmpty(currentLesson.ExampleMain))
                LogToScreen($"Пример (основной): {currentLesson.ExampleMain}");
            if (!string.IsNullOrEmpty(currentLesson.ExampleError))
                LogToScreen($"Пример (ошибка): {currentLesson.ExampleError}");
            LogToScreen("---");
        }

        ShowCurrentStep();
    }



    void ShowCurrentStep()
    {
        inputField_2.gameObject.SetActive(false);
        Dropdown_2.gameObject.SetActive(false);

        Dropdown_3.gameObject.SetActive(true);
        Dropdown_3.interactable = false;
        Dropdown_3.SetValueWithoutNotify(currentLesson.TargetBase - 2);

        if (currentStepIndex < currentLesson.Steps.Count)
        {
            LessonStep step = currentLesson.Steps[currentStepIndex];

            // Очищаем/заполняем первое поле (оставим как было для 10->X)
            inputField_1.onValueChanged.RemoveAllListeners();
            if (currentLesson.SourceBase == 10 && step.Question.Contains(":"))
                inputField_1.text = step.Question.Split(' ')[0];
            else
                inputField_1.text = "";
            inputField_1.interactable = false;
            inputField_1.onValueChanged.AddListener(text => ToBeNormalized(inputField_1, text));

            inputField_3.text = "";
            inputField_3.interactable = true;
            inputField_3.ActivateInputField();

            // Выводим задание (Prompt)
            string taskText = string.IsNullOrEmpty(step.Prompt) ? step.Question : step.Prompt;
            LogToScreen($"Шаг {currentStepIndex + 1}: {taskText}");
        }
        else
        {
            isIndependentTaskMode = true;
            ShowIndependentTask();
        }
    }

    void ShowIndependentTask()
    {
        LogToScreen("=== САМОСТОЯТЕЛЬНАЯ ЗАДАЧА ===");

        inputField_2.gameObject.SetActive(false);
        Dropdown_2.gameObject.SetActive(false);

        Dropdown_3.gameObject.SetActive(true);
        Dropdown_3.interactable = false;
        Dropdown_3.SetValueWithoutNotify(currentLesson.TargetBase - 2);

        inputField_1.onValueChanged.RemoveAllListeners();
        inputField_1.text = "";
        inputField_1.interactable = false;
        inputField_1.onValueChanged.AddListener(text => ToBeNormalized(inputField_1, text));

        inputField_3.text = "";
        inputField_3.interactable = true;
        inputField_3.ActivateInputField();

        LogToScreen($"Задача: {currentLesson.FinalTask.TaskText}");
    }

    void onClickApproval()
    {
        // Если урок еще не сгенерирован, ничего не делаем
        if (currentLesson == null)
        {
            Debug.LogWarning(">>> Попытка нажать Check, но урок еще не сгенерирован!");
            return;
        }
        if (Time.time - lastClickTime < cooldown) return;
        lastClickTime = Time.time;

        Image buttonImage = ApprovalButton.GetComponent<Image>();

        if (!isIndependentTaskMode)
        {
            Debug.Log(">>> ВЕТКА: разбор");
            LessonStep currentStep = currentLesson.Steps[currentStepIndex];
            string userAnswer = inputField_3.text.Trim();
            Debug.Log($"СРАВНЕНИЕ: userAnswer='{userAnswer}', correct='{currentStep.Answer}', равно={userAnswer == currentStep.Answer}");

            if (userAnswer == currentStep.Answer)
            {
                buttonImage.color = Color.green;

                // Если есть Detail (результат), выводим его. Иначе просто "верно".
                if (!string.IsNullOrEmpty(currentStep.Detail))
                    LogToScreen($"Шаг {currentStepIndex + 1}: верно! ({currentStep.Detail})");
                else
                    LogToScreen($"Шаг {currentStepIndex + 1}: верно! ({userAnswer})");

                currentStepIndex++;
                totalCorrect++;
                streak++;
                lastAnswerWasCorrect = true;
                SaveProgress();
                Invoke(nameof(ShowCurrentStep), 0.5f);
            }
        }
        else
        {
            Debug.Log(">>> ВЕТКА: самостоятельная задача");
            string userAnswer = inputField_3.text.Trim();

            if (userAnswer == currentLesson.FinalTask.CorrectAnswer)
            {
                buttonImage.color = Color.green;
                LogToScreen("Самостоятельная задача решена верно! Урок завершен.");
                totalCorrect++; // (если вы хотите считать и это)
                streak++;       // (если вы хотите считать и это)
                SaveProgress(); // <--- ДОБАВИТЬ ВОТ ЭТО
                Invoke(nameof(StartNewLesson), 1.5f);
            }
            else
            {
                buttonImage.color = Color.red;
                LogToScreen($"ОШИБКА: {currentLesson.FinalTask.ErrorExplanation}");
                LogToScreen($"Правильный ответ: {currentLesson.FinalTask.CorrectAnswer}. Попробуйте снова.");
            }
        }
    }

    void onAnyDropdownChanged(int index)
    {
        bool shouldDisable = Dropdown_1.value == 8 || Dropdown_3.value == 8;

        // выкл/вкл флаг
        inputField_3.interactable = !shouldDisable;
        Dropdown_2.interactable = !shouldDisable;
    }

    bool AreEqual(string number_1, int base_1, string number_3, int base_3)
    {

        // Приводим оба числа к десятичной системе (промежуточная степень = 10)
        uint value1 = ConvertToDecimal(number_1, base_1);
        uint value3 = ConvertToDecimal(number_3, base_3);

        return value1 == value3;
    }

    uint ConvertToDecimal(string number, int baseFrom)
    {
        uint result = 0;

        foreach (char c in number)
        {
            int digit;
            if (c >= '0' && c <= '9')
                digit = c - '0';
            else if (c >= 'A' && c <= 'F')
                digit = c - 'A' + 10;
            else if (c >= 'a' && c <= 'f')
                digit = c - 'a' + 10;
            else
                throw new FormatException($"Недопустимый символ '{c}' в числе.");

            checked
            {
                result = (uint)(result * baseFrom + digit);
            }
        }

        return result;
    }

    // Update is called once per frame
    void Update() { }

    (string number, int BaseSystem) CreateRandNumber()
    {
        int BaseSystem = getRandBase();
        uint value = (uint)(UnityEngine.Random.Range(0, Mathf.Pow(10, Config.MaxIntLength)));
        string number = ConvertToBasic(value, BaseSystem);
        return (number, BaseSystem);
    }

    int getRandBase()
    {
        return UnityEngine.Random.Range(2, 17);
    }

    string ConvertToBasic(uint value, int baseSystem)
    {
        if (value == 0) { return "0"; }
        string digits = "0123456789ABCDEF";
        string res = "";
        while (value > 0)
        {
            res += digits[(int)(value % (uint)baseSystem)];
            value /= (uint)baseSystem;
        }
        char[] arr = res.ToCharArray();
        System.Array.Reverse(arr);
        return new string(arr);
    }

    void CreateTask()
    {
        string number1;
        int baseSystem1;
        (number1, baseSystem1) = CreateRandNumber();
        int baseSystem3 = getRandBase();
        inputField_1.text = number1;
        Dropdown_1.SetValueWithoutNotify(baseSystem1 - 2);
        Dropdown_3.SetValueWithoutNotify(baseSystem3 - 2);
        inputField_1.interactable = false;
        Dropdown_1.interactable = false;
        Dropdown_3.interactable = false;
        if (baseSystem1 != 8 || baseSystem3 != 8)
        {
            inputField_2.interactable = true;
            Dropdown_2.interactable = true;
        }
    }

    void OnButtonCooldownClick()
    {
        if (Time.time - lastClickTime < cooldown)
            return;

        lastClickTime = Time.time;
    }

    void ToBeNormalized(TMP_InputField InputField, string text) {
        text = Normalizer.Normalize(text);
        InputField.text = text;
    }


    // Обновлённая функция-диспетчер
    public LessonPackage GenerateLessonPackage()
    {
        // Рандомно выбираем тип задания (для бесконечного режима)
        int type = UnityEngine.Random.Range(1, 4); // 1, 2 или 3

        switch (type)
        {
            case 1:
                return Generate10ToX();
            case 2:
                return GenerateXTo10();
            case 3:
            default:
                return GenerateXToY();
        }
    }

    void LogToScreen(string message)
    {
        if (Out_Txfield != null)
        {
            Out_Txfield.text += message + "\n";
        }
        // На всякий случай дублируем в консоль для отладки
        Debug.Log(message);
    }

    void ClearScreen()
    {
        if (Out_Txfield != null)
        {
            Out_Txfield.text = "";
        }
    }

    // Тип 1: 10 -> X
    LessonPackage Generate10ToX()
    {
        LessonPackage lesson = new LessonPackage();
        int targetBase = getRandBase();
        lesson.TargetBase = targetBase;
        lesson.SourceBase = 10;
        lesson.TopicName = $"Перевод из 10-й в {targetBase}-ю";

        uint baseValue = (uint)UnityEngine.Random.Range(15, 50);
        string correctResult = ConvertToBasic(baseValue, targetBase);

        lesson.TheoryText = $"Чтобы перевести число из 10-й в {targetBase}-ю, делим на {targetBase} и записываем остатки снизу вверх.";

        uint tempValue = baseValue;
        string remainders = "";

        while (tempValue > 0)
        {
            uint quotient = tempValue / (uint)targetBase;
            uint remainder = tempValue % (uint)targetBase;
            // ...
            lesson.Steps.Add(new LessonStep
            {
                Question = $"{tempValue} : {targetBase}",
                Answer = remainder.ToString(),
                Prompt = $"{tempValue} : {targetBase}. Введите ОСТАТОК:",
                Detail = $"{tempValue} : {targetBase} = {quotient} (ост. {remainder})"
            });
            tempValue = quotient;
        }

        lesson.Steps.Add(new LessonStep
        {
            Question = "Итоговый ответ",
            Answer = correctResult,
            Prompt = "Соберите остатки снизу вверх и запишите итоговое число:",
            Detail = $"Получилось число {correctResult}"
        });

        uint independentValue = (uint)UnityEngine.Random.Range(15, 50);
        lesson.FinalTask = new IndependentTask
        {
            TaskText = $"Переведите число {independentValue} в {targetBase}-ю систему.",
            CorrectAnswer = ConvertToBasic(independentValue, targetBase),
            ErrorExplanation = "Неверно. Читайте остатки снизу вверх!"
        };
        return lesson;
    }

    // Тип 2: X -> 10
    LessonPackage GenerateXTo10()
    {
        LessonPackage lesson = new LessonPackage();
        int sourceBase = getRandBase();
        lesson.SourceBase = sourceBase;
        lesson.TargetBase = 10;
        lesson.TopicName = $"Перевод из {sourceBase}-й в 10-ю";

        uint baseValue = (uint)UnityEngine.Random.Range(15, 50);
        string sourceStr = ConvertToBasic(baseValue, sourceBase);

        lesson.TheoryText = $"Чтобы перевести из {sourceBase}-й в 10-ю, умножаем каждую цифру на {sourceBase} в степени (позиция) и складываем.";

        uint power = 1;
        uint sum = 0;

        for (int i = sourceStr.Length - 1; i >= 0; i--)
        {
            char c = sourceStr[i];
            int digit = c >= '0' && c <= '9' ? c - '0' : c - 'A' + 10;
            uint result = (uint)(digit * power);

            lesson.Steps.Add(new LessonStep
            {
                Question = $"{digit} * {sourceBase}^{sourceStr.Length - 1 - i}",
                Answer = result.ToString(),
                Prompt = $"Умножьте {digit} на {sourceBase} в степени {sourceStr.Length - 1 - i}:",
                Detail = $"{digit} * {sourceBase}^{sourceStr.Length - 1 - i} = {result}"
            });

            sum += result;
            power *= (uint)sourceBase;
        }

        lesson.Steps.Add(new LessonStep
        {
            Question = "Итоговая сумма",
            Answer = sum.ToString(),
            Prompt = "Сложите все полученные результаты и запишите сумму:",
            Detail = $"Сумма равна {sum}"
        });

        uint independentValue = (uint)UnityEngine.Random.Range(15, 50);
        string indStr = ConvertToBasic(independentValue, sourceBase);
        lesson.FinalTask = new IndependentTask
        {
            TaskText = $"Переведите число {indStr} из {sourceBase}-й в 10-ю.",
            CorrectAnswer = independentValue.ToString(),
            ErrorExplanation = "Неверно. Проверьте степени."
        };
        return lesson;
    }

    // Тип 3: X -> Y (ни одна не 10)
    LessonPackage GenerateXToY()
    {
        LessonPackage lesson = new LessonPackage();
        int sourceBase = getRandBase();
        int targetBase = getRandBase();
        while (targetBase == sourceBase) targetBase = getRandBase();

        lesson.SourceBase = sourceBase;
        lesson.TargetBase = targetBase;
        lesson.TopicName = $"Перевод из {sourceBase}-й в {targetBase}-ю";

        uint baseValue = (uint)UnityEngine.Random.Range(15, 50);
        string sourceStr = ConvertToBasic(baseValue, sourceBase);

        lesson.TheoryText = $"Чтобы перевести из {sourceBase}-й в {targetBase}-ю, сначала переведите в 10-ю, затем в {targetBase}-ю.";

        uint power = 1;
        uint sum = 0;

        for (int i = sourceStr.Length - 1; i >= 0; i--)
        {
            char c = sourceStr[i];
            int digit = c >= '0' && c <= '9' ? c - '0' : c - 'A' + 10;
            uint result = (uint)(digit * power);

            lesson.Steps.Add(new LessonStep
            {
                Question = $"{digit} * {sourceBase}^{sourceStr.Length - 1 - i}",
                Answer = result.ToString(),
                Prompt = $"Умножьте {digit} на {sourceBase} в степени {sourceStr.Length - 1 - i}:",
                Detail = $"{digit} * {sourceBase}^{sourceStr.Length - 1 - i} = {result}"
            });

            sum += result;
            power *= (uint)sourceBase;
        }

        // ДОБАВЛЯЕМ ШАГ НА СУММУ (новое)
        lesson.Steps.Add(new LessonStep
        {
            Question = "Сумма",
            Answer = sum.ToString(),
            Prompt = "Сложите все полученные результаты и запишите сумму (в 10-й системе):",
            Detail = $"Сумма равна {sum}"
        });

        // ДОБАВЛЯЕМ ШАГ НА ПЕРЕВОД (финальный)
        lesson.Steps.Add(new LessonStep
        {
            Question = "Итоговый ответ",
            Answer = ConvertToBasic(sum, targetBase), // Берем именно сумму!
            Prompt = $"Переведите полученную сумму ({sum}) из 10-й в {targetBase}-ю систему:",
            Detail = $"Итоговый ответ: {ConvertToBasic(sum, targetBase)}"
        });

        uint independentValue = (uint)UnityEngine.Random.Range(15, 50);
        string indSourceStr = ConvertToBasic(independentValue, sourceBase);
        lesson.FinalTask = new IndependentTask
        {
            TaskText = $"Переведите число {indSourceStr} из {sourceBase}-й в {targetBase}-ю.",
            CorrectAnswer = ConvertToBasic(independentValue, targetBase),
            ErrorExplanation = "Неверно. Сначала в 10-ю, потом в нужную."
        };
        return lesson;
    }

    public void BuildDeck()
    {
        lessonDeck.Clear();
        deckIndex = 0;

        // 2-4 набора 10 -> X
        int count1 = UnityEngine.Random.Range(2, 5);
        for (int i = 0; i < count1; i++) lessonDeck.Add(Generate10ToX());

        // 2-4 набора X -> 10
        int count2 = UnityEngine.Random.Range(2, 5);
        for (int i = 0; i < count2; i++) lessonDeck.Add(GenerateXTo10());

        // 2-4 набора X -> Y
        int count3 = UnityEngine.Random.Range(2, 5);
        for (int i = 0; i < count3; i++) lessonDeck.Add(GenerateXToY());

        Debug.Log($">>> Колода создана: {lessonDeck.Count} уроков");
    }

    public void ClearDeck()
    {
        lessonDeck.Clear();
        deckIndex = 0;
    }

    LessonPackage GenerateRandomXToYTask()
    {
        LessonPackage lesson = new LessonPackage();
        lesson.IsRandomTask = true;

        // Выбираем случайные основания от 2 до 16
        int sourceBase = getRandBase();
        int targetBase = getRandBase();

        // Исключаем совпадение оснований (чтобы перевод был осмысленным)
        while (targetBase == sourceBase)
        {
            targetBase = getRandBase();
        }

        lesson.SourceBase = sourceBase;
        lesson.TargetBase = targetBase;
        lesson.TopicName = $"Случайное задание: из {sourceBase}-й в {targetBase}-ю";

        // Генерируем число до 4096
        uint randomValue = (uint)UnityEngine.Random.Range(0, 4096);

        // Переводим в исходную систему для отображения
        string sourceStr = ConvertToBasic(randomValue, sourceBase);

        // Теория не нужна, так как это отработка
        lesson.TheoryText = "Решите задание самостоятельно, без пошаговых подсказок.";

        // --- Генерируем финальную задачу (без шагов) ---
        // Правильный ответ: переводим исходное число в целевую систему
        string correctAnswer = ConvertToBasic(randomValue, targetBase);

        lesson.FinalTask = new IndependentTask
        {
            TaskText = $"Переведите число {sourceStr} из {sourceBase}-й системы счисления в {targetBase}-ю.",
            CorrectAnswer = correctAnswer,
            ErrorExplanation = "Неверно. Проверьте свои вычисления. Возможно, вы забыли перевернуть остатки или ошиблись в степенях."
        };

        return lesson;
    }
}