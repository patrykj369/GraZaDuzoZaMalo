using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using GraZaDuzoZaMalo.Model;
using System.Runtime.Serialization.Formatters.Binary;

namespace AppGraZaDuzoZaMaloCLI
{
    class Serializator
    {
        BinaryFormatter formatter = new BinaryFormatter();
        public void BinarySerialize(Gra gra)
        {
          using(var stream = new FileStream("zapis.save", FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(stream, gra);       
            }
        }

        public Gra BinaryDeserialize()
        {
            using(var stream = new FileStream("zapis.save", FileMode.Open, FileAccess.Read))
            {
                return (Gra)formatter.Deserialize(stream);
            }
        }
    }
}
 