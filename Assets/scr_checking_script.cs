using TMPro;
using UnityEngine;

public class scr_checking_script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public TMP_Dropdown Dropdown_1;
    public TMP_Dropdown Dropdown_2;
    public TMP_Dropdown Dropdown_3;
    public TMP_InputField inputField_1;
    public TMP_InputField inputField_2;
    public TMP_InputField inputField_3;

    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        int inx1 = Dropdown_1.value;
        int inx2 = Dropdown_2.value;
        int inx3 = Dropdown_3.value;
        string val1 = inputField_1.text;
        string val2 = inputField_2.text;
        string val3 = inputField_3.text;
    }
}
