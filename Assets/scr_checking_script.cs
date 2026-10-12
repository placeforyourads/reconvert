using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


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

    public bool flag = false;

    public float cooldown = 1f;

    private float lastClickTime = -999f;


    void Start()
    {
        ApprovalButton.onClick.AddListener(onClickApproval);
        Dropdown_1.onValueChanged.AddListener(onAnyDropdownChanged);
        Dropdown_2.onValueChanged.AddListener(onAnyDropdownChanged);
        Dropdown_3.onValueChanged.AddListener(onAnyDropdownChanged);
        ApprovalButton.onClick.AddListener(OnButtonCooldownClick);
        onAnyDropdownChanged(0);
        CreateTask();
        //add normalization
    }

    void onClickApproval()
    {
        int inx1 = Dropdown_1.value;
        inx1 += 2;
        int inx3 = Dropdown_3.value;
        inx3 += 2;
        string number1 = inputField_1.text;
        string number3 = inputField_3.text;
        Debug.Log("click");
        if (AreEqual(number1, inx1, number3, inx3))
        {
            Image buttonImage = ApprovalButton.GetComponent<Image>();
            buttonImage.color = Color.green;
            flag = true;
            Debug.Log("Right");
            CreateTask();
        }
        else
        {
            Image buttonImage = ApprovalButton.GetComponent<Image>();
            buttonImage.color = Color.red;
        }
    }

    void onAnyDropdownChanged(int index)
    {
        bool shouldDisable = Dropdown_1.value == 8 || Dropdown_3.value == 8;

        // выкл/вкл флаг
        inputField_2.interactable = !shouldDisable;
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
        uint value = (uint)(UnityEngine.Random.Range(0, 1000));
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
        return res;
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

}