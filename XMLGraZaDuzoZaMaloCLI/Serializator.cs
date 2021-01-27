using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using GraZaDuzoZaMalo.Model;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;


namespace AppGraZaDuzoZaMaloCLI
{
    class Serializator
    {
        DataContractSerializer ser = new DataContractSerializer(typeof(Gra));
        Aes key = Aes.Create();


        bool czyIstnieje { get => File.Exists("zapis.xml"); }

        public void Serialize(Gra gra)
        {
            if (czyIstnieje)
            {
                Decrypt();
            }
          using(var stream = new FileStream("zapis.xml", FileMode.Create, FileAccess.Write))
            {
                ser.WriteObject(stream, gra);        
            }
            Encrypt();
        }

        public Gra Deserialize()
        {
            if(czyIstnieje)
            {
                Decrypt();
                
                using (var stream = new FileStream("zapis.xml", FileMode.Open, FileAccess.Read))
                {
                    return (Gra)ser.ReadObject(stream);
                }
                
                
            }
            else
            {
                throw new SerializationException();
            }


        }

        public void Encrypt()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load("zapis.xml");

            XmlElement elementToEncrypt = xmlDoc.GetElementsByTagName("liczbaDoOdgadniecia")[0] as XmlElement;

            EncryptedXml eXml = new EncryptedXml();

            byte[] encryptedElement = eXml.EncryptData(elementToEncrypt, key, false);

            EncryptedData edElement = new EncryptedData();
            edElement.Type = EncryptedXml.XmlEncElementUrl;

            string encryptionMethod = null;
            if(key is Aes)
            {
                encryptionMethod = EncryptedXml.XmlEncAES256Url;
            }
            else
            {
                throw new CryptographicException("Blad crypto");
            }

            edElement.EncryptionMethod = new EncryptionMethod(encryptionMethod);

            edElement.CipherData.CipherValue = encryptedElement;

            EncryptedXml.ReplaceElement(elementToEncrypt, edElement, false);
            using(var stream = new FileStream("zapis.xml", FileMode.Create, FileAccess.Write))
            {
                xmlDoc.Save(stream);
            }
        }

        public void Decrypt()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load("zapis.xml");
            XmlElement encryptedElement = xmlDoc.GetElementsByTagName("EncryptedData")[0] as XmlElement;

            if (encryptedElement == null)
            {
                throw new XmlException("The EncryptedData element was not found.");
            }

            EncryptedData edElement = new EncryptedData();
            edElement.LoadXml(encryptedElement);

            EncryptedXml exml = new EncryptedXml();

            byte[] rgbOutput = exml.DecryptData(edElement, key);
            exml.ReplaceData(encryptedElement, rgbOutput);

            using (var stream = new FileStream("zapis.xml", FileMode.Create, FileAccess.Write))
            {
                xmlDoc.Save(stream);
            }
        }
    }
}
 