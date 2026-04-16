using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public NetworkManager networkManager;

    public void OnButtonClicked()
    {
        Debug.Log("Button clicked!");

        networkManager.SendButtonClick();
    }
}