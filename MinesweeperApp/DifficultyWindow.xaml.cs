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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static System.Reflection.Metadata.BlobBuilder;

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


                        //Get the width (within reason!)
                        width = int.Parse(wTxt.Text);
                        if(width < 8) { width = 8; }
                        else if (width > 78) { width = 78; }

                        //Get the Height (within reason!)
                        height = int.Parse(hTxt.Text);
                        if(height <1) { height = 1; }
                        else if (height > 36) { height = 36; }

                        //Get the number of bombs (within reason!)
                        bombs = int.Parse(bTxt.Text);
                        if(bombs <= 0) { bombs = 1; }
                        else if(bombs > (width * height) - 1) { bombs = (width * height) - 1; }
                        else if(bombs > 9990) { bombs = 999; }
                        
                        //if your bombs dont fit the space well enough...
                        if(bombs < (width * 2) && (width * height) > 1200)
                        {
                            bombs = width * 2;
                        }
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
                    if (gameBegun && !main.gameover)
                    {
                        main.clock.Start();
                        main.timer.Start();
                    }

                    //re-enable the window
                    main.IsEnabled = true;
                }
            }
        }

        //check if the Height box is clicked
        private void hTxt_TextFocus(object sender, System.EventArgs e)
        {
            //hTxt.Text = "";

            //check the custom button
            custBtn.IsChecked = true;
        }
        //if the height box value is changed to something it cant be default to something it can be
        private void hTxt_TextChanged(object sender, RoutedEventArgs e)
        {
            if (hTxt.Text == "")
            {
                hTxt.Text = "1";
            }
            else if (!int.TryParse(hTxt.Text, out int i))
            {
                hTxt.Text = "1";
            }
            else if (i > 36)
            {
                hTxt.Text = "36";
            }
            else if (i < 1)
            {
                hTxt.Text = "1";
            }
        }

        //check if the width box is clicked
        private void wTxt_TextFocus(object sender, System.EventArgs e)
        {
            //check the custom button
            custBtn.IsChecked = true;
        }
        //if the width box value is changed to something it cant be default to something it can be
        private void wTxt_TextChanged(object sender, RoutedEventArgs e)
        {
            if (wTxt.Text == "")
            {
                wTxt.Text = "8";
            }
            else if (!int.TryParse(wTxt.Text, out int i))
            {
                wTxt.Text = "8";
            }
            else if (i > 78)
            {
                wTxt.Text = "78";
            }
            else if (i < 8)
            {
                wTxt.Text = "8";
            }
        }

        //check if the width box is clicked
        private void bTxt_TextFocus(object sender, System.EventArgs e)
        {
            //check the custom button
            custBtn.IsChecked = true;
        }
        //if the width box value is changed to something it cant be default to something it can be
        private void bTxt_TextChanged(object sender, RoutedEventArgs e)
        {
            //Check if the data is actually suitable
            if (bTxt.Text == "")
            {
                bTxt.Text = "1";
            }
            else if (!int.TryParse(bTxt.Text, out int i))
            {
                bTxt.Text = "1";
            }
            else if (i < 1)
            {
                //if the number is too small make it as big as it is required to be
                if (i < (int.Parse(wTxt.Text) * 2) && int.Parse(hTxt.Text) * int.Parse(wTxt.Text) > 1200)
                {
                    bTxt.Text = (int.Parse(wTxt.Text) * 2).ToString();
                }
                else
                {
                    bTxt.Text = "1";
                }
            }
            else if (i > (int.Parse(hTxt.Text) * int.Parse(wTxt.Text)) - 1)
            {
                //if the number is too big make it the maximum value or cap it to 999
                i = (int.Parse(hTxt.Text) * int.Parse(wTxt.Text) - 1);

                if (i > 999)
                {
                    bTxt.Text = "999";
                }
                else
                {
                    bTxt.Text = i.ToString();
                }
            }

        }
    }
}
