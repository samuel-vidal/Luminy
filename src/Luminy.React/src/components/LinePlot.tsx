import { BasePlotProps, PlotComponent } from '../types';
import { useDisplayContext } from '../context/DisplayContext';
import { getArrayRange } from '../utils/range';

export interface LinePlotProps extends BasePlotProps {
  x: number[];
  y: number[];
  strokeWidth?: number;
  strokeDasharray?: string;
  className?: string;
}

export const LinePlot: PlotComponent<LinePlotProps> = ({
  x,
  y,
  color,
  strokeWidth = 2,
  strokeDasharray,
  className
}) => {
  const { map, getNextColor } = useDisplayContext();
  const effectiveColor = color || getNextColor();

  if (!x || !y || x.length < 2) return null;

  const count = Math.min(x.length, y.length);
  const segments: [number, number][][] = [];
  let currentSegment: [number, number][] = [];

  for (let i = 0; i < count; i++) {
    const xi = x[i];
    const yi = y[i];

    if (!Number.isFinite(xi) || !Number.isFinite(yi)) {
      if (currentSegment.length > 0) {
        segments.push(currentSegment);
        currentSegment = [];
      }
      continue;
    }

    const [px, py] = map(xi, yi);
    currentSegment.push([px, py]);
  }

  if (currentSegment.length > 0) {
    segments.push(currentSegment);
  }

  return (
    <g className={`luminy-line-plot ${className || ''}`.trim()}>
      {segments.map((segment, segIdx) => {
        if (segment.length < 2) return null;

        let d = `M ${segment[0][0].toFixed(2)} ${segment[0][1].toFixed(2)}`;
        for (let j = 1; j < segment.length; j++) {
          d += ` L ${segment[j][0].toFixed(2)} ${segment[j][1].toFixed(2)}`;
        }

        return (
          <path
            key={segIdx}
            d={d}
            stroke={effectiveColor}
            strokeWidth={strokeWidth}
            strokeDasharray={strokeDasharray}
            fill="none"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        );
      })}
    </g>
  );
};

LinePlot.displayName = 'LinePlot';

LinePlot.getRange = (props: LinePlotProps) => ({
  xRange: getArrayRange(props.x),
  yRange: getArrayRange(props.y)
});

LinePlot.getLegendItems = (props: LinePlotProps, fallbackColor: string) => {
  if (!props.label) return [];
  return [{ label: props.label, color: props.color || fallbackColor }];
};
