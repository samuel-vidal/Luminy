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

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            RegenerateSignals();
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

        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
