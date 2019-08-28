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
    class FileReader
    {
        // The file to hold all game data
        private string path = "snake_highscorelist.xml";

        /// <summary>
        /// Load the current highscore list
        /// </summary>
        /// <param name="HighscoreList"></param>
        public void LoadHighscoreList(ObservableCollection<SnakeHighscore> HighscoreList)
        {
            if (File.Exists(path))
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

        /// <summary>
        /// Save the current highscore list
        /// </summary>
        /// <param name="HighscoreList"></param>
        public void SaveHighscoreList(ObservableCollection<SnakeHighscore> HighscoreList)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<SnakeHighscore>));
            using (Stream writer = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(writer, HighscoreList);
            }
        }
    }
}
