public class User
{
    private TypeOfUser _userType;
    private string _username;

    private bool _isFirstLogin;

    public User(string p_userName)
    {
        _username = p_userName;

        if (_username == "ADMIN")
        {
            _userType = TypeOfUser.ADMIN;
        }
        else
        {
            _userType = TypeOfUser.User;
        }
    }

    public TypeOfUser UserType
    {
        get => _userType;
    }

    public string UserName
    {
        get => _username;
        set => _username = value;
    }
}

public enum TypeOfUser
{ ADMIN, User }