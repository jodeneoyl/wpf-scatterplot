using System.Windows;
using System.Windows.Controls;
using ScatterPlot.ViewModel;

namespace ScatterPlot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ChartViewModel _chartViewModel;
        public MainWindow()
        {
            InitializeComponent();
            _chartViewModel = new ChartViewModel(this);
            DataContext = _chartViewModel;
            InitializeColorControls();
        }


        #region Event Handlers  
        private void ClickResetZoom(object sender, RoutedEventArgs e)
        {
            ResetZoom();
        }
        #endregion


        #region Helper Functions  
        //Generates the color control panel according to the number of series
        public void InitializeColorControls()
        {
            //Clear the panel before generating new elements
            ColorControlPanel.Children.Clear(); 

            for (int i = 0; i < _chartViewModel.SeriesCollection.Count; i++)
            {
                var colorLabel = new Label
                {
                    Name = $"SeriesName{i}",
                    Content = $"{_chartViewModel.SeriesCollection[i].Title} series colour:",
                    Width = double.NaN //For full width
                };

                //Drop down list with predefined color selections
                ComboBox colorComboBox = new ComboBox
                {
                    Name = $"ComboName{i}",
                    ItemsSource = new[] { "Red", "Orange", "Yellow", "Green", "Blue", "Purple", "Pink" },
                    Width = double.NaN, //For full width
                    IsEditable = true,
                    IsReadOnly = true,
                    Text = "Please Select" //A default placeholder
                };

                colorComboBox.SelectionChanged += _chartViewModel.SelectionChangedColor;

                ColorControlPanel.Children.Add(colorLabel);
                ColorControlPanel.Children.Add(colorComboBox);
            }
        }

        private void ClickImport(object sender, RoutedEventArgs e)
        {
            //Refresh the color control panels when new data is imported 
            InitializeColorControls();
        }

        private void ResetZoom()
        {
            //Clears the values in the axes
            X.MinValue = double.NaN;
            X.MaxValue = double.NaN;
            Y.MinValue = double.NaN;
            Y.MaxValue = double.NaN;
        }

        #endregion
    }

}
