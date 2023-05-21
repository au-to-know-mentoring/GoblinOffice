using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Unity.VisualScripting;

public class InputManager : MonoBehaviour
{
    public UnityEngine.UI.Button GreenBtn1;
    public UnityEngine.UI.Button RedBtn2;
    public UnityEngine.UI.Button BlueBtn3;
    public UnityEngine.UI.Button YellowBtn4;
    public int test;

    // Start is called before the first frame update
    void Start()
    {
        GreenBtn1.onClick.AddListener(GreenBtn1Click);
        RedBtn2.onClick.AddListener(RedBtn2Click);
        BlueBtn3.onClick.AddListener(BlueBtn3Click);
        YellowBtn4.onClick.AddListener(YellowBtn4Click);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonPressed()
    {
        
    }

    void GreenBtn1Click()
    {
        Debug.Log("GreenBtn1Click");
    }

    void RedBtn2Click() 
    {
        Debug.Log("RedBtn2Click");
    }

    void BlueBtn3Click()
    {
        Debug.Log("BlueBtn3Click");
    }

    void YellowBtn4Click()
    {
        Debug.Log("YellowBtn4Click");
    }
}
