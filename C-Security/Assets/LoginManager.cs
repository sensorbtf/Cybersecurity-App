using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static LogsPopup;
using static TMPro.TMP_Dropdown;

public class LoginManager : MonoBehaviour
{
    //Login Panel
    [SerializeField] private LogsPopup _logsPopup;
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private GameObject _userAfterLoggedIn;
    [SerializeField] private GameObject _adminPanel;
    [SerializeField] private Button _loginButton;
    [SerializeField] private TMP_InputField _userName;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private TMP_InputField _oneUsePassword;
    [SerializeField] private TextMeshProUGUI _errorText;
    [SerializeField] private TextMeshProUGUI _randomNumberForPassword;

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
    [SerializeField] private Button _showLogsButton;
    [SerializeField] private Button _changeUserPassword;
    [SerializeField] private Button _addNewUser;
    [SerializeField] private Button _deleteUser;
    [SerializeField] private Button _blockUser;
    [SerializeField] private Button _setDayLimit;
    [SerializeField] private Button _createOneUsePassword;
    [SerializeField] private Toggle _passwordRestriction;
    [SerializeField] private TMP_InputField _numberOfDays;
    [SerializeField] private TMP_InputField _newUsername;
    [SerializeField] private TMP_InputField _newPassword;

    private User _currentUser;
    private string _selectedUser;
    private int _randomNumber;

    void Start()
    {
        RegisterNewUser("ADMIN", "test123", true);
        _loginButton.onClick.AddListener(TryToLogin);

        BackToLoginPanel();
    }

    private void TryToLogin()
    {
        if (PlayerPrefs.GetFloat(_userName.text + "isBlocked") == 1) // not locked
        {
            _errorText.text = "User is blocked!";
            LogActivity(_userName.text, Activity.Login, false);
            return;
        }

        if (!string.IsNullOrEmpty(_oneUsePassword.text) && PlayerPrefs.GetInt(_userName.text + "oneUsePassword") == 1)
        {
            if (!TryToLoginByOneUsePsswd())
            {
                return;
            }
        }
        else
        {
            string storedPasswordHash = PlayerPrefs.GetString(_userName.text + "_password");

            if (string.IsNullOrEmpty(storedPasswordHash))
            {
                _errorText.text = "User not found!";
                LogActivity(_userName.text, Activity.Login, false);
                return;
            }

            string hashedInputPassword = HashPassword(_password.text);


            if (hashedInputPassword == storedPasswordHash)
            {
                _errorText.text = "";
                LogActivity(_userName.text, Activity.Login, true);
            }
            else
            {

                _errorText.text = "Wrong password or login";
                LogActivity(_userName.text, Activity.Login, false);
                return;
            }
        }

        _currentUser = new User(_userName.text);
        _passwordMissmatchErrorText.text = "";

        HandlePanelView(_currentUser.UserType);
    }

    private bool TryToLoginByOneUsePsswd()
    {
        int numberOfLetters = _userName.text.Length;
        var rightPassword = GetOneUsePassword(numberOfLetters, _randomNumber);
        bool isValidNumber = double.TryParse(_oneUsePassword.text, out double enteredPassword);

        Debug.Log($"PSWD {rightPassword}");

        double tolerance = 0.02; 
        if (isValidNumber && Math.Abs(enteredPassword - rightPassword) < tolerance)
        {
            return true;
        }
        else
        {
            _randomNumberForPassword.text = "Wrong one use password";
            return false;
        }
    }

    private void HandlePanelView(TypeOfUser p_userType)
    {
        _userAfterLoggedIn.SetActive(true);
        _loginPanel.SetActive(false);

        UnlockNewPasswordInputs();
        _logoutButton.onClick.RemoveAllListeners();

        _logoutButton.onClick.AddListener(BackToLoginPanel);

        if (PlayerPrefs.GetInt(_userName.text + "firstLogin") == 1)
        {
            _passwordMissmatchErrorText.text = "First Login. Need to change password";
        }

        if (HasDatePassed(_userName.text))
        {
            _passwordMissmatchErrorText.text = "Password expired. Need to change";
        }

        if (p_userType == TypeOfUser.User)
        {
            _adminPanel.SetActive(false);
            return;
        }

        HandleAdminPanelView();
    }

