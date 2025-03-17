using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using UnityEngine;

[System.Serializable]
public class EnemyXMLData
{
    [XmlAttribute("name")]
    public string enemyName;

    [XmlAttribute("count")]
    public int count;
}

[System.Serializable]
public class WaveXMLData
{
    [XmlAttribute("name")]
    public string waveName;

    [XmlAttribute("type")]
    public string waveType;

    [XmlElement("Enemy")]
    public List<EnemyXMLData> enemyGroups = new List<EnemyXMLData>();
}

[System.Serializable]
[XmlRoot("WaveConfig")]
public class WaveConfigXML
{
    [XmlElement("Wave")]
    public List<WaveXMLData> waves = new List<WaveXMLData>();

    public void SaveToFile(string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(WaveConfigXML));
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            serializer.Serialize(writer, this);
        }
    }

    public static WaveConfigXML LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"파일 {filePath}을 찾을 수 없습니다.");
            return null;
        }

        XmlSerializer serializer = new XmlSerializer(typeof(WaveConfigXML));
        using (StreamReader reader = new StreamReader(filePath))
        {
            return (WaveConfigXML)serializer.Deserialize(reader);
        }
    }
}
