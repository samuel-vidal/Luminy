import { BasePlotProps, PlotComponent, Range } from '../types';
import { useDisplayContext } from '../context/DisplayContext';

export interface HistogramPlotProps extends BasePlotProps {
  data: number[];
  binCount?: number;
  stroke?: string;
  strokeWidth?: number;
  className?: string;
}

export interface HistogramBinsResult {
  min: number;
  max: number;
  binWidth: number;
  bins: number[];
}

export function computeHistogramBins(data: number[], binCount: number = 10): HistogramBinsResult {
  if (!data || data.length === 0) {
    return { min: 0, max: 0, binWidth: 1, bins: [] };
  }

  let min = Infinity;
  let max = -Infinity;
  for (let i = 0; i < data.length; i++) {
    const val = data[i];
    if (Number.isFinite(val)) {
      if (val < min) min = val;
      if (val > max) max = val;
    }
  }

  if (!Number.isFinite(min) || !Number.isFinite(max)) {
    return { min: 0, max: 0, binWidth: 1, bins: [] };
  }

  let span = max - min;
  if (span < 1e-10) {
    min -= 0.5;
    max += 0.5;
    span = 1.0;
  }

  const effectiveBins = Math.max(1, binCount);
  const binWidth = span / effectiveBins;
  const bins = new Array<number>(effectiveBins).fill(0);

  for (let i = 0; i < data.length; i++) {
    const val = data[i];
    if (Number.isFinite(val)) {
      let binIndex = Math.floor((val - min) / binWidth);
      if (binIndex >= effectiveBins) binIndex = effectiveBins - 1;
      if (binIndex < 0) binIndex = 0;
      bins[binIndex]++;
    }
  }

  return { min, max, binWidth, bins };
}

export const HistogramPlot: PlotComponent<HistogramPlotProps> = ({
  data,
  binCount = 10,
  color,
  stroke = 'rgba(255, 255, 255, 0.25)',
  strokeWidth = 1,
  className
}) => {
  const { map, getNextColor } = useDisplayContext();
  const effectiveColor = color || getNextColor();

  if (!data || data.length === 0) return null;

  const { min, binWidth, bins } = computeHistogramBins(data, binCount);

  return (
    <g className={`luminy-histogram-plot ${className || ''}`.trim()}>
      {bins.map((count, i) => {
        const xLeft = min + i * binWidth;
        const xRight = xLeft + binWidth;

        const [screenLeft, screenBottom] = map(xLeft, 0);
        const [screenRight, screenTop] = map(xRight, count);

        const rectX = Math.min(screenLeft, screenRight);
        const rectY = Math.min(screenTop, screenBottom);
        const rectW = Math.abs(screenRight - screenLeft);
        const rectH = Math.abs(screenBottom - screenTop);

        return (
          <rect
            key={i}
            x={rectX}
            y={rectY}
            width={rectW}
            height={rectH}
            fill={effectiveColor}
            stroke={stroke}
            strokeWidth={strokeWidth}
          />
        );
      })}
    </g>
  );
};

HistogramPlot.displayName = 'HistogramPlot';

HistogramPlot.getRange = (props: HistogramPlotProps): { xRange: Range; yRange: Range } => {
  if (!props.data || props.data.length === 0) {
    return { xRange: { min: 0, max: 0 }, yRange: { min: 0, max: 0 } };
  }

  const { min, max, bins } = computeHistogramBins(props.data, props.binCount || 10);
  const maxBin = Math.max(0, ...bins);

  return {
    xRange: { min, max },
    yRange: { min: 0, max: maxBin }
  };
};

HistogramPlot.getLegendItems = (props: HistogramPlotProps, fallbackColor: string) => {
  if (!props.label) return [];
  return [{ label: props.label, color: props.color || fallbackColor }];
};
