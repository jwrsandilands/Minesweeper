using System;
using System.Collections;
using System.Collections.Generic;
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

                    foreach (Bomb bomb in bombs)
                    {
                        if (Xcount == bomb.X && Ycount == bomb.Y)
                        {
                            btn.Click += new RoutedEventHandler(bombClicked);
                        }
                    }
                    btn.Click += new RoutedEventHandler(buttonClicked);
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

        void buttonClicked(object sender, EventArgs e)
        {
            Button btn = sender as Button;


            if (btn.Content != "1>")
            {
                btn.IsEnabled = false;
            }
        }

        void bombClicked(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if(btn.Content != "1>")
            {
                btn.Content = "*";
                restartBtn.Content = "XO";
            }
        }

        void flagPlaced(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Content != "1>")
            {
                btn.Content = "1>";
            }
            else
            {
                btn.Content = "";
            }
        }

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
            restartBtn.Content = ":)";

        }
    }
}