    private void HandleAdminPanelView()
    {
        _adminPanel.SetActive(true);

        _changeUserName.onClick.RemoveAllListeners();
        _changeUserPassword.onClick.RemoveAllListeners();
        _addNewUser.onClick.RemoveAllListeners();
        _deleteUser.onClick.RemoveAllListeners();
        _blockUser.onClick.RemoveAllListeners();
        _setDayLimit.onClick.RemoveAllListeners();
        _createOneUsePassword.onClick.RemoveAllListeners();
        _showLogsButton.onClick.RemoveAllListeners();
        _passwordRestriction.onValueChanged.RemoveAllListeners();

        _changeUserName.onClick.AddListener(ChangeSelectedUserName);
        _changeUserPassword.onClick.AddListener(ChangeSelectedUserPassword);
        _addNewUser.onClick.AddListener(AddNewUser);
        _deleteUser.onClick.AddListener(DeleteUser);
        _blockUser.onClick.AddListener(BlockUser);
        _setDayLimit.onClick.AddListener(SetDayLimit);
        _createOneUsePassword.onClick.AddListener(CreateOneUsePassword);
        _showLogsButton.onClick.AddListener(_logsPopup.ShowPopup);
        _passwordRestriction.onValueChanged.AddListener(SetPasswordRestrictions);

        RefreshListOfUsers();
    }

    private void BackToLoginPanel()
    {
        _currentUser = null;
        _randomNumber = UnityEngine.Random.Range(0, 100);
        _randomNumberForPassword.text = _randomNumber.ToString();

        _adminPanel.SetActive(false);
        _userAfterLoggedIn.SetActive(false);
        _loginPanel.SetActive(true);
    }

    private void RefreshListOfUsers()
    {
        List<OptionData> options = new List<OptionData>();
        var allUsers = PlayerPrefs.GetString("SavedUsersNames", "").Split(';');

        foreach (var userName in allUsers)
        {
            if (string.IsNullOrEmpty(userName) || userName == "ADMIN")
                continue;

            options.Add(new OptionData(userName));
        }


        _listOfUsers.ClearOptions();
        _listOfUsers.onValueChanged.RemoveAllListeners();

        _listOfUsers.AddOptions(options);
        _listOfUsers.onValueChanged.AddListener(SelectUser);

        SelectUser(0);
    }

    private void SetPasswordRestrictions(bool p_isToggled)
    {
        if (p_isToggled)
        {
            PlayerPrefs.SetInt(_selectedUser + "restrictions", 1); // set restrictions
            LogActivity(_selectedUser, Activity.PasswordRestictionSet, true);
        }
        else
        {
            PlayerPrefs.SetInt(_selectedUser + "restrictions", 0); // remove restrictions
            LogActivity(_selectedUser, Activity.PasswordRestictionRemoval, true);
        }
    }

    private void SetDayLimit()
    {
        int daysToAdd = int.Parse(_numberOfDays.text);

        DateTime futureDate = DateTime.Now.AddDays(daysToAdd);

        string formattedDate = futureDate.Day + "-" + futureDate.Month;
        PlayerPrefs.SetString(_selectedUser + "LimitDate", formattedDate);
        LogActivity(_selectedUser, Activity.SetDayLimit, true);
    }

    private void BlockUser()
    {
        if (PlayerPrefs.GetFloat(_selectedUser + "isBlocked") == 0) // not locked
        {
            PlayerPrefs.SetFloat(_selectedUser + "isBlocked", 1);
            LogActivity(_selectedUser, Activity.UserBlocked, true);
            _blockUser.image.color = Color.red;
        }
        else
        {
            PlayerPrefs.SetFloat(_selectedUser + "isBlocked", 0);
            LogActivity(_selectedUser, Activity.UserUnlocked, true);
            _blockUser.image.color = Color.green;
        }

        PlayerPrefs.Save();
    }

    private void DeleteUser()
    {
        PlayerPrefs.DeleteKey(_selectedUser + "_password");
        PlayerPrefs.Save();

        HandleUserDeletion(_selectedUser);
        RefreshListOfUsers();
        SelectUser(0);

        LogActivity(_selectedUser, Activity.UserRemoval, true);
    }

