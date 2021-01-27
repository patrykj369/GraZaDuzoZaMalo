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
        Aes klucz;

        public Serializator()
        {
            klucz = Aes.Create();
            klucz.Key = new byte[16] { 5, 6, 7, 3, 4, 6, 15, 16, 61, 61, 11, 19, 12, 15, 6, 5 };
        }


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
                Gra gra = null;
                using (var stream = new FileStream("zapis.xml", FileMode.Open, FileAccess.Read))
                {
                    gra = (Gra)ser.ReadObject(stream);
                }
                Encrypt();
                return gra;
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

            byte[] encryptedElement = eXml.EncryptData(elementToEncrypt, klucz, false);

            EncryptedData edElement = new EncryptedData();
            edElement.Type = EncryptedXml.XmlEncElementUrl;

            string encryptionMethod = null;
            if(klucz is Aes)
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

            byte[] rgbOutput = exml.DecryptData(edElement, klucz);
            exml.ReplaceData(encryptedElement, rgbOutput);

            using (var stream = new FileStream("zapis.xml", FileMode.Create, FileAccess.Write))
            {
                xmlDoc.Save(stream);
            }
        }
    }
}
 