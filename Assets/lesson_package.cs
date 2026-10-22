// Структура для одного шага пошагового разбора
using System.Collections.Generic;

public class LessonStep
{
    public string Question;  // Что просим ввести (например, "13 / 2")
    public string Answer;    // Правильный ответ (например, "6")
    public string Remainder; // Остаток (если нужен для проверки)
    public string Detail; // Пояснение (например, "33 : 10 = 3 (ост. 3)")
    public string Prompt; 
}

// Структура для вопроса пользователю
public class LessonQuestion
{
    public string QuestionText;
    public string[] Options;
    public int CorrectOptionIndex;
}

// Структура для самостоятельной задачи
public class IndependentTask
{
    public string TaskText;      // Условие
    public string CorrectAnswer; // Ответ
    public string ErrorExplanation; // Объяснение при ошибке
}

// Главный пакет урока
public class LessonPackage
{
    public int SourceBase;
    public int TargetBase;
    public string TopicName;
    public string TheoryText;
    public string ExampleIntro;
    public string ExampleMain;
    public string ExampleError;
    public System.Collections.Generic.List<LessonStep> Steps = new System.Collections.Generic.List<LessonStep>();
    public System.Collections.Generic.List<LessonQuestion> Questions = new System.Collections.Generic.List<LessonQuestion>();
    public IndependentTask FinalTask;
    public bool IsRandomTask = false; // <-- Обязательно!
}

