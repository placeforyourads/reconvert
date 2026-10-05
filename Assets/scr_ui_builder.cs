using UnityEngine;
using TMPro;

public class InputFieldsLayout : MonoBehaviour
{
    public TMP_InputField[] inputFields; // 3 поля

    /*[Header("Размеры и отступы")]
    public Vector2 fieldSize = new Vector2(800f, 200f);
    public float spacing = 400f;           // отступ между полями
    public Vector2 screenOffset = Vector2.zero; // смещение всей группы*/

    [Header("Выпадающие списки")]
    public TMP_Dropdown[] dropdowns;               // списки (по одному на поле)
    /*public Vector2 dropdownSize = new Vector2(100f, 100f); // размер списка
    public float dropdownGap = 40f;                // отступ между списком и полем*/

    void Start()
    {
        //LayoutFields();
    }

    /*void LayoutFields()
    {

        int count = inputFields.Length;
        float step = fieldSize.y + spacing;

        // Общая высота группы (для центрирования)
        float totalHeight = count * fieldSize.y + (count - 1) * spacing;

        for (int i = 0; i < count; i++)
        {
            var field = inputFields[i];
            if (field == null) continue;

            RectTransform rt = field.GetComponent<RectTransform>();

            // Размер
            rt.sizeDelta = fieldSize;

            // Якорь — центр экрана
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            // Позиция: первое поле сверху, далее вниз
            float y = totalHeight / 2f - fieldSize.y / 2f - i * step;
            rt.anchoredPosition = new Vector2(screenOffset.x, y + screenOffset.y);

            // инициализация списков
            if (dropdowns != null && i < dropdowns.Length && dropdowns[i] != null)
            {
                RectTransform drt = dropdowns[i].GetComponent<RectTransform>();

                // Размер списка
                drt.sizeDelta = dropdownSize;

                // Якорь — центр экрана (как у поля)
                drt.anchorMin = new Vector2(0.5f, 0.5f);
                drt.anchorMax = new Vector2(0.5f, 0.5f);
                drt.pivot = new Vector2(0.5f, 0.5f);

                // Позиция: слева от поля на расстоянии (полширины поля + зазор + полширины списка)
                float x = -(fieldSize.x / 2f + dropdownGap + dropdownSize.x / 2f);
                drt.anchoredPosition = new Vector2(
                    screenOffset.x + x,
                    y + screenOffset.y
                );
            }
        }
    }*/
}