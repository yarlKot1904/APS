using System.Text.Json;

namespace APS.Helper
{


    public static class ConfigLoader
    {
        public static SimulationConfig LoadConfig(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                MessageBox.Show($"Конфигурационный файл не найден: {configFilePath}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            try
            {
                string jsonString = File.ReadAllText(configFilePath);
                ConfigRoot configRoot = JsonSerializer.Deserialize<ConfigRoot>(jsonString);

                if (configRoot == null || configRoot.SimulationConfig == null)
                {
                    throw new Exception("Неверная структура конфигурационного файла.");
                }

                return configRoot.SimulationConfig;
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Ошибка при чтении конфигурационного файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
                return null; 
            }
        }
    }

}
