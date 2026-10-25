import { BasePlotProps, PlotComponent, Range } from '../types';
import { useDisplayContext } from '../context/DisplayContext';

export interface BoxPlotProps extends BasePlotProps {
  data: number[];
  x?: number;
  width?: number;
  fill?: string;
  strokeWidth?: number;
  outlierRadius?: number;
  className?: string;
}

export interface BoxPlotStats {
  min: number;
  max: number;
  q1: number;
  median: number;
  q3: number;
  iqr: number;
  lowerWhisker: number;
  upperWhisker: number;
  outliers: number[];
}

export function computeBoxPlotStats(data: number[]): BoxPlotStats | null {
  if (!data || data.length === 0) return null;

  const valid = data.filter((v) => Number.isFinite(v));
  if (valid.length === 0) return null;

  const sorted = [...valid].sort((a, b) => a - b);
  const min = sorted[0];
  const max = sorted[sorted.length - 1];

  const q1 = sorted[Math.floor(sorted.length * 0.25)];
  const median = sorted[Math.floor(sorted.length * 0.5)];
  const q3 = sorted[Math.floor(sorted.length * 0.75)];

  const iqr = q3 - q1;
  const lowerThreshold = q1 - 1.5 * iqr;
  const upperThreshold = q3 + 1.5 * iqr;

  const lowerWhisker = sorted.find((x) => x >= lowerThreshold) ?? q1;
  let upperWhisker = q3;
  for (let i = sorted.length - 1; i >= 0; i--) {
    if (sorted[i] <= upperThreshold) {
      upperWhisker = sorted[i];
      break;
    }
  }

  const outliers = sorted.filter((x) => x < lowerWhisker || x > upperWhisker);

  return {
    min,
    max,
    q1,
    median,
    q3,
    iqr,
    lowerWhisker,
    upperWhisker,
    outliers
  };
}

export const BoxPlot: PlotComponent<BoxPlotProps> = ({
  data,
  x = 1,
  width = 0.8,
  color,
  fill = 'none',
  strokeWidth = 1.8,
  outlierRadius = 3,
  className
}) => {
  const { map, getNextColor } = useDisplayContext();
  const effectiveColor = color || getNextColor();

  const stats = computeBoxPlotStats(data);
  if (!stats) return null;

  const { q1, median, q3, lowerWhisker, upperWhisker, outliers } = stats;

  const halfW = width / 2;
  const capHalfW = width / 4;

  // Box coordinates
  const [left, q1Y] = map(x - halfW, q1);
  const [right, q3Y] = map(x + halfW, q3);
  const rectX = Math.min(left, right);
  const rectY = Math.min(q1Y, q3Y);
  const rectW = Math.abs(right - left);
  const rectH = Math.abs(q3Y - q1Y);

  // Median line
  const [medLeft, medY] = map(x - halfW, median);
  const [medRight] = map(x + halfW, median);

  // Whisker trunk (vertical)
  const [centerX, q1CenterY] = map(x, q1);
  const [, lowerWhiskerY] = map(x, lowerWhisker);
  const [, q3CenterY] = map(x, q3);
  const [, upperWhiskerY] = map(x, upperWhisker);

  // Whisker caps (horizontal)
  const [capLowLeft, capLowY] = map(x - capHalfW, lowerWhisker);
  const [capLowRight] = map(x + capHalfW, lowerWhisker);
  const [capUpLeft, capUpY] = map(x - capHalfW, upperWhisker);
  const [capUpRight] = map(x + capHalfW, upperWhisker);

  return (
    <g className={`luminy-box-plot ${className || ''}`.trim()}>
      {/* Box */}
      <rect
        x={rectX}
        y={rectY}
        width={rectW}
        height={rectH}
        fill={fill}
        stroke={effectiveColor}
        strokeWidth={strokeWidth}
      />

      {/* Median Line */}
      <line
        x1={medLeft}
        y1={medY}
        x2={medRight}
        y2={medY}
        stroke={effectiveColor}
        strokeWidth={strokeWidth + 0.8}
      />

      {/* Lower and Upper Whisker Trunks */}
      <line
        x1={centerX}
        y1={q1CenterY}
        x2={centerX}
        y2={lowerWhiskerY}
        stroke={effectiveColor}
        strokeWidth={strokeWidth}
      />
      <line
        x1={centerX}
        y1={q3CenterY}
        x2={centerX}
        y2={upperWhiskerY}
        stroke={effectiveColor}
        strokeWidth={strokeWidth}
      />

      {/* Whisker Caps */}
      <line
        x1={capLowLeft}
        y1={capLowY}
        x2={capLowRight}
        y2={capLowY}
        stroke={effectiveColor}
        strokeWidth={strokeWidth}
      />
      <line
        x1={capUpLeft}
        y1={capUpY}
        x2={capUpRight}
        y2={capUpY}
        stroke={effectiveColor}
        strokeWidth={strokeWidth}
      />

      {/* Outlier Dots */}
      {outliers.map((val, idx) => {
        const [ox, oy] = map(x, val);
        return (
          <circle
            key={idx}
            cx={ox}
            cy={oy}
            r={outlierRadius}
            fill={effectiveColor}
          />
        );
      })}
    </g>
  );
};

BoxPlot.displayName = 'BoxPlot';

BoxPlot.getRange = (props: BoxPlotProps): { xRange: Range; yRange: Range } => {
  const stats = computeBoxPlotStats(props.data);
  if (!stats) {
    return { xRange: { min: 0, max: 0 }, yRange: { min: 0, max: 0 } };
  }

  const xPos = props.x ?? 1;
  const halfW = (props.width ?? 0.8) / 2;

  return {
    xRange: { min: xPos - halfW, max: xPos + halfW },
    yRange: { min: stats.min, max: stats.max }
  };
};

BoxPlot.getLegendItems = (props: BoxPlotProps, fallbackColor: string) => {
  if (!props.label) return [];
  return [{ label: props.label, color: props.color || fallbackColor }];
};
