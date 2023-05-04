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
using System.Windows.Shapes;

namespace MinesweeperApp
{
    /// <summary>
    /// Interaction logic for DifficultyWindow.xaml
    /// </summary>
    public partial class DifficultyWindow : Window
    {
        public DifficultyWindow()
        {
            InitializeComponent();

            //make window adjust to whatever size necessary
            diff.SizeToContent = SizeToContent.WidthAndHeight;
        }

        //If this button is clicked close the window and restart the game with the new settings
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main;

            //get inputs
            int width = 9, height = 9, bombs = 10;

            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    main = window as MainWindow;

                    //get the prefered width and height
                    if(easyBtn.IsChecked == true)
                    {
                        width = 9;
                        height = 9;
                        bombs = 10;
                    }
                    else if(normBtn.IsChecked == true)
                    {
                        width = 16;
                        height = 16;
                        bombs = 40;
                    }
                    else if(hardBtn.IsChecked == true)
                    {
                        width = 30;
                        height = 16;
                        bombs = 99;
                    }
                    else if(custBtn.IsChecked == true)
                    {
                        width = int.Parse(wTxt.Text);
                        height = int.Parse(hTxt.Text);
                        bombs = int.Parse(bTxt.Text);
                    }

                    //end and restart the game
                    main.gameEnd();
                    main.restartGame(width, height, bombs);

                    //re-enable the window
                    main.IsEnabled = true;
                }
            }

            this.Close();
        }

        //If this window is closed continue the game
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            MainWindow main;
            bool gameBegun = false;

            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    main = window as MainWindow;

                    //Check if the clock has already started
                    gameBegun = main.clock.Elapsed.TotalSeconds > 0;

                    //if the game has already started restart the clock
                    if (gameBegun)
                    {
                        main.clock.Start();
                        main.dt.Start();
                    }

                    //re-enable the window
                    main.IsEnabled = true;
                }
            }
        }
    }
}
