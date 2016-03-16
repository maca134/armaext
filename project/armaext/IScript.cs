namespace armaext
{
    interface IScript
    {
        string Invoke(string input);
        void Load(string script);
    }
}
