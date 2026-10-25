import { BasePlotProps, PlotComponent } from '../types';
import { useDisplayContext } from '../context/DisplayContext';
import { getArrayRange } from '../utils/range';

export interface PointPlotProps extends BasePlotProps {
  x: number[];
  y: number[];
  pointSize?: number;
  className?: string;
}

export const PointPlot: PlotComponent<PointPlotProps> = ({
  x,
  y,
  color,
  pointSize = 3.5,
  className
}) => {
  const { map, getNextColor } = useDisplayContext();
  const effectiveColor = color || getNextColor();

  if (!x || !y || x.length === 0) return null;

  const count = Math.min(x.length, y.length);
  const validPoints: [number, number][] = [];

  for (let i = 0; i < count; i++) {
    const xi = x[i];
    const yi = y[i];
    if (Number.isFinite(xi) && Number.isFinite(yi)) {
      const [px, py] = map(xi, yi);
      validPoints.push([px, py]);
    }
  }

  return (
    <g className={`luminy-point-plot ${className || ''}`.trim()}>
      {validPoints.map(([px, py], idx) => (
        <circle
          key={idx}
          cx={px}
          cy={py}
          r={pointSize}
          fill={effectiveColor}
        />
      ))}
    </g>
  );
};

PointPlot.displayName = 'PointPlot';

PointPlot.getRange = (props: PointPlotProps) => ({
  xRange: getArrayRange(props.x),
  yRange: getArrayRange(props.y)
});

PointPlot.getLegendItems = (props: PointPlotProps, fallbackColor: string) => {
  if (!props.label) return [];
  return [{ label: props.label, color: props.color || fallbackColor }];
};
