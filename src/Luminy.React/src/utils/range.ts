import { Range } from '../types';

export function getArrayRange(values: number[]): Range {
  let min = Infinity;
  let max = -Infinity;

  for (let i = 0; i < values.length; i++) {
    const v = values[i];
    if (Number.isFinite(v)) {
      if (v < min) min = v;
      if (v > max) max = v;
    }
  }

  if (min === Infinity || max === -Infinity) {
    return { min: 0, max: 0 };
  }

  return { min, max };
}

export function combineRanges(ranges: Range[]): Range {
  if (ranges.length === 0) return { min: 0, max: 1 };

  let min = ranges[0].min;
  let max = ranges[0].max;

  for (let i = 1; i < ranges.length; i++) {
    if (ranges[i].min < min) min = ranges[i].min;
    if (ranges[i].max > max) max = ranges[i].max;
  }

  if (min === max) {
    return { min: min - 1, max: max + 1 };
  }

  return { min, max };
}
