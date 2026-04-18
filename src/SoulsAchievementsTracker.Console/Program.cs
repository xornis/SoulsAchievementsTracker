using System.Security.Cryptography;
using System.Text;
using SoulsFormats;

namespace SoulsAchievementsTracker
{
    internal class Program
    {
        private static readonly byte[] AES_KEY = new byte[]
        {
            0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
            0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10
        };

        private static void Main(string[] args)
        {
            string saveFilePath = FindSaveFile();

            if (saveFilePath == null)
            {
                Console.WriteLine("No save files found!");
                Console.WriteLine("Make sure Dark Souls Remastered is installed and you have at least one character.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Found save file: {saveFilePath}\n");

            byte[] saveData = File.ReadAllBytes(saveFilePath);
            BND4 bnd = BND4.Read(saveData);

            var userDataFile = bnd.Files.Find(f => f.Name == "USER_DATA001");
            if (userDataFile == null)
            {
                Console.WriteLine("Character slot 1 is empty!");
                Console.ReadKey();
                return;
            }

            byte[] decrypted = DecryptUserData(userDataFile.Bytes);
            var character = new DSRCharacter(decrypted);

            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║        CHARACTER INFORMATION           ║");
            Console.WriteLine("╚════════════════════════════════════════╝\n");

            Console.WriteLine($"Name:         {character.Name}");
            Console.WriteLine($"Level:        {character.Level}");
            Console.WriteLine($"Souls:        {character.Souls:N0}");
            Console.WriteLine($"Humanity:     {character.Humanity}");
            Console.WriteLine($"Gender:       {(character.IsMale ? "Male" : "Female")}");
            Console.WriteLine($"Class:        {character.GetClassName()}");
            Console.WriteLine();
            Console.WriteLine("═══ STATS ═══");
            Console.WriteLine($"Vitality:     {character.Vitality}");
            Console.WriteLine($"Attunement:   {character.Attunement}");
            Console.WriteLine($"Endurance:    {character.Endurance}");
            Console.WriteLine($"Strength:     {character.Strength}");
            Console.WriteLine($"Dexterity:    {character.Dexterity}");
            Console.WriteLine($"Resistance:   {character.Resistance}");
            Console.WriteLine($"Intelligence: {character.Intelligence}");
            Console.WriteLine($"Faith:        {character.Faith}");
        }

        private static string FindSaveFile()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dsrBasePath = Path.Combine(documentsPath, "NBGI", "DARK SOULS REMASTERED");

            if (!Directory.Exists(dsrBasePath))
                return null;

            var steamIdFolders = Directory.GetDirectories(dsrBasePath);

            if (steamIdFolders.Length == 0)
                return null;

            var allSaveFiles = steamIdFolders
                .SelectMany(folder => Directory.GetFiles(folder, "*.sl2"))
                .Select(filePath => new FileInfo(filePath))
                .Where(fileInfo => fileInfo.Exists)
                .ToList();

            if (allSaveFiles.Count == 0)
                return null;

            var newestSave = allSaveFiles
                .OrderByDescending(f => f.LastWriteTime)
                .First();

            return newestSave.FullName;
        }

        private static byte[] DecryptUserData(byte[] encryptedData)
        {
            byte[] iv = new byte[16];
            Array.Copy(encryptedData, 0, iv, 0, 16);

            byte[] cipherText = new byte[encryptedData.Length - 16];
            Array.Copy(encryptedData, 16, cipherText, 0, cipherText.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Key = AES_KEY;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
                }
            }
        }
    }

    public class DSRCharacter
    {
        private readonly byte[] data;
        
        private const int OFFSET_STAMINA = 0x90;       
        private const int OFFSET_VITALITY = 0xA0;       
        private const int OFFSET_ATTUNEMENT = 0xA8;     
        private const int OFFSET_ENDURANCE = 0xB0;      
        private const int OFFSET_STRENGTH = 0xB8;       
        private const int OFFSET_DEXTERITY = 0xC0;      
        private const int OFFSET_INTELLIGENCE = 0xC8;   
        private const int OFFSET_FAITH = 0xD0;          
        private const int OFFSET_HUMANITY = 0xE0;       
        private const int OFFSET_RESISTANCE = 0xE8;     
        private const int OFFSET_LEVEL = 0xF0;          
        private const int OFFSET_SOULS = 0xF4;          
        private const int OFFSET_NAME = 0x108;          
        private const int OFFSET_GENDER = 0x12A;        
        private const int OFFSET_CLASS = 0x12E;         

        public DSRCharacter(byte[] data)
        {
            this.data = data;
        }

        public string Name => ReadString(OFFSET_NAME, 32);
        public int Level => ReadInt32(OFFSET_LEVEL);
        public int Souls => ReadInt32(OFFSET_SOULS);
        public int Stamina => ReadInt32(OFFSET_STAMINA);
        public long Vitality => ReadInt64(OFFSET_VITALITY);
        public long Attunement => ReadInt64(OFFSET_ATTUNEMENT);
        public long Endurance => ReadInt64(OFFSET_ENDURANCE);
        public long Strength => ReadInt64(OFFSET_STRENGTH);
        public long Dexterity => ReadInt64(OFFSET_DEXTERITY);
        public long Intelligence => ReadInt64(OFFSET_INTELLIGENCE);
        public long Faith => ReadInt64(OFFSET_FAITH);
        public long Resistance => ReadInt64(OFFSET_RESISTANCE);
        public long Humanity => ReadInt64(OFFSET_HUMANITY);
        public bool IsMale => ReadByte(OFFSET_GENDER) == 1;
        public byte ClassId => ReadByte(OFFSET_CLASS);

        public string GetClassName()
        {
            string[] classes = new[]
            {
                "Warrior", "Knight", "Wanderer", "Thief", "Bandit",
                "Hunter", "Sorcerer", "Pyromancer", "Cleric", "Deprived"
            };

            return ClassId < classes.Length ? classes[ClassId] : $"Unknown ({ClassId})";
        }

        private string ReadString(int offset, int size)
        {
            if (offset + size > data.Length) return "";
            return Encoding.Unicode.GetString(data, offset, size).TrimEnd('\0');
        }

        private int ReadInt32(int offset)
        {
            if (offset + 4 > data.Length) return 0;
            return BitConverter.ToInt32(data, offset);
        }

        private long ReadInt64(int offset)
        {
            if (offset + 8 > data.Length) return 0;
            return BitConverter.ToInt64(data, offset);
        }

        private byte ReadByte(int offset)
        {
            if (offset >= data.Length) return 0;
            return data[offset];
        }
    }
}