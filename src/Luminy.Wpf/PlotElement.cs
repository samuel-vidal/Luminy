using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Luminy.Model;

namespace Luminy.Wpf
{
    /// <summary>
    /// Base class for all visual data series in Luminy.Wpf.
    /// </summary>
    public abstract class PlotElement : FrameworkElement
    {
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(PlotElement),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(
                nameof(Color),
                typeof(System.Windows.Media.Color?),
                typeof(PlotElement),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary> Gets or sets the descriptive series label used in legends. </summary>
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary> Gets or sets the primary series color. </summary>
        public System.Windows.Media.Color? Color
        {
            get => (System.Windows.Media.Color?)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        /// <summary> Calculates the bounding box covered by this plot's data. </summary>
        public abstract (Luminy.Model.Range xRange, Luminy.Model.Range yRange) GetValueRange();

        /// <summary> Renders the plot series using high-performance DirectX DrawingContext. </summary>
        public abstract void Render(DrawingContext dc, CoordinateMap map, Rect plotBounds, System.Windows.Media.Color defaultColor);

        /// <summary> Gets legend entries for this plot series. </summary>
        public virtual IEnumerable<(string Label, System.Windows.Media.Color Color)> GetLegendItems(System.Windows.Media.Color defaultColor)
        {
            if (!string.IsNullOrEmpty(Label))
            {
                yield return (Label, Color ?? defaultColor);
            }
        }
    }
}
