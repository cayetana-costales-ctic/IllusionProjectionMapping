using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SaveUIManager : MonoBehaviour
{
    public SaveManager saveManager;

    [Header("UI")]
    public TMP_InputField inputName;

    public TMP_Dropdown dropdownFiles;
    public TextMeshProUGUI textCurrentFile;

    public Button btnSave;
    public Button btnSaveAs;
    public Button btnLoad;

    private void Start()
    {
        if (!saveManager) saveManager = FindFirstObjectByType<SaveManager>();

        inputName.gameObject.SetActive(false);
        dropdownFiles.gameObject.SetActive(false);

        btnSave.onClick.AddListener(OnQuickSaveClicked);
        btnSaveAs.onClick.AddListener(OnSaveAsClicked);
        btnLoad.onClick.AddListener(OnLoadMenuClicked);

        inputName.onSubmit.AddListener(OnInputSubmit);
        dropdownFiles.onValueChanged.AddListener(OnDropdownValueChange);

        RefreshUI();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (inputName.gameObject.activeSelf && !IsPointerOverUI(inputName.gameObject))
            {
                inputName.gameObject.SetActive(false);
            }

            if (dropdownFiles.gameObject.activeSelf)
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    dropdownFiles.gameObject.SetActive(false);
                }
            }
        }
    }

    private void OnQuickSaveClicked()
    {
        if (!string.IsNullOrEmpty(saveManager.CurrentSceneName))
        {
            saveManager.SaveScene(saveManager.CurrentSceneName);
            if (textCurrentFile) textCurrentFile.text = $"Guardado: {saveManager.CurrentSceneName}";
        }
        else
        {
            ActivateInputMode();
        }
    }

    private void OnSaveAsClicked()
    {
        ActivateInputMode();
    }

    private void OnLoadMenuClicked()
    {
        RefreshUI();

        dropdownFiles.SetValueWithoutNotify(0);

        dropdownFiles.gameObject.SetActive(true);
        inputName.gameObject.SetActive(false);

        dropdownFiles.Show();
    }

    private void OnInputSubmit(string name)
    {
        name = name.Trim();
        if (string.IsNullOrEmpty(name)) return;

        saveManager.SaveScene(name);

        inputName.gameObject.SetActive(false);
        RefreshUI();
    }

    public void OnDropdownValueChange(int index)
    {
        if (index == 0) return;

        string selectedFile = dropdownFiles.options[index].text;

        saveManager.LoadScene(selectedFile);

        dropdownFiles.gameObject.SetActive(false);
        RefreshUI();
    }

    private void RefreshUI()
    {
        dropdownFiles.ClearOptions();

        List<string> files = saveManager.GetSavedScenes();

        List<string> options = new List<string>();
        options.Add(" Choose...");
        options.AddRange(files);

        dropdownFiles.AddOptions(options);

        if (textCurrentFile)
        {
            string current = saveManager.CurrentSceneName;
            textCurrentFile.text = string.IsNullOrEmpty(current) ? "Unsaved" : $"File: {current}";
        }
    }

    private void ActivateInputMode()
    {
        dropdownFiles.gameObject.SetActive(false);
        inputName.gameObject.SetActive(true);
        inputName.text = "";
        inputName.Select();
        inputName.ActivateInputField();
    }

    private bool IsPointerOverUI(GameObject uiObject)
    {
        RectTransform rect = uiObject.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, null);
    }
}