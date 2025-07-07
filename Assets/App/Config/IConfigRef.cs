namespace App.Config
{
    public interface IConfigRef
    {
        void Init(ConfigManager configManager);
        void PostProcessData(ConfigManager configManager);
    }
}
