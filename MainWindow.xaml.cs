using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Snake_Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Private constant fields
        private const int SnakeSquareSize = 20;
        private const int SnakeStartLength = 3;
        private const int SnakeStartSpeed = 400;
        private const int SnakeSpeedThreshold = 100;
        private const int MaxHighscoreListEntryCount = 5;

        // Snake description properties
        private SolidColorBrush snakeBodyBrush = Brushes.Green;
        private SolidColorBrush snakeHeadBrush = Brushes.YellowGreen;
        private List<SnakePart> snakeParts = new List<SnakePart>();

        // Food description properties
        private UIElement snakeFood = null;
        private SolidColorBrush foodBrush = Brushes.Red;

        // Game status properties
        public enum SnakeDirection {  Left, Right, Up, Down };
        private SnakeDirection snakeDirection = SnakeDirection.Right;
        private int snakeLength;
        private int currentScore = 0;
        private Label currentLabel = null;
        private int highScore;

        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();
        private Random rnd = new Random();

        public ObservableCollection<SnakeHighscore> HighscoreList {get; set; } = new ObservableCollection<SnakeHighscore>();

        public MainWindow()
        {
            InitializeComponent();
            gameTickTimer.Tick += GameTickTimer_Tick;
            LoadHighscoreList();
        }

        /********************************
         * EVENT HANDLERS
        *******************************/

        /// <summary>
        /// Event handler for when window is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Actions to be performed when window is loaded
            DrawGameArea();
        }

        /// <summary>
        /// Event handler for GameWindow Mouse Down - Drag the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Event handler for close button - Close the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Event handler for BtnShowScoreList Click - Close welcome 
        /// Message and open highscore list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShowighScoreList_Click(object sender, RoutedEventArgs e)
        {
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// EVent handler for BtnAddToHighscoreList Click - Add a new highscore entry
        /// and open the highscore list 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddToHighscoreList_Click(object sender, RoutedEventArgs e)
        {
            int newIndex = 0;
            // Figure out where new entry should be inserted
            if (HighscoreList.Count > 0 && currentScore < HighscoreList.Max(x => x.Score))
            {
                SnakeHighscore justAbove = HighscoreList.OrderByDescending(x => x.Score).First(x => x.Score >= currentScore);
                if (justAbove != null)
                    newIndex = HighscoreList.IndexOf(justAbove) + 1;
            }

            // Create and insert the new entry
            HighscoreList.Insert(newIndex, new SnakeHighscore()
            {
                PlayerName = txtPlayerName.Text,
                Score = currentScore
            });

            // Make sure number of entries does not exceed maximum
            while (HighscoreList.Count > MaxHighscoreListEntryCount)
            {
                HighscoreList.RemoveAt(MaxHighscoreListEntryCount);
            }

            SaveHighscoreList();

            bdrNewHighScore.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Event handler for game tick timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            // Actions to be performed every tick
            MoveSnake();
            CheckCollision();
        }

        /// <summary>
        /// Event handler for key up in main window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameArea_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case (Key.Up):
                case (Key.W):
                    snakeDirection = snakeDirection == SnakeDirection.Down ? snakeDirection : SnakeDirection.Up;
                    break;
                case (Key.Down):
                case (Key.S):
                    snakeDirection = snakeDirection == SnakeDirection.Up ? snakeDirection : SnakeDirection.Down;
                    break;
                case (Key.Left):
                case (Key.A):
                    snakeDirection = snakeDirection == SnakeDirection.Right ? snakeDirection : SnakeDirection.Left;
                    break;
                case (Key.Right):
                case (Key.D):
                    snakeDirection = snakeDirection == SnakeDirection.Left ? snakeDirection : SnakeDirection.Right;
                    break;
                case (Key.Space):
                    if (bdrHighscoreList.Visibility == Visibility.Visible)
                    {
                        // Navigate to welcome page
                        bdrHighscoreList.Visibility = Visibility.Collapsed;
                        bdrWelcomeMessage.Visibility = Visibility.Visible;
                    }
                    else if (!gameTickTimer.IsEnabled)
                    {
                        // Start a new game if pressed when no game in progress
                        StartNewGame();
                    }
                    else
                    {
                        // End the game if pressed when game already in progress
                        EndGame();
                    }
                    break;
            }
        }

        /**************************
         * GAME LOGIC
         *************************/

        /// <summary>
        /// Clear the game area and start a new game
        /// </summary>
        private void StartNewGame()
        {
            // Remove potential dead snake parts and leftover food...
            foreach (SnakePart snakeBodyPart in snakeParts)
            {
                if (snakeBodyPart.UiElement != null)
                    GameArea.Children.Remove(snakeBodyPart.UiElement);
            }
            snakeParts.Clear();

            if (snakeFood != null)
                GameArea.Children.Remove(snakeFood);

            // Close any open windows
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrGameOver.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Collapsed;

            // Initialize game settings
            currentScore = 0;
            snakeLength = SnakeStartLength;
            snakeDirection = SnakeDirection.Right;
            snakeParts.Add(new SnakePart(new Point(SnakeSquareSize * 5,SnakeSquareSize * 5),true));
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);

            // Remove instructions
            GameArea.Children.Remove(currentLabel);
            currentLabel = null;

            // Draw the elements and start game
            DrawSnake();
            DrawSnakeFood();
            gameTickTimer.IsEnabled = true;
            UpdateGameStatus();
        }

        /// <summary>
        /// End the game and display the final score
        /// </summary>
        private void EndGame()
        {
            bool isNewHighscore = false;
            if (currentScore > 0)
            {
                int lowestHighscore = (HighscoreList.Count > 0 ? HighscoreList.Min(x => x.Score) : 0);
                if ((currentScore > lowestHighscore) || (HighscoreList.Count < MaxHighscoreListEntryCount))
                {
                    bdrNewHighScore .Visibility = Visibility.Visible;
                    txtPlayerName.Focus();
                    isNewHighscore = true;
                }
            }
            if (!isNewHighscore)
            {
                tbFinalScore.Text = currentScore.ToString();
                bdrGameOver.Visibility = Visibility.Visible;
            }
            gameTickTimer.IsEnabled = false;
        }

        /// <summary>
        /// Draw the game area background
        /// </summary>
        private void DrawGameArea()
        {
            bool backgroundComplete = false;
            int nextX = 0;
            int nextY = 0;
            int rowCounter = 0;
            bool nextIsOdd = false;

            while (!backgroundComplete)
            {
                // Create a rectangle with the specified color
                Rectangle rect = new Rectangle
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = nextIsOdd ? Brushes.White : Brushes.LightGray
                };

                // Add rectangle to canvas
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);

                // Set values for next rectange
                nextIsOdd = !nextIsOdd;
                nextX += SnakeSquareSize;

                if (nextX >= GameArea.ActualWidth)
                {
                    // Start a new row
                    nextX = 0;
                    nextY += SnakeSquareSize;
                    rowCounter++;
                    nextIsOdd = (rowCounter % 2 != 0);
                }

                if (nextY >= GameArea.ActualHeight)
                {
                    // Background is complete
                    backgroundComplete = true;
                }
            }

        }

        /// <summary>
        /// Draw the snake
        /// </summary>
        private void DrawSnake()
        {
            foreach (SnakePart snakePart in snakeParts)
            {
                snakePart.Draw(SnakeSquareSize,GameArea);
            }
        }

        /// <summary>
        /// Move the snake one block on the game area
        /// </summary>
        private void MoveSnake()
        {
            // Remove the last part of the snake, in preparation of the new part added below
            while (snakeParts.Count >= snakeLength)
            {
                GameArea.Children.Remove(snakeParts[0].UiElement);
                snakeParts.RemoveAt(0);

            }

            // Let all existing snake parts be snake body parts
            foreach (SnakePart snakePart in snakeParts)
            {
                (snakePart.UiElement as Rectangle).Fill = snakeBodyBrush;
                snakePart.IsHead = false;
            }

            // Determine which direction to expand the snake, based on the current direction
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;

            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += SnakeSquareSize;
                    break;
            }

            // Add new head to snake
            snakeParts.Add(new SnakePart(new Point(nextX, nextY), true));

            // Draw snake part
            DrawSnake();
        }

        /// <summary>
        /// Randomly generate a valid snake food position
        /// </summary>
        private Point GetNextFoodPosition()
        {
            int maxX = (int)(GameArea.ActualWidth / SnakeSquareSize);
            int maxY = (int)(GameArea.ActualHeight / SnakeSquareSize);
            int foodX = rnd.Next(0, maxX) * SnakeSquareSize;
            int foodY = rnd.Next(0, maxY) * SnakeSquareSize;

            foreach (SnakePart snakePart in snakeParts)
            {
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                {
                    // Food position is invalid, get another one
                    return GetNextFoodPosition();
                }
            }

            return new Point(foodX,foodY);
        }

        /// <summary>
        /// Place a new snake food block on the game area
        /// it on the game canvas
        /// </summary>
        private void DrawSnakeFood()
        {
            if (snakeFood != null)
            {
                // Remove existing snake food
                GameArea.Children.Remove(snakeFood);
            }

            // Draw a new snake food
            snakeFood = new Ellipse()
            {
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Fill = foodBrush
            };

            Point snakeFoodPosition = GetNextFoodPosition();
            GameArea.Children.Add(snakeFood);
            Canvas.SetTop(snakeFood, snakeFoodPosition.Y);
            Canvas.SetLeft(snakeFood, snakeFoodPosition.X);
        }

        /// <summary>
        /// Checks if snake has collided with food, the window edge or itself
        /// and handles the case accordingly
        /// </summary>
        private void CheckCollision()
        {
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];

            // Check if collision with snake food
            if ((snakeHead.Position.X == Canvas.GetLeft(snakeFood)) && (snakeHead.Position.Y == Canvas.GetTop(snakeFood)))
            {
                EatSnakeFood();
            }

            // Check if collision with window edge
            if (snakeHead.Position.X + SnakeSquareSize > GameArea.ActualWidth || snakeHead.Position.X < 0 ||
                snakeHead.Position.Y + SnakeSquareSize > GameArea.ActualHeight || snakeHead.Position.Y < 0)
            {
                EndGame();
            }

            // Check if collision with snake
            for (int i = 0; i < snakeParts.Count - 1; i++)
            {
                if (snakeParts[i].Position == snakeHead.Position)
                {
                    EndGame();
                }
            }
        }

        /// <summary>
        /// Update game when snake eats a food block
        /// </summary>
        private void EatSnakeFood()
        {
            snakeLength++;
            currentScore++;
            int timeInterval = Math.Max(SnakeSpeedThreshold, (int)gameTickTimer.Interval.TotalMilliseconds - (currentScore * 2));
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(timeInterval);
            
            GameArea.Children.Remove(snakeFood);
            snakeFood = null;

            DrawSnakeFood();
            UpdateGameStatus();

        }

        /// <summary>
        /// Update the game statistics
        /// </summary>
        private void UpdateGameStatus()
        {
            tbStatusScore.Text = currentScore.ToString();
            tbStatusSpeed.Text = gameTickTimer.Interval.TotalMilliseconds.ToString();
        }

        private void LoadHighscoreList()
        {
            if (File.Exists("snake_highscorelist.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<SnakeHighscore>));
                using (Stream reader = new FileStream("snake_highscorelist.xml", FileMode.Open))
                {
                    List<SnakeHighscore> tempList = (List<SnakeHighscore>)serializer.Deserialize(reader);
                    this.HighscoreList.Clear();
                    foreach (var item in tempList.OrderByDescending(x => x.Score))
                        this.HighscoreList.Add(item);
                }
            }
        }

        private void SaveHighscoreList()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<SnakeHighscore>));
            using (Stream writer = new FileStream("snake_highscorelist.xml", FileMode.Create))
            {
                serializer.Serialize(writer, this.HighscoreList);
            }
        }
    }
}
