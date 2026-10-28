using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Luminy.Wpf.Demo
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private double frequency = 2.0;
        private double amplitude = 1.0;
        private double noise = 0.15;
        private bool showGrid = true;
        private bool showLegend = true;

        private float[] timePoints = [];
        private float[] sineSignal = [];
        private float[] cosineSignal = [];
        private float[] noisySignal = [];

        private float[] donutValues = [45f, 25f, 18f, 10f, 2f];
        private string[] donutLabels = ["Desktop", "Mobile", "Tablet", "Console", "Other"];
        private float donutInternalRadius = 0.6f;

        private float[] clusterAX = [];
        private float[] clusterAY = [];
        private float[] clusterBX = [];
        private float[] clusterBY = [];
        private int scatterSpread = 10;
        private double pointSize = 4.0;

        private float[] barX = [1f, 2f, 3f, 4f, 5f];
        private float[] barHeights = [42f, 85f, 120f, 98f, 64f];
        private float barWidth = 0.65f;

        private float[] histogramData = [];
        private int histogramBins = 25;

        private float[] box1Data = [];
        private float[] box2Data = [];
        private float[] box3Data = [];
        private float boxWidth = 0.6f;

        private float[,] matrixData = new float[12, 12];
        private double matrixFrequency = 2.5;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            RegenerateSignals();
            RegenerateScatter();
            RegenerateHistogram();
            RegenerateBoxes();
            RegenerateMatrix();
        }

        public double Frequency
        {
            get => frequency;
            set
            {
                if (SetProperty(ref frequency, value))
                    RegenerateSignals();
            }
        }

        public double Amplitude
        {
            get => amplitude;
            set
            {
                if (SetProperty(ref amplitude, value))
                    RegenerateSignals();
            }
        }

        public double Noise
        {
            get => noise;
            set
            {
                if (SetProperty(ref noise, value))
                    RegenerateSignals();
            }
        }

        public bool ShowGrid
        {
            get => showGrid;
            set => SetProperty(ref showGrid, value);
        }

        public bool ShowLegend
        {
            get => showLegend;
            set => SetProperty(ref showLegend, value);
        }

        public float[] TimePoints
        {
            get => timePoints;
            private set => SetProperty(ref timePoints, value);
        }

        public float[] SineSignal
        {
            get => sineSignal;
            private set => SetProperty(ref sineSignal, value);
        }

        public float[] CosineSignal
        {
            get => cosineSignal;
            private set => SetProperty(ref cosineSignal, value);
        }

        public float[] NoisySignal
        {
            get => noisySignal;
            private set => SetProperty(ref noisySignal, value);
        }

        public float[] DonutValues
        {
            get => donutValues;
            set => SetProperty(ref donutValues, value);
        }

        public string[] DonutLabels
        {
            get => donutLabels;
            set => SetProperty(ref donutLabels, value);
        }

        public float DonutInternalRadius
        {
            get => donutInternalRadius;
            set => SetProperty(ref donutInternalRadius, value);
        }

        public int ScatterSpread
        {
            get => scatterSpread;
            set
            {
                if (SetProperty(ref scatterSpread, value))
                    RegenerateScatter();
            }
        }

        public double PointSize
        {
            get => pointSize;
            set => SetProperty(ref pointSize, value);
        }

        public float[] ClusterAX
        {
            get => clusterAX;
            private set => SetProperty(ref clusterAX, value);
        }

        public float[] ClusterAY
        {
            get => clusterAY;
            private set => SetProperty(ref clusterAY, value);
        }

        public float[] ClusterBX
        {
            get => clusterBX;
            private set => SetProperty(ref clusterBX, value);
        }

        public float[] ClusterBY
        {
            get => clusterBY;
            private set => SetProperty(ref clusterBY, value);
        }

        public float[] BarX
        {
            get => barX;
            set => SetProperty(ref barX, value);
        }

        public float[] BarHeights
        {
            get => barHeights;
            set => SetProperty(ref barHeights, value);
        }

        public float BarWidth
        {
            get => barWidth;
            set => SetProperty(ref barWidth, value);
        }

        public float[] HistogramData
        {
            get => histogramData;
            private set => SetProperty(ref histogramData, value);
        }

        public int HistogramBins
        {
            get => histogramBins;
            set => SetProperty(ref histogramBins, value);
        }

        public float[] Box1Data
        {
            get => box1Data;
            private set => SetProperty(ref box1Data, value);
        }

        public float[] Box2Data
        {
            get => box2Data;
            private set => SetProperty(ref box2Data, value);
        }

        public float[] Box3Data
        {
            get => box3Data;
            private set => SetProperty(ref box3Data, value);
        }

        public float BoxWidth
        {
            get => boxWidth;
            set => SetProperty(ref boxWidth, value);
        }

        public float[,] MatrixData
        {
            get => matrixData;
            private set => SetProperty(ref matrixData, value);
        }

        public double MatrixFrequency
        {
            get => matrixFrequency;
            set
            {
                if (SetProperty(ref matrixFrequency, value))
                    RegenerateMatrix();
            }
        }

        private void RegenerateSignals()
        {
            const int count = 150;
            var t = new float[count];
            var sin = new float[count];
            var cos = new float[count];
            var noisy = new float[count];

            var step = 4f * MathF.PI / count;
            for (var i = 0; i < count; i++)
            {
                var time = i * step;
                t[i] = time;
                var s = (float)amplitude * MathF.Sin((float)frequency * time);
                var c = (float)amplitude * MathF.Cos((float)frequency * time);
                var n = (MathF.Sin(i * 12.9898f) * 43758.5453f) % 1f;

                sin[i] = s;
                cos[i] = c;
                noisy[i] = s + (n - 0.5f) * 2f * (float)noise;
            }

            TimePoints = t;
            SineSignal = sin;
            CosineSignal = cos;
            NoisySignal = noisy;
        }

        private void RegenerateScatter()
        {
            const int n = 70;
            var ax = new float[n];
            var ay = new float[n];
            var bx = new float[n];
            var by = new float[n];
            var spreadFactor = scatterSpread / 10f;

            for (var i = 0; i < n; i++)
            {
                var u1 = ((i * 17 + 5) % 100) / 100f;
                var u2 = ((i * 31 + 13) % 100) / 100f;
                var r1 = MathF.Sqrt(-2f * MathF.Log(MathF.Max(1e-5f, u1))) * MathF.Cos(2f * MathF.PI * u2);
                var r2 = MathF.Sqrt(-2f * MathF.Log(MathF.Max(1e-5f, u1))) * MathF.Sin(2f * MathF.PI * u2);

                ax[i] = 2.0f + r1 * 0.8f * spreadFactor;
                ay[i] = 3.0f + r2 * 0.8f * spreadFactor;

                bx[i] = 6.0f + r2 * 1.1f * spreadFactor;
                by[i] = 7.0f + r1 * 1.1f * spreadFactor;
            }

            ClusterAX = ax;
            ClusterAY = ay;
            ClusterBX = bx;
            ClusterBY = by;
        }

        private void RegenerateHistogram()
        {
            const int total = 600;
            var data = new float[total];

            for (var i = 0; i < total; i++)
            {
                var u1 = ((i * 23 + 7) % 100) / 100f;
                var u2 = ((i * 47 + 19) % 100) / 100f;
                var g = MathF.Sqrt(-2f * MathF.Log(MathF.Max(1e-5f, u1))) * MathF.Cos(2f * MathF.PI * u2);

                data[i] = (i < total / 2) ? (3.0f + g * 0.9f) : (7.5f + g * 1.3f);
            }

            HistogramData = data;
        }

        private void RegenerateBoxes()
        {
            box1Data = [-1.5f, 0.2f, 0.5f, 0.8f, 1.1f, 1.2f, 1.5f, 1.7f, 2.0f, 2.4f, 4.2f];
            box2Data = [2.1f, 3.0f, 3.4f, 3.8f, 4.1f, 4.2f, 4.5f, 4.9f, 5.2f, 5.8f, 7.5f];
            box3Data = [4.5f, 5.5f, 5.8f, 6.2f, 6.5f, 6.8f, 7.1f, 7.4f, 7.9f, 8.5f, 9.8f];
        }

        private void RegenerateMatrix()
        {
            const int size = 12;
            var center = (size - 1) / 2.0f;
            var m = new float[size, size];

            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    var di = (i - center) / center;
                    var dj = (j - center) / center;
                    var dist = MathF.Sqrt(di * di + dj * dj);
                    m[i, j] = MathF.Cos(dist * (float)matrixFrequency * MathF.PI) * MathF.Exp(-dist * 0.7f);
                }
            }

            MatrixData = m;
        }

        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
