using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace Snake_Game
{
    static class GameData
    {
        private static void LoadHighscoreList(ObservableCollection<SnakeHighscore> HighscoreList)
        {
            if (File.Exists("snake_highscorelist.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<SnakeHighscore>));
                using (Stream reader = new FileStream("snake_highscorelist.xml", FileMode.Open))
                {
                    List<SnakeHighscore> tempList = (List<SnakeHighscore>)serializer.Deserialize(reader);
                    HighscoreList.Clear();
                    foreach (var item in tempList.OrderByDescending(x => x.Score))
                        HighscoreList.Add(item);
                }
            }
        }
    }
}
