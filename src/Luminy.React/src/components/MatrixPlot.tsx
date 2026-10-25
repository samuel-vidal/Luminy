import { BasePlotProps, PlotComponent, Range } from '../types';
import { useDisplayContext } from '../context/DisplayContext';
import { interpolateColor } from '../utils/palette';

export interface MatrixPlotProps extends BasePlotProps {
  data: number[][];
  colorMin?: string;
  colorMax?: string;
  cellStroke?: string;
  cellStrokeWidth?: number;
  className?: string;
}

export function getMatrixDimensions(data: number[][]): { rows: number; cols: number } {
  if (!data || data.length === 0) return { rows: 0, cols: 0 };
  const rows = data.length;
  let cols = 0;
  for (let i = 0; i < rows; i++) {
    if (data[i] && data[i].length > cols) {
      cols = data[i].length;
    }
  }
  return { rows, cols };
}

export function getMatrixValueExtremes(data: number[][]): { min: number; max: number } {
  let min = Infinity;
  let max = -Infinity;

  for (let i = 0; i < data.length; i++) {
    const row = data[i];
    if (!row) continue;
    for (let j = 0; j < row.length; j++) {
      const val = row[j];
      if (Number.isFinite(val)) {
        if (val < min) min = val;
        if (val > max) max = val;
      }
    }
  }

  if (!Number.isFinite(min) || !Number.isFinite(max)) {
    return { min: 0, max: 1 };
  }

  return { min, max };
}

export const MatrixPlot: PlotComponent<MatrixPlotProps> = ({
  data,
  colorMin = '#f0f9ff',
  colorMax = '#0284c7',
  cellStroke = 'rgba(255, 255, 255, 0.4)',
  cellStrokeWidth = 0.5,
  className
}) => {
  const { map } = useDisplayContext();

  if (!data || data.length === 0) return null;

  const { rows, cols } = getMatrixDimensions(data);
  if (rows === 0 || cols === 0) return null;

  const { min, max } = getMatrixValueExtremes(data);
  const span = Math.abs(max - min);

  return (
    <g className={`luminy-matrix-plot ${className || ''}`.trim()}>
      {data.map((row, i) => {
        if (!row) return null;
        return row.map((val, j) => {
          if (!Number.isFinite(val)) return null;

          const [left, top] = map(j + 0.5, i + 0.5);
          const [right, bottom] = map(j + 1.5, i + 1.5);

          const rectX = Math.min(left, right);
          const rectY = Math.min(top, bottom);
          const rectW = Math.abs(right - left);
          const rectH = Math.abs(bottom - top);

          const normalized = span < 1e-12 ? 0.5 : (val - min) / span;
          const fill = interpolateColor(colorMin, colorMax, normalized);

          return (
            <rect
              key={`${i}-${j}`}
              x={rectX}
              y={rectY}
              width={rectW}
              height={rectH}
              fill={fill}
              stroke={cellStroke}
              strokeWidth={cellStrokeWidth}
            />
          );
        });
      })}
    </g>
  );
};

MatrixPlot.displayName = 'MatrixPlot';

MatrixPlot.getRange = (props: MatrixPlotProps): { xRange: Range; yRange: Range } => {
  const { rows, cols } = getMatrixDimensions(props.data);
  if (rows === 0 || cols === 0) {
    return { xRange: { min: 0, max: 0 }, yRange: { min: 0, max: 0 } };
  }

  return {
    xRange: { min: 0.5, max: cols + 0.5 },
    yRange: { min: 0.5, max: rows + 0.5 }
  };
};

MatrixPlot.getLegendItems = (props: MatrixPlotProps, fallbackColor: string) => {
  if (!props.label) return [];
  return [{ label: props.label, color: props.colorMax || fallbackColor }];
};
