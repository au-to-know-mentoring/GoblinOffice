using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UnityEngine.UI.Button GreenBtn1;
    public UnityEngine.UI.Button RedBtn2;
    public UnityEngine.UI.Button BlueBtn3;
    public UnityEngine.UI.Button YellowBtn4;
    public int test;
    public SettingsData GlobalSettingsObject;


    public Text UIButtonPressed;
    public int ButtonCurrentlyPressed = 0;
    // Start is called before the first frame update
    void Start()
    {
        //GreenBtn1.onClick.AddListener(GreenBtn1Click);
        //RedBtn2.onClick.AddListener(RedBtn2Click);
        //BlueBtn3.onClick.AddListener(BlueBtn3Click);
        //YellowBtn4.onClick.AddListener(YellowBtn4Click);

      
        SetButtonColors();
    }

    void SetButtonColors()
    {
        GreenBtn1.image.color = GlobalSettingsObject.Green1;
        RedBtn2.image.color = GlobalSettingsObject.Red2;
        BlueBtn3.image.color = GlobalSettingsObject.Blue3;
        YellowBtn4.image.color = GlobalSettingsObject.Yellow4;
    }
    // Update is called once per frame
    void Update()
    {
        switch (ButtonCurrentlyPressed)
        {
            case 1:
                // Do something for GreenBtn1 being held down.
                break;

            case 2:
                // Do something for RedBtn2 being held down.
                break;

            case 3:
                // Do something for BlueBtn3 being held down.
                break;

            case 4:
                // Do something for YellowBtn4 being held down.
                break;
        }
        UIButtonPressed.text = ButtonCurrentlyPressed.ToString();
    }

    public void ButtonPressed()
    {
        
    }

    public void NoButtonPressed()
    {
        ButtonCurrentlyPressed = 0;
    }
    public void GreenBtn1Pressed(BaseEventData eventData)
    {
        Debug.Log("Green button pressed and held.");
        ButtonCurrentlyPressed = 1;
    }

    public void RedBtn2Pressed(BaseEventData eventData)
    {
        Debug.Log("Red button pressed and held.");
        ButtonCurrentlyPressed = 2;
    }

    public void BlueBtn3Pressed(BaseEventData eventData)
    {
        Debug.Log("Blue button pressed and held.");
        ButtonCurrentlyPressed = 3;
    }

    public void YellowBtn4Pressed(BaseEventData eventData)
    {
        Debug.Log("Yellow button pressed and held.");
        ButtonCurrentlyPressed = 4;
    }
    public void ButtonReleased(BaseEventData eventData)
    {
        Debug.Log("Green button released.");
        ButtonCurrentlyPressed = 0; // Reset the button state
    }


    void GreenBtn1Click()
    {
        Debug.Log("GreenBtn1Click");
        ButtonCurrentlyPressed = 1;
    }

    void RedBtn2Click() 
    {
        Debug.Log("RedBtn2Click");
        ButtonCurrentlyPressed = 2;
    }

    void BlueBtn3Click()
    {
        Debug.Log("BlueBtn3Click");
        ButtonCurrentlyPressed = 3;
    }

    void YellowBtn4Click()
    {
        Debug.Log("YellowBtn4Click");
        ButtonCurrentlyPressed = 4;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (eventData.selectedObject == GreenBtn1.gameObject)
            ButtonCurrentlyPressed= 1;
        else if (eventData.selectedObject == RedBtn2.gameObject)
            ButtonCurrentlyPressed = 2;
        else if (eventData.selectedObject == BlueBtn3.gameObject)
            ButtonCurrentlyPressed = 3;
        else if (eventData.selectedObject == YellowBtn4.gameObject)
            ButtonCurrentlyPressed = 4;

        Debug.Log("OnPointerDown called");
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {

        ButtonCurrentlyPressed = 0;
    }
}
