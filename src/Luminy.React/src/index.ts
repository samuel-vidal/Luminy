// Components
export { Display } from './components/Display';
export type { DisplayProps } from './components/Display';

export { LinePlot } from './components/LinePlot';
export type { LinePlotProps } from './components/LinePlot';

export { DonutPlot } from './components/DonutPlot';
export type { DonutPlotProps } from './components/DonutPlot';

export { PointPlot } from './components/PointPlot';
export type { PointPlotProps } from './components/PointPlot';

export { BarChart, calculateDefaultBarWidth } from './components/BarChart';
export type { BarChartProps } from './components/BarChart';

export { HistogramPlot, computeHistogramBins } from './components/HistogramPlot';
export type { HistogramPlotProps, HistogramBinsResult } from './components/HistogramPlot';

export { BoxPlot, computeBoxPlotStats } from './components/BoxPlot';
export type { BoxPlotProps, BoxPlotStats } from './components/BoxPlot';

export { MatrixPlot, getMatrixDimensions, getMatrixValueExtremes } from './components/MatrixPlot';
export type { MatrixPlotProps } from './components/MatrixPlot';

// Context & Hooks
export { useDisplayContext } from './context/DisplayContext';
export type { DisplayContextValue } from './context/DisplayContext';

// Types
export type {
  Range,
  CoordinateMap,
  AxesStyle,
  ScaleType,
  LegendPosition,
  BasePlotProps,
  LegendItem,
  PlotComponent,
  PlotElement,
  PlotChildren
} from './types';

// Utilities
export { calculateNiceTicks, formatTickLabel } from './utils/ticks';
export { getArrayRange, combineRanges } from './utils/range';
export { DEFAULT_PALETTE, parseColor, interpolateColor } from './utils/palette';
