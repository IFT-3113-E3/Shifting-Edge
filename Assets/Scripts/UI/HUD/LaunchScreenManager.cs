using UnityEngine;

public class HomeScreenManager : MonoBehaviour
{

    void Update()
    {
        if (InputDetected())
        {
            LoadMainMenu();
        }
    }

    private bool InputDetected()
    {
        // Souris
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            return true;
        // Clavier
        if (Input.anyKeyDown)
            return true;
        // Manette (Unity détecte les axes de la manette comme Input.GetAxis et Input.GetButton)
        if (Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel"))
            return true;
        // Détection des axes des manettes (mouvement des sticks)
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            return true;

        return false;
    }

    public void LoadMainMenu()
    {
        SceneLoader.LoadScene("StartMenu");
    }
}