    private void ChangeSelectedUserName()
    {
        var userPassword = PlayerPrefs.GetString(_selectedUser + "_password");
        var oldUserName = _listOfUsers.options.ElementAt(_listOfUsers.value).text;

        PlayerPrefs.DeleteKey(oldUserName + "_password");
        _listOfUsers.options.ElementAt(_listOfUsers.value).text = _newUsername.text;
        _selectedUser = _newUsername.text;

        PlayerPrefs.SetString(_selectedUser + "_password", userPassword);
        PlayerPrefs.Save();

        HandleUserDeletion(oldUserName);
        HandleUserChangeOrAdd(_selectedUser);
        LogActivity(_selectedUser, Activity.UsernameChanged, true);
    }

    private void ChangeSelectedUserPassword()
    {
        string hashedInputPassword = HashPassword(_newPassword.text);

        PlayerPrefs.SetString(_selectedUser + "_password", hashedInputPassword);
        PlayerPrefs.Save();
        LogActivity(_selectedUser, Activity.PasswordChange, true);

    }

    private void AddNewUser()
    {
        string existingKeys = PlayerPrefs.GetString("SavedUsersNames", "");

        if (existingKeys == null)
            return;

        if (existingKeys.Contains(_newUsername.text))
            return;

        RegisterNewUser(_newUsername.text, _newPassword.text);
        _listOfUsers.AddOptions(new List<OptionData> { new OptionData(_newUsername.text) });
        var index = _listOfUsers.options.Count;

        if (index == 0)
            SelectUser(index);
        else
            SelectUser(index - 1);
    }

    private void SelectUser(int p_selectedUserIndex)
    {
        _selectedUser = _listOfUsers.options.ElementAt(p_selectedUserIndex).text;

        if (PlayerPrefs.GetFloat(_selectedUser + "isBlocked") == 0) // not locked
        {
            _blockUser.image.color = Color.green;
        }
        else
        {
            _blockUser.image.color = Color.red;
        }

        if (PlayerPrefs.GetInt(_selectedUser + "restrictions") == 1)
        {
            _passwordRestriction.isOn = true;
        }
        else
        {
            _passwordRestriction.isOn = false;
        }
    }

