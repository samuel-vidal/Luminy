import { BasePlotProps, PlotComponent, Range } from '../types';
import { useDisplayContext } from '../context/DisplayContext';

export interface BarChartProps extends BasePlotProps {
  heights: number[];
  x?: number[];
  barWidth?: number;
  className?: string;
}

export function calculateDefaultBarWidth(x: number[]): number {
  if (!x || x.length < 2) return 0.8;
  let sum = 0;
  for (let i = 1; i < x.length; i++) {
    sum += x[i] - x[i - 1];
  }
  return (sum / (x.length - 1)) * 0.8;
}

export const BarChart: PlotComponent<BarChartProps> = ({
  heights,
  x: explicitX,
  barWidth: explicitWidth,
  color,
  className
}) => {
  const { map, getNextColor } = useDisplayContext();
  const effectiveColor = color || getNextColor();

  if (!heights || heights.length === 0) return null;

  const x = explicitX || heights.map((_, i) => i);
  const barWidth = explicitWidth ?? calculateDefaultBarWidth(x);
  const count = Math.min(x.length, heights.length);

  return (
    <g className={`luminy-bar-chart ${className || ''}`.trim()}>
      {Array.from({ length: count }, (_, i) => {
        const xi = x[i];
        const hi = heights[i];
        if (!Number.isFinite(xi) || !Number.isFinite(hi)) return null;

        const [left, bottom] = map(xi - barWidth / 2, 0);
        const [right, top] = map(xi + barWidth / 2, hi);

        const rectX = Math.min(left, right);
        const rectY = Math.min(top, bottom);
        const rectW = Math.abs(right - left);
        const rectH = Math.abs(bottom - top);

        return (
          <rect
            key={i}
            x={rectX}
            y={rectY}
            width={rectW}
            height={rectH}
            fill={effectiveColor}
          />
        );
      })}
    </g>
  );
};

BarChart.displayName = 'BarChart';

BarChart.getRange = (props: BarChartProps): { xRange: Range; yRange: Range } => {
  if (!props.heights || props.heights.length === 0) {
    return { xRange: { min: 0, max: 0 }, yRange: { min: 0, max: 0 } };
  }

  const x = props.x || props.heights.map((_, i) => i);
  const barWidth = props.barWidth ?? calculateDefaultBarWidth(x);

  const xMin = Math.min(...x) - barWidth / 2;
  const xMax = Math.max(...x) + barWidth / 2;
  const yMin = Math.min(0, ...props.heights);
  const yMax = Math.max(0, ...props.heights);

  return {
    xRange: { min: xMin, max: xMax },
    yRange: { min: yMin, max: yMax }
  };
};

BarChart.getLegendItems = (props: BarChartProps, fallbackColor: string) => {
  if (!props.label) return [];
  return [{ label: props.label, color: props.color || fallbackColor }];
};
