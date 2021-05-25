using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Threading;

namespace SnakeGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int _snakeSquareSize = 15;
        const int _snakeStartLength = 3;
        const int _snakeStartSpeed = 400;
        const int _snakeSpeedThreshold = 100;
        private SolidColorBrush _snakeBodyBrush = Brushes.Green;
        private SolidColorBrush _snakeHeadBrush = Brushes.Red;
        private List<Snake> _snakes = new List<Snake>();
        private Constants.SnakeDirection _snakeDirection = Constants.SnakeDirection.Right;
        private int _snakeLength;
        private DispatcherTimer _gameTickTimer = new DispatcherTimer();
        private Random _random = new Random();
        private UIElement _snakeFood = null;
        private SolidColorBrush _foodBrush = Brushes.Red;

        public MainWindow()
        {
            InitializeComponent();
            _gameTickTimer.Tick += GameTickTimer_Tick;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            PrepareGameArea();
            StartNewGame();
        }

        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
        }

        private void StartNewGame()
        {
            _snakeLength = _snakeStartLength;
            _snakeDirection = Constants.SnakeDirection.Right;
            _snakes.Add(new Snake() { Position = new Point(_snakeSquareSize * 5, _snakeSquareSize * 5) });
            _gameTickTimer.Interval = TimeSpan.FromMilliseconds(_snakeStartSpeed);

            // Draw the snake  
            DrawSnake();
            DrawSnakeFood();

            _gameTickTimer.IsEnabled = true;
        }

        private void PrepareGameArea()
        {
            bool doneDrawingBackground = false, nextIsOdd = false;
            int nextX = 0, nextY = 0, rowCounter = 0;

            while(!doneDrawingBackground)
            {
                Rectangle _rectangle = new Rectangle()
                {
                    Width = _snakeSquareSize,
                    Height = _snakeSquareSize,
                    Fill = nextIsOdd ? Brushes.White : Brushes.Black
                };

                GameArea.Children.Add(_rectangle);
                Canvas.SetTop(_rectangle, nextY);
                Canvas.SetLeft(_rectangle, nextX);

                nextIsOdd = !nextIsOdd;
                nextX += _snakeSquareSize;

                if (nextX >= GameArea.ActualWidth)
                {
                    nextX = 0;
                    nextY += _snakeSquareSize;
                    rowCounter++;
                    nextIsOdd = (rowCounter % 2 != 0);
                }

                if (nextY >= GameArea.ActualHeight)
                    doneDrawingBackground = true;
            }
        }

        private void DrawSnake()
        {
            foreach(Snake snake in _snakes)
            {
                if(snake.UiElement is null)
                {
                    snake.UiElement = new Rectangle()
                    {
                        Width = _snakeSquareSize,
                        Height = _snakeSquareSize,
                        Fill = snake.IsHead ? _snakeHeadBrush : _snakeBodyBrush
                    };

                    GameArea.Children.Add(snake.UiElement);
                    Canvas.SetTop(snake.UiElement, snake.Position.X);
                    Canvas.SetTop(snake.UiElement, snake.Position.Y);
                }
            }
        }

        private void MoveSnake()
        {
            // Remove the last part of the snake, in preparation of the new part added below  
            while (_snakes.Count >= _snakeLength)
            {
                GameArea.Children.Remove(_snakes[0].UiElement);
                _snakes.RemoveAt(0);
            }

            // Next up, we'll add a new element to the snake, which will be the (new) head  
            // Therefore, we mark all existing parts as non-head (body) elements and then  
            // we make sure that they use the body brush  
            foreach (Snake snake in _snakes)
            {
                (snake.UiElement as Rectangle).Fill = _snakeBodyBrush;
                snake.IsHead = false;
            }

            // Determine in which direction to expand the snake, based on the current direction  
            Snake snakeHead = _snakes[_snakes.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (_snakeDirection)
            {
                case Constants.SnakeDirection.Left:
                    nextX -= _snakeSquareSize;
                    break;
                case Constants.SnakeDirection.Right:
                    nextX += _snakeSquareSize;
                    break;
                case Constants.SnakeDirection.Up:
                    nextY -= _snakeSquareSize;
                    break;
                case Constants.SnakeDirection.Down:
                    nextY += _snakeSquareSize;
                    break;
            }

            _snakes.Add(new Snake()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });
            
            DrawSnake();
            
            //// TODO : CollisionCheck
        }

        private Point GetNextFoodPosition()
        {
            int maxX = (int)(GameArea.ActualWidth / _snakeSquareSize);
            int maxY = (int)(GameArea.ActualHeight / _snakeSquareSize);
            int foodX = _random.Next(0, maxX) * _snakeSquareSize;
            int foodY = _random.Next(0, maxY) * _snakeSquareSize;

            foreach (Snake snake in _snakes)
            {
                if ((snake.Position.X == foodX) && (snake.Position.Y == foodY))
                    return GetNextFoodPosition();
            }

            return new Point(foodX, foodY);
        }

        private void DrawSnakeFood()
        {
            Point foodPosition = GetNextFoodPosition();
            _snakeFood = new Ellipse()
            {
                Width = _snakeSquareSize,
                Height = _snakeSquareSize,
                Fill = _foodBrush
            };
            GameArea.Children.Add(_snakeFood);
            Canvas.SetTop(_snakeFood, foodPosition.Y);
            Canvas.SetLeft(_snakeFood, foodPosition.X);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            Constants.SnakeDirection originalSnakeDirection = _snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    if (_snakeDirection != Constants.SnakeDirection.Down)
                        _snakeDirection = Constants.SnakeDirection.Up;
                    break;
                case Key.Down:
                    if (_snakeDirection != Constants.SnakeDirection.Up)
                        _snakeDirection = Constants.SnakeDirection.Down;
                    break;
                case Key.Left:
                    if (_snakeDirection != Constants.SnakeDirection.Right)
                        _snakeDirection = Constants.SnakeDirection.Left;
                    break;
                case Key.Right:
                    if (_snakeDirection != Constants.SnakeDirection.Left)
                        _snakeDirection = Constants.SnakeDirection.Right;
                    break;
                case Key.Space:
                    StartNewGame();
                    break;
            }
            if (_snakeDirection != originalSnakeDirection)
                MoveSnake();
        }
    }
}
