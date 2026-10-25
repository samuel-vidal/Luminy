import type React from 'react';

export interface Range {
  min: number;
  max: number;
}

export type CoordinateMap = (u: number, v: number) => [number, number];

export type AxesStyle = 'normal' | 'frame' | 'boxed' | 'none';
export type ScaleType = 'linear' | 'logarithmic';
export type LegendPosition = 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left';

export interface BasePlotProps {
  label?: string;
  color?: string;
}

export interface LegendItem {
  label: string;
  color: string;
}

export interface PlotComponent<P extends BasePlotProps = BasePlotProps> {
  (props: P): React.ReactElement | null;
  displayName?: string;
  /** Pure static descriptor computing [xRange, yRange] from props without DOM mounting */
  getRange?: (props: P) => { xRange: Range; yRange: Range };
  /** Optional static method returning legend entries */
  getLegendItems?: (props: P, fallbackColor: string) => LegendItem[];
}

export type PlotElement<P extends BasePlotProps = BasePlotProps> = React.ReactElement<P, PlotComponent<P>>;

export type PlotChildren =
  | PlotElement<any>
  | null
  | false
  | undefined
  | (PlotElement<any> | null | false | undefined)[];
