using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MinesweeperApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Button[,] buttons;
        Bomb[] bombs;

        public MainWindow()
        {
            InitializeComponent();

            //make window adjust to whatever size necessary
            main.SizeToContent = SizeToContent.WidthAndHeight;

            createGrid();
        }

        public void createGrid(int xSize = 8, int ySize = 8)
        {
            //create grid's size counters
            int xSizeCount = 0, ySizeCount = 0;
            GridLength space = new GridLength(24, GridUnitType.Pixel);
            minefield.HorizontalAlignment = HorizontalAlignment.Center;

            //Create the minefield
            positionMines(10, xSize, ySize);

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

            //Along the corridor...
            buttons = new Button[xSize,ySize];
            int Xcount = 0;

            while (Xcount < buttons.GetLength(0))
            {
                //Up the stairs...
                int Ycount = 0;
                //char Ychar = 'A';

                while (Ycount < buttons.GetLength(1))
                {
                    Button btn = new Button();
                    btn.FontSize = 16;
                    btn.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(mouseUp);

                    foreach (Bomb bomb in bombs)
                    {
                        if (Xcount == bomb.X && Ycount == bomb.Y)
                        {
                            btn.PreviewMouseLeftButtonUp -= mouseUp;
                            btn.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(bombClicked);
                            //btn.Content = "B";
                        }
                    }
                    btn.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(mouseDown);
                    btn.MouseRightButtonDown += new MouseButtonEventHandler(flagPlaced);
                    //btn.Content = (1 + Xcount).ToString() + Ychar;
                    buttons[Xcount,Ycount] = btn;
                    Grid.SetColumn(btn, Xcount);
                    Grid.SetRow(btn, Ycount);

                    Ycount++;
                    //Ychar++;
                }
                Xcount++;
            }

            //Along the corridor... (For every button on the X)
            for (int xbtns = 0; xbtns < buttons.GetLength (0); xbtns++)
            {
                //up the stairs... (Print a downward array of buttons)
                for (int ybtns = 0; ybtns < buttons.GetLength (1); ybtns++)
                {
                    minefield.Children.Add(buttons[xbtns,ybtns]);
                }
            }
        }

        //set up the mines!
        public void positionMines(int mines = 10, int XSize = 8, int YSize = 8)
        {
            bombs = new Bomb[0];
            int mineCount = 0;

            while (mineCount < mines)
            {
                Random ran = new Random();
                Bomb newBomb = new Bomb();
                newBomb.X = ran.Next(0, XSize);
                newBomb.Y = ran.Next(0, YSize);

                //if there is already a mine at these co-ordinates
                while (bombs.Any(bomb => bomb.X == newBomb.X && bomb.Y == newBomb.Y))
                {
                    newBomb.X = ran.Next(0, XSize);
                    newBomb.Y = ran.Next(0, YSize);
                }

                Array.Resize(ref bombs, bombs.Length + 1);
                bombs[mineCount] = newBomb;

                mineCount++;
            }
        }

        //Oho? You clicked a square?
        void mouseDown(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if (btn.Content != "1>")
            {
                faceState = 1;
            }
        }

        //You're in the clear. :)
        void mouseUp(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            tileActivated(btn);
        }

        //activate the buttons!
        void tileActivated(Button btn)
        {
            //Get the button and calculate the mines around it
            var parent = minefield.Parent as UIElement;
            var location = btn.TranslatePoint(new Point(0, 0), parent);
            int X = (int)Math.Floor(location.X / 24);
            int Y = (int)Math.Floor(location.Y / 24) - 3;
            int numBombs = mineCalculator(X, Y);

            if (btn.Content != "1>")
            {
                faceState = 0;
                btn.IsEnabled = false;
                if (numBombs != 0) { btn.Content = numBombs; }
            }

            //This tile has no nearby bombs. Reveal adjacent tiles
            if(numBombs == 0)
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
                        //if the button selected is found in the array
                        if (btn == buttons[Xcount, Ycount])
                        {
                            //If the button isnt agaist the wall or a disabled button (NORTHWEST)
                            if (btn != buttons[0, Ycount] && btn != buttons[Xcount, 0] && buttons[Xcount - 1, Ycount -1].IsEnabled )
                            {
                                //Activate the tile to the West
                                tileActivated(buttons[Xcount - 1, Ycount - 1]);
                            }

                            //If the button isnt agaist the wall or a disabled button (NORTH)
                            if (btn != buttons[Xcount, 0] && buttons[Xcount, Ycount - 1].IsEnabled)
                            {
                                //Activate the tile to the North
                                tileActivated(buttons[Xcount, Ycount - 1]);
                            }

                            //If the button isnt agaist the wall or a disabled button (NORTHEAST)
                            if (btn != buttons[Xcount, 0] && btn != buttons[buttons.GetLength(0) - 1, Ycount] && buttons[Xcount + 1, Ycount - 1].IsEnabled)
                            {
                                //Activate the tile to the North
                                tileActivated(buttons[Xcount + 1, Ycount - 1]);
                            }

                            //If the button isnt agaist the wall or a disabled button (WEST)
                            if (btn != buttons[0, Ycount] && buttons[Xcount - 1, Ycount].IsEnabled)
                            {
                                //Activate the tile to the West
                                tileActivated(buttons[Xcount - 1, Ycount]);
                            }

                            //If the button isnt agaist the wall or a disabled button (EAST)
                            if (btn != buttons[buttons.GetLength(0) -1, Ycount] && buttons[Xcount + 1, Ycount].IsEnabled)
                            {
                                //Activate the tile to the east
                                tileActivated(buttons[Xcount + 1, Ycount]);
                            }

                            //If the button isnt agaist the wall or a disabled button (SOUTHWEST)
                            if (btn != buttons[Xcount, buttons.GetLength(1) - 1] && btn != buttons[0, Ycount] && buttons[Xcount - 1, Ycount + 1].IsEnabled)
                            {
                                //activate the tile to the south
                                tileActivated(buttons[Xcount - 1, Ycount + 1]);
                            }

                            //If the button isnt agaist the wall or a disabled button (SOUTH)
                            if (btn != buttons[Xcount, buttons.GetLength(1) - 1] && buttons[Xcount, Ycount + 1].IsEnabled)
                            {
                                //activate the tile to the south
                                tileActivated(buttons[Xcount, Ycount + 1]);
                            }

                            //If the button isnt agaist the wall or a disabled button (SOUTHEAST)
                            if (btn != buttons[Xcount, buttons.GetLength(1) - 1] && btn != buttons[buttons.GetLength(0) - 1, Ycount] && buttons[Xcount + 1, Ycount + 1].IsEnabled)
                            {
                                //activate the tile to the south
                                tileActivated(buttons[Xcount + 1, Ycount + 1]);
                            }

                            break;
                        }
                        Ycount++;
                    }
                    Xcount++;
                }
            }

        }

        //You clicked a bomb!
        void bombClicked(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if(btn.Content != "1>")
            {
                faceState = 2;
                btn.Content = "*";
                btn.IsEnabled = false;
            }
        }

        //when a flag is placed (Right Click)
        void flagPlaced(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Content != "1>" && btn.Content != "?")
            {
                btn.Content = "1>";
            }
            else if(btn.Content == "1>")
            {
                btn.Content = "?";
            }
            else
            {
                btn.Content = "";
            }
        }


        //When the reset button is clicked...
        private void restartBtn_Click(object sender, RoutedEventArgs e)
        {
            int Xcount = 0;

            while (Xcount < buttons.GetLength(0))
            {
                //Up the stairs...
                int Ycount = 0;

                while (Ycount < buttons.GetLength(1))
                {
                    buttons[Xcount, Ycount].IsEnabled = true;
                    buttons[Xcount, Ycount].Content = "";

                    Ycount++;
                }
                Xcount++;
            }
            faceState = 0;

        }


        //Button Face States
        private int _faceState;

        public int faceState
        {
            get => _faceState;
            set
            {
                _faceState = value;
                switch (_faceState)
                {
                    case 0: //Happy
                        restartBtn.Content = ":)";
                        break;
                    case 1: //Cautious
                        restartBtn.Content = ":?";
                        break;
                    case 2: //Dead
                        restartBtn.Content = "XO";
                        break;
                    default: //Unknown
                        restartBtn.Content = ":S";
                        break;
                }
            }
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
            //.X.

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
    }
}