    private void UnlockNewPasswordInputs()
    {
        _confirmChangePasswordButton.onClick.RemoveAllListeners();
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
                LogActivity(_currentUser.UserName, Activity.PasswordChange, false);
                return;
            }
        }

        if (_changePasswordField.text == _confirmPasswordField.text)
        {
            string hashedInputPassword = HashPassword(_confirmPasswordField.text);

            if (PlayerPrefs.GetString(_currentUser.UserName + "_password") == hashedInputPassword)
            {
                _passwordMissmatchErrorText.text = "New password cant be the same as ealier ones";
            }
            else
            {
                PlayerPrefs.SetString(_currentUser.UserName + "_password", HashPassword(_confirmPasswordField.text));
                PlayerPrefs.SetInt(_currentUser.UserName + "firstLogin", 0);
                PlayerPrefs.Save();
                _passwordMissmatchErrorText.text = "Passwords set";

                _passwordMissmatchErrorText.color = Color.green;
                LogActivity(_currentUser.UserName, Activity.PasswordChange, true);
            }
        }
        else
        {
            _passwordMissmatchErrorText.text = "Passwords missmatch";
            LogActivity(_currentUser.UserName, Activity.PasswordChange, false);
        }


    }

    public void RegisterNewUser(string p_username, string p_password, bool p_asAdmin = false)
    {
        string hashedPassword;
        string username;

        if (p_asAdmin)
        {
            hashedPassword = HashPassword("test123");

            PlayerPrefs.SetString("ADMIN" + "_password", hashedPassword);
            PlayerPrefs.Save();

            username = "ADMIN";
        }
        else
        {
            hashedPassword = HashPassword(p_password);

            PlayerPrefs.SetString(p_username + "_password", hashedPassword);
            PlayerPrefs.SetInt(p_username + "firstLogin", 1); // 1 == true
            PlayerPrefs.SetInt(p_username + "oneUsePassword", 0); // 0 == false
            PlayerPrefs.Save(); // saving single user with password

            username = p_username;
        }

        HandleUserChangeOrAdd(username);

        LogActivity(username, Activity.UserCreation, true);
        Debug.Log(PlayerPrefs.GetString(username + "_password"));
    }

    private void CreateOneUsePassword()
    {
        PlayerPrefs.SetInt(_selectedUser + "oneUsePassword", 1); //  1 == true
        PlayerPrefs.Save(); // saving single user with password
    }

    private void HandleUserChangeOrAdd(string username)
    {
        string existingKeys = PlayerPrefs.GetString("SavedUsersNames", "");

        string updatedKeys;
        if (!existingKeys.Contains(username))// might need to add ; for checking similar usernames
            updatedKeys = existingKeys + (string.IsNullOrEmpty(existingKeys) ? "" : ";") + username;
        else
            return;

        if (string.IsNullOrEmpty(updatedKeys))
            Debug.LogError("Huge error in registering");

        PlayerPrefs.SetFloat(username + "isBlocked", 0);
        PlayerPrefs.SetString("SavedUsersNames", updatedKeys);
        PlayerPrefs.Save(); // saving full list of users 
    }

    private void HandleUserDeletion(string username)
    {
        string existingKeys = PlayerPrefs.GetString("SavedUsersNames", "");

        if (existingKeys == null)
            return;

        if (!existingKeys.Contains(username))
            return;

        List<string> usersList = new List<string>(existingKeys.Split(';'));

        usersList.Remove(username);

        string updatedKeys = string.Join(";", usersList);

        if (string.IsNullOrEmpty(updatedKeys))
            Debug.LogError("Huge error in registering");


        PlayerPrefs.SetString("SavedUsersNames", updatedKeys);
        PlayerPrefs.Save();
    }

    private string HashPassword(string password)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(bytes);

        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    private double GetOneUsePassword(double p_x, double p_a)
    {
        if (p_x <= 0 || p_a <= 0)
            throw new ArgumentOutOfRangeException("Arguments must be greater than zero.");

        return Math.Round(Math.Log10(p_x / p_a),3);
    }

    private bool HasDatePassed(string p_username)
    {
        string savedDateString = PlayerPrefs.GetString(p_username + "LimitDate", "");

        if (string.IsNullOrEmpty(savedDateString))
            return false;

        string[] parts = savedDateString.Split('-');
        if (parts.Length != 2) return false;

        int savedDay = int.Parse(parts[0]);
        int savedMonth = int.Parse(parts[1]);

        DateTime savedDate = new DateTime(DateTime.Now.Year, savedMonth, savedDay);

        if (DateTime.Now.Date >= savedDate.Date)
            return true;

        return false;
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

    private void LogActivity(string p_userName, Activity typeOfActivity, bool p_doneRight)
    {
        var logData = new LogData
        {
            UserName = p_userName,
            DateTime = DateTime.Now,
            TypeOfActivity = typeOfActivity,
            WasSuccessfull = p_doneRight
        };

        string logFilePath = Path.Combine(Application.persistentDataPath, "activity_log.json");

        LogDataList logList = new LogDataList { logDataList = new List<LogData>() };
        if (File.Exists(logFilePath))
        {
            string existingJson = File.ReadAllText(logFilePath);
            logList = JsonUtility.FromJson<LogDataList>(existingJson) ?? logList;
        }

        logList.logDataList.Add(logData);
        string jsonToSave = JsonUtility.ToJson(logList);
        File.WriteAllText(logFilePath, jsonToSave);

        Debug.Log($"Logged activity for user {p_userName}");
    }

}

[Serializable]
public struct LogData
{
    public string UserName;
    public DateTime DateTime;
    public Activity TypeOfActivity;
    public bool WasSuccessfull;
}

[System.Serializable]
public class LogDataList
{
    public List<LogData> logDataList;
}

public enum Activity
{
    Login = 0, Logout = 1, PasswordChange = 2, UserCreation = 3, UsernameChanged = 4,
    UserTypeChange = 5, PasswordRestictionSet = 6, PasswordRestictionRemoval = 7,
    SetDayLimit = 8,
    UserUnlocked = 9,
    UserBlocked = 10,
    UserRemoval = 11,
}