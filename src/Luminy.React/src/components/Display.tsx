import React, { useId, useMemo } from 'react';
import {
  AxesStyle,
  CoordinateMap,
  LegendItem,
  LegendPosition,
  PlotChildren,
  PlotComponent,
  Range,
  ScaleType
} from '../types';
import { DisplayProvider } from '../context/DisplayContext';
import { combineRanges } from '../utils/range';
import { calculateNiceTicks, formatTickLabel } from '../utils/ticks';
import { DEFAULT_PALETTE } from '../utils/palette';

export interface DisplayProps {
  width?: number;
  height?: number;
  padding?: number;
  inset?: number;
  title?: string;
  xLabel?: string;
  yLabel?: string;
  axesStyle?: AxesStyle;
  showGrid?: boolean;
  gridColor?: string;
  showLegend?: boolean;
  legendPosition?: LegendPosition;
  xScaleType?: ScaleType;
  yScaleType?: ScaleType;
  xRange?: [number, number];
  yRange?: [number, number];
  className?: string;
  style?: React.CSSProperties;
  children: PlotChildren;
}

export const Display: React.FC<DisplayProps> = ({
  width = 600,
  height = 400,
  padding = 40,
  inset = 20,
  title = '',
  xLabel = '',
  yLabel = '',
  axesStyle = 'frame',
  showGrid = false,
  gridColor = '#e5e5e5',
  showLegend = true,
  legendPosition = 'top-right',
  xScaleType = 'linear',
  yScaleType = 'linear',
  xRange: manualXRange,
  yRange: manualYRange,
  className,
  style,
  children
}) => {
  const clipId = useId();

  // 1. Synchronously collect ranges and legend items from valid children
  const { autoXRange, autoYRange, legendItems } = useMemo(() => {
    const xRanges: Range[] = [];
    const yRanges: Range[] = [];
    const items: LegendItem[] = [];
    let colorIdx = 0;

    React.Children.forEach(children, (child) => {
      if (!React.isValidElement(child)) return;

      const Component = child.type as PlotComponent<any>;
      const fallbackColor = DEFAULT_PALETTE[colorIdx++ % DEFAULT_PALETTE.length];

      if (Component.getRange) {
        const bounds = Component.getRange(child.props);
        xRanges.push(bounds.xRange);
        yRanges.push(bounds.yRange);
      }

      if (Component.getLegendItems) {
        items.push(...Component.getLegendItems(child.props, fallbackColor));
      }
    });

    return {
      autoXRange: combineRanges(xRanges),
      autoYRange: combineRanges(yRanges),
      legendItems: items
    };
  }, [children]);

  const xRange: Range = manualXRange ? { min: manualXRange[0], max: manualXRange[1] } : autoXRange;
  const yRange: Range = manualYRange ? { min: manualYRange[0], max: manualYRange[1] } : autoYRange;

  // 2. Coordinate mapping function
  const coordinateMap: CoordinateMap = useMemo(() => {
    return (u: number, v: number): [number, number] => {
      const scaledU =
        xScaleType === 'logarithmic'
          ? (Math.log(u) - Math.log(xRange.min)) / (Math.log(xRange.max) - Math.log(xRange.min))
          : (u - xRange.min) / (xRange.max - xRange.min);

      const scaledV =
        yScaleType === 'logarithmic'
          ? (Math.log(v) - Math.log(yRange.min)) / (Math.log(yRange.max) - Math.log(yRange.min))
          : (v - yRange.min) / (yRange.max - yRange.min);

      const x = padding + inset + scaledU * (width - 2 * (padding + inset));
      const y = height - padding - inset - scaledV * (height - 2 * (padding + inset));
      return [x, y];
    };
  }, [width, height, padding, inset, xRange.min, xRange.max, yRange.min, yRange.max, xScaleType, yScaleType]);

  // 3. Ticks
  const xTicks = useMemo(() => calculateNiceTicks(xRange), [xRange]);
  const yTicks = useMemo(() => calculateNiceTicks(yRange), [yRange]);

  // Context value with color palette generator
  let runtimeColorIdx = 0;
  const contextValue = useMemo(() => ({
    map: coordinateMap,
    xRange,
    yRange,
    xScaleType,
    yScaleType,
    getNextColor: () => DEFAULT_PALETTE[runtimeColorIdx++ % DEFAULT_PALETTE.length]
  }), [coordinateMap, xRange, yRange, xScaleType, yScaleType]);

  // Plot bounds in screen pixels
  const [plotLeft, plotTop] = coordinateMap(xRange.min, yRange.max);
  const [plotRight, plotBottom] = coordinateMap(xRange.max, yRange.min);

  // 4. Legend positioning calculations
  const legendBox = useMemo(() => {
    if (!showLegend || legendItems.length === 0) return null;

    const rowHeight = 20;
    const boxWidth = 15;
    const legPadding = 10;
    const maxLen = Math.max(...legendItems.map((item) => item.label.length), 5);
    const legendWidth = Math.max(100, maxLen * 7 + boxWidth + legPadding * 3);
    const legendHeight = legendItems.length * rowHeight + legPadding * 2;

    let lx: number;
    let ly: number;

    const frameLeft = plotLeft - inset;
    const frameTop = plotTop - inset;
    const frameRight = plotRight + inset;
    const frameBottom = plotBottom + inset;

    switch (legendPosition) {
      case 'top-left':
        lx = frameLeft + legPadding;
        ly = frameTop + legPadding;
        break;
      case 'bottom-right':
        lx = frameRight - legendWidth - legPadding;
        ly = frameBottom - legendHeight - legPadding;
        break;
      case 'bottom-left':
        lx = frameLeft + legPadding;
        ly = frameBottom - legendHeight - legPadding;
        break;
      case 'top-right':
      default:
        lx = frameRight - legendWidth - legPadding;
        ly = frameTop + legPadding;
        break;
    }

    return { lx, ly, legendWidth, legendHeight, rowHeight, boxWidth, legPadding };
  }, [showLegend, legendItems, legendPosition, plotLeft, plotTop, plotRight, plotBottom, inset]);

  return (
    <DisplayProvider value={contextValue}>
      <svg
        width={width}
        height={height}
        viewBox={`0 0 ${width} ${height}`}
        className={`luminy-display ${className || ''}`.trim()}
        style={{ display: 'block', maxWidth: '100%', height: 'auto', ...style }}
        xmlns="http://www.w3.org/2000/svg"
      >
        <defs>
          <clipPath id={clipId}>
            <rect
              x={padding}
              y={padding}
              width={Math.max(0, width - 2 * padding)}
              height={Math.max(0, height - 2 * padding)}
            />
          </clipPath>
        </defs>

        {/* Grid */}
        {showGrid && axesStyle !== 'none' && (
          <g className="luminy-grid" stroke={gridColor} strokeWidth={1} strokeDasharray="3 3">
            {xTicks.map((tick, i) => {
              const [x] = coordinateMap(tick, yRange.min);
              return <line key={`gx-${i}`} x1={x} y1={plotTop - inset} x2={x} y2={plotBottom + inset} />;
            })}
            {yTicks.map((tick, i) => {
              const [, y] = coordinateMap(xRange.min, tick);
              return <line key={`gy-${i}`} x1={plotLeft - inset} y1={y} x2={plotRight + inset} y2={y} />;
            })}
          </g>
        )}

        {/* Plots (clipped) */}
        <g clipPath={`url(#${clipId})`} className="luminy-plots">
          {children}
        </g>

        {/* Axes & Ticks */}
        {axesStyle === 'frame' && (
          <g className="luminy-axes" stroke="#000000" strokeWidth={1}>
            {/* Frame rectangle */}
            <rect
              x={plotLeft - inset}
              y={plotTop - inset}
              width={plotRight - plotLeft + 2 * inset}
              height={plotBottom - plotTop + 2 * inset}
              fill="none"
            />
            {/* X Ticks & Labels */}
            {xTicks.map((tick, i) => {
              const [tx] = coordinateMap(tick, yRange.min);
              const ty = plotBottom + inset;
              return (
                <g key={`tx-${i}`} stroke="none" fill="#000000">
                  <line x1={tx} y1={ty} x2={tx} y2={ty - 5} stroke="#000000" />
                  <text
                    x={tx}
                    y={ty + 15}
                    fontSize={10}
                    fontFamily="Arial, sans-serif"
                    textAnchor="middle"
                  >
                    {formatTickLabel(tick, xScaleType)}
                  </text>
                </g>
              );
            })}
            {/* Y Ticks & Labels */}
            {yTicks.map((tick, i) => {
              const [, ty] = coordinateMap(xRange.min, tick);
              const tx = plotLeft - inset;
              return (
                <g key={`ty-${i}`} stroke="none" fill="#000000">
                  <line x1={tx} y1={ty} x2={tx + 5} y2={ty} stroke="#000000" />
                  <text
                    x={tx - 6}
                    y={ty + 3}
                    fontSize={10}
                    fontFamily="Arial, sans-serif"
                    textAnchor="end"
                  >
                    {formatTickLabel(tick, yScaleType)}
                  </text>
                </g>
              );
            })}
          </g>
        )}

        {/* Title */}
        {title && (
          <text
            x={width / 2}
            y={padding / 2 + 5}
            textAnchor="middle"
            fontSize={16}
            fontWeight="bold"
            fontFamily="Arial, sans-serif"
            fill="#2c3e50"
          >
            {title}
          </text>
        )}

        {/* X Axis Label */}
        {xLabel && (
          <text
            x={width / 2}
            y={height - 8}
            textAnchor="middle"
            fontSize={13}
            fontFamily="Arial, sans-serif"
            fill="#34495e"
          >
            {xLabel}
          </text>
        )}

        {/* Y Axis Label */}
        {yLabel && (
          <text
            x={15}
            y={height / 2}
            textAnchor="middle"
            fontSize={13}
            fontFamily="Arial, sans-serif"
            fill="#34495e"
            transform={`rotate(-90, 15, ${height / 2})`}
          >
            {yLabel}
          </text>
        )}

        {/* Legend */}
        {legendBox && (
          <g className="luminy-legend">
            <rect
              x={legendBox.lx}
              y={legendBox.ly}
              width={legendBox.legendWidth}
              height={legendBox.legendHeight}
              fill="#ffffff"
              stroke="#000000"
              strokeWidth={0.5}
            />
            {legendItems.map((item, idx) => {
              const iy = legendBox.ly + legendBox.legPadding + idx * legendBox.rowHeight + legendBox.rowHeight / 2;
              return (
                <g key={`leg-${idx}`}>
                  <rect
                    x={legendBox.lx + legendBox.legPadding}
                    y={iy - legendBox.boxWidth / 4}
                    width={legendBox.boxWidth}
                    height={legendBox.boxWidth / 2}
                    fill={item.color}
                  />
                  <text
                    x={legendBox.lx + legendBox.legPadding + legendBox.boxWidth + 6}
                    y={iy + 4}
                    fontSize={10}
                    fontFamily="Arial, sans-serif"
                    fill="#000000"
                    textAnchor="start"
                  >
                    {item.label}
                  </text>
                </g>
              );
            })}
          </g>
        )}
      </svg>
    </DisplayProvider>
  );
};
