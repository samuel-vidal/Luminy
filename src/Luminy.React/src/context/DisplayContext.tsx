import { createContext, useContext } from 'react';
import { CoordinateMap, Range, ScaleType } from '../types';

export interface DisplayContextValue {
  map: CoordinateMap;
  xRange: Range;
  yRange: Range;
  xScaleType: ScaleType;
  yScaleType: ScaleType;
  getNextColor: () => string;
}

const DisplayContext = createContext<DisplayContextValue | null>(null);

export const DisplayProvider = DisplayContext.Provider;

export function useDisplayContext(): DisplayContextValue {
  const context = useContext(DisplayContext);
  if (!context) {
    throw new Error('Luminy plot components must be rendered inside a <Display> container.');
  }
  return context;
}
