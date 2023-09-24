using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

public class LoginManager : MonoBehaviour
{
    //Login Panel
    [SerializeField] private Toggle _isAdminToggle;
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private GameObject _userAfterLoggedIn;
    [SerializeField] private GameObject _adminPanel;
    [SerializeField] private Button _loginButton;
    [SerializeField] private TMP_InputField _userName;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private TextMeshProUGUI _errorText;

    // After Login Panel
    [SerializeField] private GameObject _changePasswordFieldGo;
    [SerializeField] private TMP_InputField _changePasswordField;
    [SerializeField] private GameObject _confirmPasswordFieldGo;
    [SerializeField] private TMP_InputField _confirmPasswordField;
    [SerializeField] private Button _changePasswordButton;
    [SerializeField] private GameObject _confirmChangePasswordButtonGo;
    [SerializeField] private Button _confirmChangePasswordButton;
    [SerializeField] private Button _logoutButton;
    [SerializeField] private TextMeshProUGUI _passwordMissmatchErrorText;

    // Admin Panel
    [SerializeField] private TMP_Dropdown _listOfUsers;
    [SerializeField] private Button _changeUserName;
    [SerializeField] private Button _changeUserPassword;
    [SerializeField] private Button _addNewUser;
    [SerializeField] private Button _deleteUser;
    [SerializeField] private Button _blockUser;
    [SerializeField] private Button _setDayLimit;
    [SerializeField] private Toggle _passwordRestriction;
    [SerializeField] private TMP_InputField _numberOfDays;
    [SerializeField] private TMP_InputField _newUsername;
    [SerializeField] private TMP_InputField _newPassword;

    private User _currentUser;
    private string _selectedUser;

    void Start()
    {
        RegisterNewUser("ADMIN", "test123", true);
        _isAdminToggle.onValueChanged.AddListener(ChangeUserType);
        _loginButton.onClick.AddListener(TryToLogin);

        _changePasswordFieldGo.SetActive(false);
        _confirmPasswordFieldGo.SetActive(false);
        _confirmChangePasswordButtonGo.SetActive(false);
        _userAfterLoggedIn.SetActive(false);
        _adminPanel.SetActive(false);
        _loginPanel.SetActive(true);
    }

    private void TryToLogin()
    {
        string storedPasswordHash = PlayerPrefs.GetString(_userName.text);

        if (string.IsNullOrEmpty(storedPasswordHash))
        {
            Debug.LogError("User not found!");
            return;
        }

        string hashedInputPassword = HashPassword(_password.text);

        if (hashedInputPassword == storedPasswordHash)
        {
            _errorText.text = "";
        }
        else
        {
            _errorText.text = "Wrong password or login";

            return;
        }

        _loginPanel.SetActive(false);
        _userAfterLoggedIn.SetActive(true);

        _currentUser = new User(_userName.text);
        _passwordMissmatchErrorText.text = "";

        HandlePanelView(_currentUser.UserType);
    }

    private void HandlePanelView(TypeOfUser p_userType)
    {
        _userAfterLoggedIn.SetActive(true);
        _loginPanel.SetActive(false);
        _changePasswordButton.onClick.AddListener(UnlockNewPasswordInputs);
        _logoutButton.onClick.AddListener(BackToLoginPanel);

        if (PlayerPrefs.GetInt(_userName.text + "firstLogin") == 1)
        {
            UnlockNewPasswordInputs();
            _passwordMissmatchErrorText.text = "First Login. Need to change password";
        }

        if (p_userType == TypeOfUser.User)
            return;

        HandleAdminPanelView();
    }

    private void HandleAdminPanelView()
    {
        _adminPanel.SetActive(true);

        List<OptionData> options = new List<OptionData>();
        var allUsers = PlayerPrefs.GetString("SavedUsersNames", "").Split(';');

        foreach (var userName in allUsers)
        {
            if (string.IsNullOrEmpty(userName) || userName == "ADMIN")
                continue;

            options.Add(new OptionData(userName));
        }

        _listOfUsers.AddOptions(options);
        _listOfUsers.onValueChanged.AddListener(SelectUser);

        _changeUserName.onClick.AddListener(ChangeSelectedUserName);
        _changeUserPassword.onClick.AddListener(ChangeSelectedUserPassword);
        _addNewUser.onClick.AddListener(AddNewUser);
        _deleteUser.onClick.AddListener(DeleteUser);
        _blockUser.onClick.AddListener(BlockUser);
        _setDayLimit.onClick.AddListener(SetDayLimit);
        _passwordRestriction.onValueChanged.AddListener(SetPasswordRestrictions);
    }

    private void SetPasswordRestrictions(bool p_isToggled)
    {
        if (p_isToggled)
        {
            PlayerPrefs.SetInt(_selectedUser + "restrictions", 1); // set restrictions
        }
        else
        {
            PlayerPrefs.SetInt(_selectedUser + "restrictions", 0); // remove restrictions
        }
    }

    private void SetDayLimit()
    {
        PlayerPrefs.SetInt(_selectedUser + "days", int.Parse(_numberOfDays.text));
    }

