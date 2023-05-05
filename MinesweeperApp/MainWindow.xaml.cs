using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MinesweeperApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Set array names for buttons and bombs for minefield
        private Button[,] buttons;
        private Bomb[] bombs;

        //Set the global flag counter
        private int flagCounter = 0;

        //set the global clock
        public Stopwatch clock;
        public DispatcherTimer timer = new DispatcherTimer();
        public bool gameover = false;

        //Button Face States storer/setter
        private int currentFace;
        public int faceState
        {
            set
            {
                currentFace = value;
                switch (currentFace)
                {
                    case 0: //Happy
                        restartBtn.Content = ":)";
                        break;
                    case 1: //Cautious
                        restartBtn.Content = ":O";
                        break;
                    case 2: //Dead
                        restartBtn.Content = "X(";
                        break;
                    case 3: //Victory !
                        restartBtn.Content = "B)";
                        break;
                    default: //Unknown
                        restartBtn.Content = ":S";
                        break;
                }
            }
        }

        //General Main window class
        public MainWindow()
        {
            InitializeComponent();

            //set basic elements for the clock
            timer.Tick += new EventHandler(timerTicks);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);

            //make window adjust to whatever size necessary
            main.SizeToContent = SizeToContent.WidthAndHeight;

            //create the minefield
            createGrid();
        }

        //Grid's basic size and elements are the beginner grid's properties
        private void createGrid(int xSize = 9, int ySize = 9, int mines = 10)
        {
            //create grid's size counters and set the size the buttons will take up
            int xSizeCount = 0, ySizeCount = 0;
            GridLength space = new GridLength(24, GridUnitType.Pixel);
            minefield.HorizontalAlignment = HorizontalAlignment.Center;

            //create a new stopwatch
            clock = new Stopwatch();

            //Create the bombs' co-ords for placement in the field
            positionMines(mines, xSize, ySize);

            //Define the Columns
            while (xSizeCount < xSize)
            {
                ColumnDefinition colDef = new ColumnDefinition() { Width = space };
                minefield.ColumnDefinitions.Add(colDef);
                xSizeCount++;
            }

            //define the rows
            while (ySizeCount < ySize)
            {
                RowDefinition rowDef = new RowDefinition() { Height = space };
                minefield.RowDefinitions.Add(rowDef);
                ySizeCount++;
            }

            //ITERATE THROUGH THE FIELD TO SET BUTTONS AND BOMBS
            //Along the corridor...
            buttons = new Button[xSize, ySize];
            int Xcount = 0;
            while (Xcount < buttons.GetLength(0))
            {
                //Up the stairs...
                int Ycount = 0;

                while (Ycount < buttons.GetLength(1))
                {
                    //You are a button...
                    Button btn = new Button();
                    btn.FontSize = 16;
                    btn.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(mouseUp);

                    //... unless you're a bomb...
                    foreach (Bomb bomb in bombs)
                    {
                        //... and if you are a bomb you will go boom
                        if (Xcount == bomb.X && Ycount == bomb.Y)
                        {
                            btn.PreviewMouseLeftButtonUp -= mouseUp;
                            btn.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(bombClicked);
                            //btn.Content = "B";
                        }
                    }
                    //both buttons and bombs can be stepped on/flagged
                    btn.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(mouseDown);
                    btn.MouseRightButtonDown += new MouseButtonEventHandler(flagPlaced);

                    //place the button/bomb
                    buttons[Xcount, Ycount] = btn;
                    Grid.SetColumn(btn, Xcount);
                    Grid.SetRow(btn, Ycount);

                    Ycount++;
                }
                Xcount++;
            }

            //Along the corridor... (For every button on the X)
            for (int xbtns = 0; xbtns < buttons.GetLength(0); xbtns++)
            {
                //up the stairs... (Print a downward array of buttons)
                for (int ybtns = 0; ybtns < buttons.GetLength(1); ybtns++)
                {
                    minefield.Children.Add(buttons[xbtns, ybtns]);
                }
            }
        }

        //set up the mines!
        private void positionMines(int mines = 10, int XSize = 9, int YSize = 9)
        {
            //set bomb array size
            bombs = new Bomb[0];

            //set number of flags
            flagCounter = mines;
            flagCounterDisplay.Content = flagCounter.ToString("000");

            //randomise the bomb's placement
            int mineCount = 0;
            while (mineCount < mines)
            {
                Random ran = new Random();
                Bomb newBomb = new Bomb();
                newBomb.X = ran.Next(0, XSize);
                newBomb.Y = ran.Next(0, YSize);

                //if there is already a mine at these co-ordinates, re-randomise
                while (bombs.Any(bomb => bomb.X == newBomb.X && bomb.Y == newBomb.Y))
                {
                    newBomb.X = ran.Next(0, XSize);
                    newBomb.Y = ran.Next(0, YSize);
                }

                //set bomb
                Array.Resize(ref bombs, bombs.Length + 1);
                bombs[mineCount] = newBomb;

                mineCount++;
            }
        }

        //Oho? You clicked a square?
        private void mouseDown(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            //start the clock if it is not running
            if (!clock.IsRunning)
            {
                clock.Start();
                timer.Start();
            }

            //only allow clicks if not flagged
            if (btn.Content != "🚩")
            {
                faceState = 1;
            }
        }

        //You're in the clear. :)
        private void mouseUp(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            //only allow clicks if not flagged
            if (btn.Content != "🚩")
            {
                tileActivated(btn);
            }
        }

        //activate the buttons!
        private void tileActivated(Button btn)
        {
            //Get the button and calculate the mines around it
            var parent = minefield.Parent as UIElement;
            var location = btn.TranslatePoint(new Point(0, 0), parent);
            int X = (int)Math.Floor(location.X / 24);
            int Y = (int)Math.Floor(location.Y / 24) - 3;
            int numBombs = mineCalculator(X, Y);

            //do not allow iterative unflagging or colour changing
            if (btn.Content != "🚩")
            {
                //Tell the player how many bombs are nearby
                switch (numBombs)
                {
                case 0:
                    break;
                case 1:
                    btn.Foreground = Brushes.Blue;
                    break;
                case 2:
                    btn.Foreground = Brushes.Green;
                    break;
                case 3:
                    btn.Foreground = Brushes.Red;
                    break;
                case 4:
                    btn.Foreground = Brushes.DarkBlue;
                    break;
                case 5:
                    btn.Foreground = Brushes.DarkRed;
                    break;
                case 6:
                    btn.Foreground = Brushes.DarkCyan;
                    break;
                case 7:
                    btn.Foreground = Brushes.Black;
                    break;
                case 8:
                    btn.Foreground = Brushes.DarkSlateGray;
                    break;
                }

                faceState = 0;
                btn.IsEnabled = false;
                //only print a number if there are bombs nearby, otherwise blank the square
                if (numBombs != 0) { btn.Content = numBombs; }
                else { btn.Content = ""; }
            }

            //This tile has no nearby bombs. Reveal adjacent tiles
            if (numBombs == 0)
            {
                //Locate the activated button!
                //Along the corridor...
                int Xcount = 0;

                while (Xcount < buttons.GetLength(0))
                {
                    //Up the stairs...
                    int Ycount = 0;

                    while (Ycount < buttons.GetLength(1))
                    {
                        //Match the button to its position in the array
                        if (btn == buttons[Xcount, Ycount])
                        {
                            //If the button is not agaist the wall or a disabled button 
                            //On true activate the tile of...
                            //... NORTHWEST 
                            if ((btn != buttons[0, Ycount]) && (btn != buttons[Xcount, 0]) && (buttons[Xcount - 1, Ycount - 1].IsEnabled))
                            {
                                tileActivated(buttons[Xcount - 1, Ycount - 1]);
                            }

                            //... NORTH
                            if ((btn != buttons[Xcount, 0]) && (buttons[Xcount, Ycount - 1].IsEnabled))
                            {
                                tileActivated(buttons[Xcount, Ycount - 1]);
                            }

                            //... NORTHEAST
                            if ((btn != buttons[Xcount, 0]) && (btn != buttons[buttons.GetLength(0) - 1, Ycount]) && (buttons[Xcount + 1, Ycount - 1].IsEnabled))
                            {
                                tileActivated(buttons[Xcount + 1, Ycount - 1]);
                            }

                            //... WEST
                            if ((btn != buttons[0, Ycount]) && (buttons[Xcount - 1, Ycount].IsEnabled))
                            {
                                tileActivated(buttons[Xcount - 1, Ycount]);
                            }

                            //... EAST
                            if ((btn != buttons[buttons.GetLength(0) - 1, Ycount]) && (buttons[Xcount + 1, Ycount].IsEnabled))
                            {
                                tileActivated(buttons[Xcount + 1, Ycount]);
                            }

                            //... SOUTHWEST
                            if ((btn != buttons[Xcount, buttons.GetLength(1) - 1]) && (btn != buttons[0, Ycount]) && (buttons[Xcount - 1, Ycount + 1].IsEnabled))
                            {
                                tileActivated(buttons[Xcount - 1, Ycount + 1]);
                            }

                            //... SOUTH
                            if ((btn != buttons[Xcount, buttons.GetLength(1) - 1]) && (buttons[Xcount, Ycount + 1].IsEnabled))
                            {
                                tileActivated(buttons[Xcount, Ycount + 1]);
                            }

                            //... SOUTHEAST 
                            if ((btn != buttons[Xcount, buttons.GetLength(1) - 1]) && (btn != buttons[buttons.GetLength(0) - 1, Ycount]) && (buttons[Xcount + 1, Ycount + 1].IsEnabled))
                            {
                                tileActivated(buttons[Xcount + 1, Ycount + 1]);
                            }

                            break;
                        }
                        Ycount++;
                    }
                    Xcount++;
                }
            }

            //calculate victory
            bool theTruth;
            int truthCount = 0;
            //along the corridor
            int XcountAgain = 0;

            while (XcountAgain < buttons.GetLength(0))
            {
                //Up the stairs...
                int YcountAgain = 0;
                while (YcountAgain < buttons.GetLength(1))
                {
                    //is the located button disabled?
                    theTruth = !buttons[XcountAgain, YcountAgain].IsEnabled;
                    if (theTruth)
                    {
                        truthCount++;
                    }
                    YcountAgain++;
                }
                XcountAgain++;
            }
            //if every tile that isn't a bomb is revealed, victory!
            if (truthCount == buttons.Length - bombs.Length)
            {
                gameEnd();
            }
        }

        //You clicked a bomb!
        private void bombClicked(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            //Unless the tile is flagged, Explode!
            if (btn.Content != "🚩")
            {
                //stop the clock
                if (clock.IsRunning)
                {
                    clock.Stop();
                }
                TimeSpan time = clock.Elapsed;

                //If the timer is capped out, do not print the actual time.
                if (time.TotalSeconds < 999)
                {
                    timeCounterDisplay.Content = time.TotalSeconds.ToString("000");
                }
                else
                {
                    timeCounterDisplay.Content = "999";
                }

                //Format game to game over state.
                faceState = 2;
                btn.FontSize = 20;
                btn.FontWeight = FontWeights.Bold;
                btn.Content = "*";
                btn.Foreground = Brushes.Black;
                btn.Background = Brushes.Red;

                //Locate and disable all buttons
                //Along the corridor...
                int Xcount = 0;

                while (Xcount < buttons.GetLength(0))
                {
                    //Up the stairs...
                    int Ycount = 0;

                    while (Ycount < buttons.GetLength(1))
                    {
                        //remove active button's functions
                        buttons[Xcount, Ycount].PreviewMouseLeftButtonDown -= mouseDown;
                        buttons[Xcount,Ycount].PreviewMouseLeftButtonUp -= bombClicked;
                        buttons[Xcount, Ycount].PreviewMouseLeftButtonUp -= mouseUp;
                        buttons[Xcount, Ycount].MouseRightButtonDown -= flagPlaced;

                        //if the bombs arent flagged, reveal them to the player! (And reveal their mistakes with an X)
                        if ((bombs.Any(bomb => bomb.X == Xcount && bomb.Y == Ycount)) && (btn != buttons[Xcount, Ycount]) && (buttons[Xcount, Ycount].Content != "🚩"))
                        {
                            buttons[Xcount, Ycount].FontSize = 20;
                            buttons[Xcount, Ycount].FontWeight = FontWeights.Bold;
                            buttons[Xcount, Ycount].Content = "*";
                            buttons[Xcount, Ycount].Foreground = Brushes.Black;
                            buttons[Xcount, Ycount].IsEnabled = false;
                        }
                        else if((!bombs.Any(bomb => bomb.X == Xcount && bomb.Y == Ycount)) && (btn != buttons[Xcount, Ycount]) && (buttons[Xcount, Ycount].Content == "🚩"))
                        {
                            buttons[Xcount, Ycount].FontSize = 16;
                            buttons[Xcount, Ycount].FontWeight = FontWeights.Bold;
                            buttons[Xcount, Ycount].Content = "X";
                            buttons[Xcount, Ycount].Foreground = Brushes.Red;
                            buttons[Xcount, Ycount].IsEnabled = false;
                        }

                        Ycount++;
                    }
                    Xcount++;
                }
            }
            gameover = true;
        }

        //when a flag is placed (Right Click)
        private void flagPlaced(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            //check if this button can be flagged, or should be quiried
            if ((btn.Content != "🚩") && (btn.Content != "?" && flagCounter != 0))
            {
                btn.Content = "🚩";
                flagCounter--;
            }
            else if(btn.Content == "🚩")
            {
                btn.Content = "?";
                flagCounter++;
            }
            else
            {
                btn.Content = "";
            }

            flagCounterDisplay.Content = flagCounter.ToString("000");
        }


        //When the reset button is clicked...
        private void restartBtn_Click(object sender, RoutedEventArgs e)
        {
            restartGame(buttons.GetLength(0), buttons.GetLength(1), bombs.Length);
        }

        //Reset the game!
        public void restartGame(int width = 9, int height = 9, int mines = 10)
        {
            //UI tweaks and game setup
            faceState = 0;
            gameover = false;
            timeCounterDisplay.Content = "000";
            minefield.Children.Clear();
            minefield.RowDefinitions.Clear();
            minefield.ColumnDefinitions.Clear();
            createGrid(width, height, mines);
        }

        //Calculate adjacent mines...
        int mineCalculator(int x, int y)
        {
            int numBombs = 0;

            //Search around the square for bombs
            //X..
            if (bombs.Any(bomb => bomb.X == x -1 && bomb.Y == y -1))
            {
                numBombs++;
            }
            //.X.
            if (bombs.Any(bomb => bomb.X == x && bomb.Y == y - 1))
            {
                numBombs++;
            }
            //..X
            if (bombs.Any(bomb => bomb.X == x + 1 && bomb.Y == y - 1))
            {
                numBombs++;
            }

            //X..
            if (bombs.Any(bomb => bomb.X == x - 1 && bomb.Y == y))
            {
                numBombs++;
            }
            //..X
            if (bombs.Any(bomb => bomb.X == x + 1 && bomb.Y == y))
            {
                numBombs++;
            }

            //X..
            if (bombs.Any(bomb => bomb.X == x - 1 && bomb.Y == y + 1))
            {
                numBombs++;
            }
            //.X.
            if (bombs.Any(bomb => bomb.X == x && bomb.Y == y + 1))
            {
                numBombs++;
            }
            //..X
            if (bombs.Any(bomb => bomb.X == x + 1 && bomb.Y == y + 1))
            {
                numBombs++;
            }

            return numBombs;
        }

        //calculate what the current time is
        void timerTicks(object sender, EventArgs e)
        {
            if (clock.IsRunning)
            {
                //print the total seconds to the display
                TimeSpan time = clock.Elapsed;
                if (time.TotalSeconds < 999)
                {
                    timeCounterDisplay.Content = (time.TotalSeconds).ToString("000");
                }
                else
                {
                    timeCounterDisplay.Content = "999";
                    clock.Stop();
                }
            }
        }

        //End the game
        public void gameEnd()
        {
            //stop the clock
            if (clock.IsRunning)
            {
                clock.Stop();
            }

            //print the total seconds to the display
            TimeSpan time = clock.Elapsed;
            if (time.TotalSeconds < 999)
            {
                timeCounterDisplay.Content = time.TotalSeconds.ToString("000");
            }
            else
            {
                timeCounterDisplay.Content = "999";
            }

            //Set Face Status
            faceState = 3;
            gameover = true;

            //Locate and disable all buttons
            //Along the corridor...
            int Xcount = 0;

            while (Xcount < buttons.GetLength(0))
            {
                //Up the stairs...
                int Ycount = 0;

                while (Ycount < buttons.GetLength(1))
                {
                    //Remove button functions
                    buttons[Xcount, Ycount].PreviewMouseLeftButtonDown -= mouseDown;
                    buttons[Xcount, Ycount].PreviewMouseLeftButtonUp -= bombClicked;
                    buttons[Xcount, Ycount].PreviewMouseLeftButtonUp -= mouseUp;
                    buttons[Xcount, Ycount].MouseRightButtonDown -= flagPlaced;

                    //If there are unflagged bombs remaining, flag them.
                    if ((bombs.Any(bomb => bomb.X == Xcount && bomb.Y == Ycount)) && (buttons[Xcount, Ycount].Content != "🚩"))
                    {
                        buttons[Xcount, Ycount].Content = "🚩";
                    }
                    Ycount++;
                }
                Xcount++;
            }
        }

        private void difficultyBtn_Click(object sender, RoutedEventArgs e)
        {
            //stop the clock
            if (clock.IsRunning)
            {
                clock.Stop();
            }

            //Create new difficulty Window
            DifficultyWindow diff = new DifficultyWindow();

            //get the difficulty to display
            if((buttons.GetLength(0) == 9) && (buttons.GetLength(1) == 9) && (bombs.Length == 10))
            {
                diff.easyBtn.IsChecked = true;
            }
            else if((buttons.GetLength(0) == 16) && (buttons.GetLength(1) == 16) && (bombs.Length == 40))
            {
                diff.normBtn.IsChecked = true;
            }
            else if ((buttons.GetLength(0) == 30) && (buttons.GetLength(1) == 16) && (bombs.Length == 99))
            {
                diff.hardBtn.IsChecked = true;
            }
            else
            {
                //if custom get the attributes of the difficulty
                diff.wTxt.Text = buttons.GetLength(0).ToString();
                diff.hTxt.Text = buttons.GetLength(1).ToString();
                diff.bTxt.Text = bombs.Length.ToString();
                diff.custBtn.IsChecked = true;
            }

            //show the window and disable the main
            main.IsEnabled = false;
            diff.Show();
        }
    }
}
