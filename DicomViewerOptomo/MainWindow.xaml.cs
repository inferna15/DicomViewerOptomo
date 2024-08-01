using System.Windows;
using Kitware.VTK;

namespace DicomViewerOptomo
{
    
    public partial class MainWindow : Window
    {
        private int isStart = 0;
        private int[] extent = new int[6];
        private double[] spacing = new double[3];
        private double[] origin = new double[3];
        private double[] center = new double[3];
        private double[] oran = new double[3]; // incelenecek
        private vtkImageReslice[] reslices = new vtkImageReslice[3];
        private vtkImageMapper[] mappers = new vtkImageMapper[3];
        private vtkLineSource[] lines = new vtkLineSource[6];
        private vtkRenderer[] renderers = new vtkRenderer[3];
        private double[,,] linePos = new double[6,2,2];

        public MainWindow()
        {
            InitializeComponent();
            Init("C:/Users/fatil/OneDrive/Belgeler/Dicoms/tavuk");
            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SizeChanged(object sender, EventArgs e)
        {
            if (isStart == 1)
            {
                Y_Slider.Maximum = extent[3];
                Y_Slider.Minimum = extent[2];
                Y_Slider.Value = extent[2];
                X_Slider.Maximum = extent[1];
                X_Slider.Minimum = extent[0];
                X_Slider.Value = extent[0];
                Z_Slider.Maximum = extent[5];
                Z_Slider.Minimum = extent[4];
                Z_Slider.Value = extent[4];

                int width = (int)Y_Slice.ActualWidth;
                int height = (int)Y_Slice.ActualHeight;
                int min = Math.Min(width, height);

                if (min == height)
                {
                    int ext = extent[1] * min / extent[5];
                    int offset = (width - ext) / 2;
                    oran[0] = (double)min / (double)extent[5];
                    reslices[0].SetOutputExtent(offset, ext + offset, 0, min, 0, 0);
                    reslices[0].SetOutputSpacing(spacing[0] / oran[0], spacing[2] / oran[0], spacing[1] / oran[0]);
                    // Z
                    linePos[0, 0, 0] = offset;
                    linePos[0, 1, 0] = ext + offset;
                    linePos[0, 0, 1] = 0;
                    linePos[0, 1, 1] = 0;
                    // X
                    linePos[1, 0, 0] = offset;
                    linePos[1, 1, 0] = offset;
                    linePos[1, 0, 1] = 0;
                    linePos[1, 1, 1] = min;

                    ext = extent[5] * min / extent[3];
                    offset = (width - ext) / 2;
                    oran[1] = (double)min / (double)extent[3];
                    reslices[1].SetOutputExtent(offset, ext + offset, 0, min, 0, 0);
                    reslices[1].SetOutputSpacing(spacing[1] / oran[1], spacing[2] / oran[1], spacing[0]);
                    // Z
                    linePos[2, 0, 0] = offset;
                    linePos[2, 1, 0] = ext + offset;
                    linePos[2, 0, 1] = 0;
                    linePos[2, 1, 1] = 0;
                    // Y
                    linePos[3, 0, 0] = offset;
                    linePos[3, 1, 0] = offset;
                    linePos[3, 0, 1] = 0;
                    linePos[3, 1, 1] = min;

                    ext = extent[1] * min / extent[3];
                    offset = (width - ext) / 2;
                    oran[2] = (double)min / (double)extent[3];
                    reslices[2].SetOutputExtent(offset, ext + offset, 0, min, 0, 0);
                    reslices[2].SetOutputSpacing(spacing[0] / oran[2], spacing[1] / oran[2], spacing[2]);
                    // Y
                    linePos[4, 0, 0] = offset;
                    linePos[4, 1, 0] = ext + offset;
                    linePos[4, 0, 1] = 0;
                    linePos[4, 1, 1] = 0;
                    // X
                    linePos[5, 0, 0] = offset;
                    linePos[5, 1, 0] = offset;
                    linePos[5, 0, 1] = 0;
                    linePos[5, 1, 1] = min;

                    for (int i = 0; i < 6; i++)
                    {
                        lines[i] = new vtkLineSource();
                        lines[i].SetPoint1(linePos[i, 0, 0], linePos[i, 0, 1], 0);
                        lines[i].SetPoint2(linePos[i, 1, 0], linePos[i, 1, 1], 0);
                    }

                    for (int i = 0; i < 6; i++)
                    {
                        vtkPolyDataMapper2D lineMapper = new vtkPolyDataMapper2D();
                        vtkActor2D lineActor = new vtkActor2D();
                        lineMapper.SetInputConnection(lines[i].GetOutputPort());
                        lineActor.SetMapper(lineMapper);
                        lineActor.GetProperty().SetColor(0.5, 0.5, 0.5);
                        renderers[i / 2].AddActor(lineActor);
                    }

                }
            }
            else
            {
                isStart++;
            }
        }
        private void Init(string path)
        {
            vtkDICOMImageReader reader = new vtkDICOMImageReader();
            reader.SetDirectoryName(path);
            reader.Update();

            extent = reader.GetDataExtent();
            spacing = reader.GetDataSpacing();
            origin = reader.GetDataOrigin();

            center = new double[]
            {
                origin[0] + spacing[0] * 0.5 * (extent[0] + extent[1]),
                origin[1] + spacing[1] * 0.5 * (extent[2] + extent[3]),
                origin[2] + spacing[2] * 0.5 * (extent[4] + extent[5])
            };

            InitYReslice(reader.GetOutputPort());
            InitXReslice(reader.GetOutputPort());
            InitZReslice(reader.GetOutputPort());
        }

        #region Layer Değerleri
        private void Y_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)Y_Slider.Value;
            reslices[0].SetResliceAxesOrigin(center[0], value * spacing[1], center[2]);
            Y_Layer.Text = extent[3].ToString() + " / " + value.ToString();
            double l3 = linePos[3, 0, 0] + (oran[1] * (double)value);
            double l4 = linePos[4, 0, 1] + (oran[2] * (double)value);
            lines[3].SetPoint1(l3, linePos[3, 0, 1], 0);
            lines[3].SetPoint2(l3, linePos[3, 1, 1], 0);
            lines[4].SetPoint1(linePos[4, 0, 0], l4, 0);
            lines[4].SetPoint2(linePos[4, 1, 0], l4, 0);
            Y_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Y_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(Y_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Y_Slice.Child), null);
            X_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(X_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(X_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(X_Slice.Child), null);
            Z_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Z_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(Z_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Z_Slice.Child), null);

        }

        private void X_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)X_Slider.Value;
            reslices[1].SetResliceAxesOrigin(value * spacing[0], center[1], center[2]);
            X_Layer.Text = extent[1].ToString() + " / " + value.ToString();
            double l1 = linePos[1, 0, 0] + (oran[0] * (double)value);
            double l5 = linePos[5, 0, 0] + (oran[2] * (double)value);
            lines[1].SetPoint1(l1, linePos[1, 0, 1], 0);
            lines[1].SetPoint2(l1, linePos[1, 1, 1], 0);
            lines[5].SetPoint1(l5, linePos[5, 0, 1], 0);
            lines[5].SetPoint2(l5, linePos[5, 1, 1], 0);
            Y_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Y_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(Y_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Y_Slice.Child), null);
            X_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(X_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(X_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(X_Slice.Child), null);
            Z_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Z_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(Z_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Z_Slice.Child), null);
        }

        private void Z_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)Z_Slider.Value;
            reslices[2].SetResliceAxesOrigin(center[0], center[1], value * spacing[2]);
            Z_Layer.Text = extent[5].ToString() + " / " + value.ToString();
            double l0 = linePos[1, 0, 1] + (oran[0] * (double)value);
            double l2 = linePos[5, 0, 1] + (oran[1] * (double)value);
            lines[0].SetPoint1(linePos[0, 0, 0], l0, 0);
            lines[0].SetPoint2(linePos[0, 1, 0], l0, 0);
            lines[2].SetPoint1(linePos[2, 0, 0], l2, 0);
            lines[2].SetPoint2(linePos[2, 1, 0], l2, 0);
            Y_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Y_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(Y_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Y_Slice.Child), null);
            X_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(X_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(X_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(X_Slice.Child), null);
            Z_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Z_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(Z_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Z_Slice.Child), null);
        }
        #endregion

        #region Init Reslices
        private void InitYReslice(vtkAlgorithmOutput output)
        {
            var renderWindowControl = new RenderWindowControl();

            renderWindowControl.Load += (sender, args) =>
            {
                vtkImageReslice reslice = new vtkImageReslice();
                reslice.SetInputConnection(output);
                reslice.SetOutputDimensionality(2);
                reslice.SetResliceAxesDirectionCosines(1, 0, 0, 0, 0, 1, 0, 1, 0);
                reslice.SetResliceAxesOrigin(center[0], 0, center[2]);

                reslices[0] = reslice;

                reslice.GetResliceTransform();

                vtkImageMapper mapper = new vtkImageMapper();
                mapper.SetInputConnection(reslice.GetOutputPort());
                mapper.SetColorWindow(1000);
                mapper.SetColorLevel(500);
                mappers[0] = mapper;

                vtkActor2D actor = new vtkActor2D();
                actor.SetMapper(mapper);

                vtkRenderer renderer = new vtkRenderer();
                renderer.AddActor(actor);
                renderer.SetBackground(0.0, 0.0, 0.0);
                renderers[0] = renderer;

                renderWindowControl.RenderWindow.AddRenderer(renderer);
            };
            Y_Slice.Child = renderWindowControl;
            Y_Layer.Text = extent[3].ToString() + " / 0";
        }

        private void InitXReslice(vtkAlgorithmOutput output)
        {
            var renderWindowControl = new RenderWindowControl();

            renderWindowControl.Load += (sender, args) =>
            {
                vtkImageReslice reslice = new vtkImageReslice();
                reslice.SetInputConnection(output);
                reslice.SetOutputDimensionality(2);
                reslice.SetResliceAxesDirectionCosines(0, 1, 0, 0, 0, 1, 1, 0, 0);
                reslice.SetResliceAxesOrigin(0, center[1], center[2]);

                reslices[1] = reslice;

                reslice.GetResliceTransform();

                vtkImageMapper mapper = new vtkImageMapper();
                mapper.SetInputConnection(reslice.GetOutputPort());
                mapper.SetColorWindow(1000);
                mapper.SetColorLevel(500);
                mappers[1] = mapper;

                vtkActor2D actor = new vtkActor2D();
                actor.SetMapper(mapper);

                vtkRenderer renderer = new vtkRenderer();
                renderer.AddActor(actor);
                renderer.SetBackground(0.0, 0.0, 0.0);
                renderers[1] = renderer;

                renderWindowControl.RenderWindow.AddRenderer(renderer);
            };
            X_Slice.Child = renderWindowControl;
            X_Layer.Text = extent[1].ToString() + " / 0";
        }

        private void InitZReslice(vtkAlgorithmOutput output)
        {
            var renderWindowControl = new RenderWindowControl();

            renderWindowControl.Load += (sender, args) =>
            {
                vtkImageReslice reslice = new vtkImageReslice();
                reslice.SetInputConnection(output);
                reslice.SetOutputDimensionality(2);
                reslice.SetResliceAxesDirectionCosines(1, 0, 0, 0, 1, 0, 0, 0, 1);
                reslice.SetResliceAxesOrigin(center[0], center[1], 0);

                reslices[2] = reslice;

                reslice.GetResliceTransform();

                vtkImageMapper mapper = new vtkImageMapper();
                mapper.SetInputConnection(reslice.GetOutputPort());
                mapper.SetColorWindow(1000);
                mapper.SetColorLevel(500);
                mappers[2] = mapper;

                vtkActor2D actor = new vtkActor2D();
                actor.SetMapper(mapper);

                vtkRenderer renderer = new vtkRenderer();
                renderer.AddActor(actor);
                renderer.SetBackground(0.0, 0.0, 0.0);
                renderers[2] = renderer;

                renderWindowControl.RenderWindow.AddRenderer(renderer);
            };
            Z_Slice.Child = renderWindowControl;
            Z_Layer.Text = extent[5].ToString() + " / 0";
        }
        #endregion

        #region Pencere Tuşları
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void minButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        #endregion

        #region Color Değerleri
        private void Y_Color_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            mappers[0].SetColorWindow(value * 10);
            mappers[0].SetColorLevel(value * 5);
            Y_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Y_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(Y_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Y_Slice.Child), null);
        }

        private void X_Color_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            mappers[1].SetColorWindow(value * 10);
            mappers[1].SetColorLevel(value * 5);
            X_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(X_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(X_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(X_Slice.Child), null);
        }

        private void Z_Color_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            mappers[2].SetColorWindow(value * 10);
            mappers[2].SetColorLevel(value * 5);
            Z_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Z_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(Z_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Z_Slice.Child), null);
        }
        #endregion

    }
}