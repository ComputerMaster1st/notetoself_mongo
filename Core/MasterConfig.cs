using System;
using System.IO;
using System.Xml.Serialization;

namespace notetoself_mongo
{
    [XmlRoot("masterconfig")]
    public class MasterConfig {
        [XmlElement("token")]
        public string Token { get; set; }
        [XmlElement("mongouser")]
        public string MongoUser { get; set; }
        [XmlElement("mongopass")]
        public string MongoPass { get; set; }

        MasterConfig() {}

        public MasterConfig(string Token, string MongoUser, string MongoPass) {
            this.Token = Token;
            this.MongoUser = MongoUser;
            this.MongoPass = MongoPass;
        }

        public static MasterConfig Load() {
            if (File.Exists("notetoself-config.xml")) {
                XmlSerializer deserializer = new XmlSerializer(typeof(MasterConfig));
                MasterConfig Config;

                using (TextReader reader = new StreamReader("notetoself-config.xml")) {
                    object RawConfig = deserializer.Deserialize(reader);
                    Config = (MasterConfig)RawConfig;
                }

                return Config;

            } else return null;
        }

        public void Save() {
            XmlSerializer serializer = new XmlSerializer(typeof(MasterConfig));

            using (TextWriter writer = new StreamWriter("notetoself-config.xml")) {
                serializer.Serialize(writer, this); 
            }
        }

        public static MasterConfig Setup() {
            Console.Write("Token: ");
            string Token = Console.ReadLine();

            Console.Write("MongoDB User: ");
            string MongoUser = Console.ReadLine();

            Console.Write("MongoDB Pass: ");
            string MongoPass = Console.ReadLine();

            MasterConfig Config = new MasterConfig(Token, MongoUser, MongoPass);

            return Config;
        }
    }
}