import { BasePlotProps, PlotComponent } from '../types';
import { useDisplayContext } from '../context/DisplayContext';
import { DEFAULT_PALETTE } from '../utils/palette';

export interface DonutPlotProps extends BasePlotProps {
  values: number[];
  labels?: string[];
  externalRadius?: number;
  internalRadius?: number;
  colors?: string[];
  showSliceLabels?: boolean;
  insideValueDisplay?: 'none' | 'raw' | 'percent';
  minInsideValueThreshold?: number;
  className?: string;
}

export const DonutPlot: PlotComponent<DonutPlotProps> = ({
  values,
  labels = [],
  externalRadius = 1.0,
  internalRadius = 0.6,
  colors,
  showSliceLabels = true,
  insideValueDisplay = 'none',
  minInsideValueThreshold = 0.05,
  className
}) => {
  const { map } = useDisplayContext();

  if (!values || values.length === 0) return null;

  // Filter positive finite values
  const validIndices: number[] = [];
  let total = 0;
  for (let i = 0; i < values.length; i++) {
    const val = values[i];
    if (Number.isFinite(val) && val > 0) {
      validIndices.push(i);
      total += val;
    }
  }

  if (total <= 0 || validIndices.length === 0) return null;

  const [cx, cy] = map(0, 0);
  const [rx] = map(externalRadius, 0);
  const [, ry] = map(0, externalRadius);
  const rExt = Math.min(Math.abs(rx - cx), Math.abs(ry - cy));
  const rInt = externalRadius > 0
    ? rExt * (Math.max(0, Math.min(internalRadius, externalRadius)) / externalRadius)
    : 0;

  const sliceColors = values.map((_, i) =>
    colors && i < colors.length ? colors[i] : DEFAULT_PALETTE[i % DEFAULT_PALETTE.length]
  );

  let currentAngle = -Math.PI / 2;

  return (
    <g className={`luminy-donut-plot ${className || ''}`.trim()}>
      {validIndices.map((index) => {
        const val = values[index];
        const ratio = val / total;
        const sweepAngle = ratio * Math.PI * 2;
        const startAngle = currentAngle;
        const endAngle = currentAngle + sweepAngle;
        currentAngle = endAngle;

        const sliceColor = sliceColors[index];
        const midAngle = startAngle + sweepAngle / 2;

        let slicePath: string;
        const isSingleSlice = ratio >= 0.9999;

        if (isSingleSlice) {
          if (rInt <= 0.001) {
            slicePath = `M ${cx - rExt} ${cy} A ${rExt} ${rExt} 0 1 0 ${cx + rExt} ${cy} A ${rExt} ${rExt} 0 1 0 ${cx - rExt} ${cy} Z`;
          } else {
            slicePath = `M ${cx - rExt} ${cy} A ${rExt} ${rExt} 0 1 0 ${cx + rExt} ${cy} A ${rExt} ${rExt} 0 1 0 ${cx - rExt} ${cy} ` +
              `M ${cx - rInt} ${cy} A ${rInt} ${rInt} 0 1 1 ${cx + rInt} ${cy} A ${rInt} ${rInt} 0 1 1 ${cx - rInt} ${cy} Z`;
          }
        } else {
          const x1Ext = cx + rExt * Math.cos(startAngle);
          const y1Ext = cy + rExt * Math.sin(startAngle);
          const x2Ext = cx + rExt * Math.cos(endAngle);
          const y2Ext = cy + rExt * Math.sin(endAngle);
          const largeArc = sweepAngle > Math.PI ? 1 : 0;

          if (rInt <= 0.001) {
            slicePath = `M ${x1Ext.toFixed(2)} ${y1Ext.toFixed(2)} A ${rExt.toFixed(2)} ${rExt.toFixed(2)} 0 ${largeArc} 1 ${x2Ext.toFixed(2)} ${y2Ext.toFixed(2)} L ${cx.toFixed(2)} ${cy.toFixed(2)} Z`;
          } else {
            const x1Int = cx + rInt * Math.cos(startAngle);
            const y1Int = cy + rInt * Math.sin(startAngle);
            const x2Int = cx + rInt * Math.cos(endAngle);
            const y2Int = cy + rInt * Math.sin(endAngle);

            slicePath = `M ${x1Ext.toFixed(2)} ${y1Ext.toFixed(2)} A ${rExt.toFixed(2)} ${rExt.toFixed(2)} 0 ${largeArc} 1 ${x2Ext.toFixed(2)} ${y2Ext.toFixed(2)} ` +
              `L ${x2Int.toFixed(2)} ${y2Int.toFixed(2)} A ${rInt.toFixed(2)} ${rInt.toFixed(2)} 0 ${largeArc} 0 ${x1Int.toFixed(2)} ${y1Int.toFixed(2)} Z`;
          }
        }

        // Inside value calculation
        const rMid = (rInt + rExt) / 2;
        const ix = cx + rMid * Math.cos(midAngle);
        const iy = cy + rMid * Math.sin(midAngle);
        const showInside = insideValueDisplay !== 'none' && ratio >= minInsideValueThreshold;
        const insideText = insideValueDisplay === 'percent'
          ? `${(ratio * 100).toFixed(ratio < 0.1 ? 1 : 0)}%`
          : Number(val.toPrecision(3)).toString();

        // Outside label calculation
        const label = labels[index];
        const rLabel = rExt * 1.10;
        const lx = cx + rLabel * Math.cos(midAngle);
        const ly = cy + rLabel * Math.sin(midAngle);
        const cosMid = Math.cos(midAngle);
        const sinMid = Math.sin(midAngle);

        const textAnchor: 'start' | 'end' | 'middle' =
          cosMid > 0.3 ? 'start' : cosMid < -0.3 ? 'end' : 'middle';
        const yOffset = sinMid < -0.3 ? -2 : sinMid > 0.3 ? 10 : 4;

        return (
          <g key={`slice-${index}`}>
            {/* Slice shape */}
            <path
              d={slicePath}
              fill={sliceColor}
              stroke="#ffffff"
              strokeWidth={1.5}
            />

            {/* Inside badge */}
            {showInside && (
              <text
                x={ix}
                y={iy + 4}
                textAnchor="middle"
                fontSize={11}
                fontWeight="bold"
                fontFamily="Arial, sans-serif"
                fill="#ffffff"
              >
                {insideText}
              </text>
            )}

            {/* Outside label */}
            {showSliceLabels && label && (
              <text
                x={lx}
                y={ly + yOffset}
                textAnchor={textAnchor}
                fontSize={11}
                fontFamily="Arial, sans-serif"
                fill="#1e293b"
              >
                {label}
              </text>
            )}
          </g>
        );
      })}
    </g>
  );
};

DonutPlot.displayName = 'DonutPlot';

DonutPlot.getRange = (props: DonutPlotProps) => {
  const margin = Math.max((props.externalRadius ?? 1.0) * 1.35, 0.1);
  return {
    xRange: { min: -margin, max: margin },
    yRange: { min: -margin, max: margin }
  };
};

DonutPlot.getLegendItems = (props: DonutPlotProps) => {
  if (!props.labels || !props.values) return [];
  const count = Math.min(props.values.length, props.labels.length);
  const items = [];
  for (let i = 0; i < count; i++) {
    const lbl = props.labels[i];
    if (lbl) {
      const clr = props.colors && i < props.colors.length
        ? props.colors[i]
        : DEFAULT_PALETTE[i % DEFAULT_PALETTE.length];
      items.push({ label: lbl, color: clr });
    }
  }
  return items;
};
