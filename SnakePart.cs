using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Snake_Game
{
    class SnakePart
    {
        private SolidColorBrush snakeBodyBrush = Brushes.Green;
        private SolidColorBrush snakeHeadBrush = Brushes.YellowGreen;

        public UIElement UiElement { get; set; }
        public Point Position { get; set; }
        public bool IsHead { get; set; }


        /// <summary>
        /// Initialize a snake part 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="isHead"></param>
        public SnakePart(Point position, bool isHead)
        {
            Position = position;
            IsHead = isHead;
        }

        /// <summary>
        /// Draw the snake part on the canvas
        /// </summary>
        /// <param name="snakeSquareSize">The size of the snake part to be drawn</param>
        /// <param name="gameArea">The canvas to be drawn onto</param>
        public void Draw(int snakeSquareSize, Canvas gameArea)
        {
            if (UiElement == null)
            {
                UiElement = new Rectangle()
                {
                    Width = snakeSquareSize,
                    Height = snakeSquareSize,
                    Fill = IsHead ? snakeHeadBrush : snakeBodyBrush
                };
                gameArea.Children.Add(UiElement);
                Canvas.SetTop(UiElement, Position.Y);
                Canvas.SetLeft(UiElement, Position.X);
            }
        }
    }
}
