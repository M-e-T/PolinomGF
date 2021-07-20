using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using GFStochasticMono.Interface;

namespace GFStochasticMono.Model
{
    public class SerializeSettings : ISerialize
    {
        private const string FileName = "Setting.xml";
        public void SaveSettings(SaveSettings saveSettings)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (var file = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                formatter.Serialize(file, saveSettings);
            }
        }
        public SaveSettings OpenSettings()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            if (File.Exists(FileName))
            {
                using (var file = new FileStream(FileName, FileMode.Open))
                {
                    SaveSettings settings = formatter.Deserialize(file) as SaveSettings;
                    return settings;
                }
            }
            else
                return new SaveSettings();
        }
    }
}
