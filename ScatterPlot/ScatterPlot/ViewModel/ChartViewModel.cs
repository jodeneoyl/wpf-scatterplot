using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using ScatterPlot.Model;

namespace ScatterPlot.ViewModel
{
    public class ChartViewModel : INotifyPropertyChanged
    {
        #region Initialize Variables 
        private SeriesCollection _series;
        public SeriesCollection SeriesCollection
        {
            get { return _series; }
            set
            {
                _series = value;
                OnPropertyChanged("SeriesCollection");
            }
        }

        private string _title;
        public string TitleName
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("TitleName");
            }
        }

        private string _xAxis;
        public string XAxisName
        {
            get { return _xAxis; }
            set
            {
                _xAxis = value;
                OnPropertyChanged("XAxisName");
            }
        }

        private string _yAxis;
        public string YAxisName
        {
            get { return _yAxis; }
            set
            {
                _yAxis = value;
                OnPropertyChanged("YAxisName");
            }
        }

        private ZoomingOptions _zoomingMode;
        public ZoomingOptions ZoomingMode
        {
            get { return _zoomingMode; }
            set
            {
                _zoomingMode = value;
                OnPropertyChanged("ZoomingMode");
            }
        }

        private readonly MainWindow _mainWindow;
        #endregion
        
        #region Initialize RelayCommand  
        //Initialize commands for application interaction
        public RelayCommand ChangeTitleCommand { get; set; }
        public RelayCommand ImportCommand { get; set; }
        public RelayCommand RandomizeCommand { get; set; }
        public RelayCommand ToggleZoomCommand { get; set; }
        #endregion

        //Constructor for class ChartViewModel 
        public ChartViewModel(MainWindow main)
        {
            _mainWindow = main;

            //Preset Zoom as XY
            ZoomingMode = ZoomingOptions.Xy;

            //Initialize a default dummy data set
            InitializeSeriesCollection();

            //Commands for application interactions
            ChangeTitleCommand = new RelayCommand(p => UpdateTitle(_title));
            ImportCommand = new RelayCommand(p => ImportCsv());
            RandomizeCommand = new RelayCommand(p => GenerateRandom());
            ToggleZoomCommand = new RelayCommand(p => ToggleZoom());
        }

        //Initialize a set of dummy data set for initial launch
        private void InitializeSeriesCollection()
        {
            TitleName = "Pocket Money Trace";
            XAxisName = "Received ($)";
            YAxisName = "Spent ($)";

            //Chart data points and other information in SeriesCollection object
            SeriesCollection = new SeriesCollection
            {
                //Series 1
                new ScatterSeries
                {
                    Title = "Aby",
                    Values = new ChartValues<ScatterPoint>
                    {
                        new ScatterPoint(6, 5, 1),
                        new ScatterPoint(5, 4, 1),
                        new ScatterPoint(7, 4, 1),
                        new ScatterPoint(5, 7, 1),
                        new ScatterPoint(8, 3, 1)
                    }
                },
                //Series 2
                new ScatterSeries
                {
                    Title = "Rin",
                    Values = new ChartValues<ScatterPoint>
                    {
                        new ScatterPoint(7, 5, 1),
                        new ScatterPoint(6, 5, 1),
                        new ScatterPoint(11, 9, 1),
                        new ScatterPoint(6, 4, 1),
                        new ScatterPoint(8, 8, 1)
                    }
                },
                //Series 3
                new ScatterSeries
                {
                    Title = "Jil",
                    Values = new ChartValues<ScatterPoint>
                    {
                        new ScatterPoint(12, 8, 1),
                        new ScatterPoint(13, 5, 1),
                        new ScatterPoint(11, 5, 1),
                        new ScatterPoint(10, 6, 1),
                        new ScatterPoint(10, 7, 1)
                    }
                }
            };
        }

        //Process data imported from CSV into SeriesCollection object
        private void ProcessSeriesCollection(List<Data> dataList)
        {
            //Group data by distinct Series
            var groupedData = (dataList.GroupBy(g => g.Series).Select(g => new
            {
                Series = g.Key,
                Data = g.Select(s => new ScatterPoint
                {
                    X = s.X,
                    Y = s.Y,
                    Weight = s.Weight
                }).ToList()
            })).ToList();

            SeriesCollection = new SeriesCollection();

            foreach (var series in groupedData)
            {
                var title = series.Series;
                var values = series.Data;

                ScatterSeries ss = new ScatterSeries {Title = title};

                ChartValues<ScatterPoint> cv = new ChartValues<ScatterPoint>();
                foreach (var val in values)
                {
                    cv.Add(val);
                }

                ss.Values = cv;
                SeriesCollection.Add(ss);
            }

        }

        //Event handler for color control combo box
        public void SelectionChangedColor(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combobox)
            {
                var colour = combobox.SelectedItem.ToString();
                UpdateColor(combobox.Name, colour);
            }
        }

        //Update color selection for respective series
        public void UpdateColor(string objectName, string color)
        {
            //Series determined according to the index number
            var index = 0;
            var regex = new Regex(@"\d+");
            var match = regex.Match(objectName);

            if (match.Success)
            {
                index = int.Parse(match.Value);
            }

            switch (color)
            {
                case "Red":
                    ((ScatterSeries)SeriesCollection[index]).Fill = Brushes.IndianRed;
                    break;
                case "Orange":
                    ((ScatterSeries)SeriesCollection[index]).Fill = Brushes.LightSalmon;
                    break;
                case "Yellow":
                    ((ScatterSeries)SeriesCollection[index]).Fill = Brushes.Wheat;
                    break;
                case "Green":
                    ((ScatterSeries)SeriesCollection[index]).Fill = Brushes.DarkSeaGreen;
                    break;
                case "Blue":
                    ((ScatterSeries)SeriesCollection[index]).Fill = Brushes.CornflowerBlue;
                    break;
                case "Purple":
                    ((ScatterSeries)SeriesCollection[index]).Fill = Brushes.Plum;
                    break;
                case "Pink":
                    ((ScatterSeries)SeriesCollection[index]).Fill = Brushes.LightPink;
                    break;
            }
        }

        //Import and read external data source in CSV format
        private void ImportCsv()
        {
            string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent?.FullName;

            //Dialog for file selection
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            if (projectDirectory != null) openFileDialog.InitialDirectory = projectDirectory;

            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                //Update title name according to the file name
                var fileName = openFileDialog.FileName;
                UpdateTitle(openFileDialog.SafeFileName.Split('.')[0]); //remove file extension
                using (TextFieldParser parser = new TextFieldParser(fileName, Encoding.Default))
                {
                    //Delimited by comma (,)
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    bool firstLine = true;

                    var dataList = new List<Data>();

                    while (!parser.EndOfData)
                    {
                        //Process row
                        string[] fields = parser.ReadFields();
                        if (fields != null)
                        {
                            // get the column headers
                            if (firstLine)
                            {
                                //Update axes names
                                XAxisName = fields[1];
                                YAxisName = fields[2];
                                firstLine = false;
                                continue;
                            }

                            //Process data into Data object
                            Data data = new Data();
                            if (fields[0] != null) data.Series = fields[0];
                            if (fields[1] != null) data.X = Convert.ToDouble(fields[1]);
                            if (fields[2] != null) data.Y = Convert.ToDouble(fields[2]);
                            if (fields.Length > 3)
                                if (fields[3] != null)
                                    data.Weight = Convert.ToDouble(fields[3]);
                            dataList.Add(data);
                        }
                    }

                    ProcessSeriesCollection(dataList);

                    _mainWindow.InitializeColorControls();
                }
            }
        }

        //Generates a set of random values for each data point
        private void GenerateRandom()
        {
            var r = new Random();
            foreach (var series in SeriesCollection)
            {
                foreach (var bubble in series.Values.Cast<ScatterPoint>())
                {
                    //Rounded off to 2dp
                    bubble.X = Math.Round((r.NextDouble() * 10), 2);
                    bubble.Y = Math.Round((r.NextDouble() * 10), 2);
                    bubble.Weight = 1;
                }
            }
        }

        //Toggles zoom mode with X, Y, XY, and None options
        private void ToggleZoom()
        {
            switch (ZoomingMode)
            {
                case ZoomingOptions.None:
                    ZoomingMode = ZoomingOptions.X;
                    break;
                case ZoomingOptions.X:
                    ZoomingMode = ZoomingOptions.Y;
                    break;
                case ZoomingOptions.Y:
                    ZoomingMode = ZoomingOptions.Xy;
                    break;
                case ZoomingOptions.Xy:
                    ZoomingMode = ZoomingOptions.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //Updates title of the chart 
        public void UpdateTitle(string title)
        {
            TitleName = title;
        }


        #region INotifyPropertyChanged Members  
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    //Helper class to convert ZoomingMode values for view
    public class ZoomingModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ZoomingOptions)value)
            {
                case ZoomingOptions.None:
                    return "None";
                case ZoomingOptions.X:
                    return "X";
                case ZoomingOptions.Y:
                    return "Y";
                case ZoomingOptions.Xy:
                    return "XY";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