    private void BlockUser()
    {
        if (PlayerPrefs.GetFloat(_selectedUser) == 0) // not locked
        {
            PlayerPrefs.SetFloat(_selectedUser, 1);
            _blockUser.image.color = Color.red;
        }
        else
        { 
            PlayerPrefs.SetFloat(_selectedUser, 0);
            _blockUser.image.color = Color.green;
        }

        PlayerPrefs.Save();
    }

    private void DeleteUser()
    {
        PlayerPrefs.DeleteKey(_newUsername.text);
        PlayerPrefs.Save();

        _listOfUsers.options.Remove(new OptionData(_selectedUser));
    }

    private void ChangeSelectedUserName()
    {
        var userPassword = PlayerPrefs.GetString(_selectedUser);

        PlayerPrefs.DeleteKey(_listOfUsers.options.ElementAt(_listOfUsers.value).text);
        _listOfUsers.options.ElementAt(_listOfUsers.value).text = _newUsername.text;

        PlayerPrefs.SetString(_newUsername.text, userPassword);
        PlayerPrefs.Save();
    }

    private void ChangeSelectedUserPassword()
    {
        string hashedInputPassword = HashPassword(_newPassword.text);

        PlayerPrefs.SetString(_selectedUser, hashedInputPassword);
        PlayerPrefs.Save();
    }

    private void AddNewUser()
    {
        RegisterNewUser(_newUsername.text, _newPassword.text);

        _listOfUsers.AddOptions(new List<OptionData> { new OptionData(_newUsername.text) });
    }

    private void SelectUser(int p_selectedUserIndex)
    {
        _selectedUser = _listOfUsers.options.ElementAt(_listOfUsers.value).text;
    }

    private void BackToLoginPanel()
    {
        _currentUser = null;

        _loginPanel.SetActive(true);
        _userAfterLoggedIn.SetActive(false);
    }

    private void UnlockNewPasswordInputs()
    {
        _changePasswordFieldGo.SetActive(true);
        _confirmPasswordFieldGo.SetActive(true);
        _confirmChangePasswordButtonGo.SetActive(true);

        _confirmChangePasswordButton.onClick.AddListener(ChangePasswordOfCurrentUser);
    }

    private void ChangePasswordOfCurrentUser()
    {
        _passwordMissmatchErrorText.color = Color.red;

        if (PlayerPrefs.GetInt(_currentUser.UserName + "restrictions") == 1)
        {
            if (!CheckPasswordRestrictions(_confirmPasswordField.text))
            {
                _passwordMissmatchErrorText.text = "New password needs to have 14 symbols and atleast 1 number";
                return;
            }
        }

        if (_changePasswordField.text == _confirmPasswordField.text)
        {
            string hashedInputPassword = HashPassword(_confirmPasswordField.text);

            if (PlayerPrefs.GetString(_currentUser.UserName) == hashedInputPassword)
            {
                _passwordMissmatchErrorText.text = "New password cant be the same as ealier ones";
            }
            else
            {
                PlayerPrefs.SetString(_currentUser.UserName, HashPassword(_confirmPasswordField.text));
                PlayerPrefs.SetInt(_currentUser.UserName + "firstLogin", 0);
                PlayerPrefs.Save();
                _passwordMissmatchErrorText.text = "Passwords set";
                _passwordMissmatchErrorText.color = Color.green;
            }
        }
        else
        {
            _passwordMissmatchErrorText.text = "Passwords missmatch";
        }
    }

    private void ChangeUserType(bool p_isAdmin)
    {
        if (p_isAdmin)
        {
            _userName.text = "ADMIN";
            _userName.readOnly = true;
        }
        else
        {
            _userName.readOnly = false;
        }
    }

    public void RegisterNewUser(string p_username, string p_password, bool p_asAdmin = false)
    {
        string hashedPassword;
        string updatedKeys = null;
        string username = null;

        if (p_asAdmin)
        {
            hashedPassword = HashPassword("test123");

            PlayerPrefs.SetString("ADMIN", hashedPassword);
            PlayerPrefs.Save();

            username = "ADMIN";
        }
        else
        {
            hashedPassword = HashPassword(p_password);

            PlayerPrefs.SetString(p_username, hashedPassword);
            PlayerPrefs.SetInt(p_username + "firstLogin", 1); // 1 == true
            PlayerPrefs.Save(); // saving single user with password

            username = p_username;
        }

        string existingKeys = PlayerPrefs.GetString("SavedUsersNames", "");

        if (!existingKeys.Contains(username))// might need to add ; for checking similar usernames
            updatedKeys = existingKeys + (string.IsNullOrEmpty(existingKeys) ? "" : ";") + username;
        else
            return;

        if (string.IsNullOrEmpty(updatedKeys))
            Debug.LogError("Huge error in registering");

        PlayerPrefs.SetFloat(_selectedUser, 0);
        PlayerPrefs.SetString("SavedUsersNames", updatedKeys);
        PlayerPrefs.Save(); // saving full list of users 
    }

    private string HashPassword(string password)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(bytes);

        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    private bool CheckPasswordRestrictions(string p_password)
    {
        if (string.IsNullOrEmpty(p_password))
        {
            return false;
        }

        if (p_password.Length < 14)
        {
            return false;
        }

        if (!Regex.IsMatch(p_password, @"\d"))
        {
            return false;
        }

        return true;
    }
}
