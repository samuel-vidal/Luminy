import { Range, ScaleType } from '../types';

export function calculateNiceTicks(range: Range): number[] {
  const span = range.max - range.min;
  if (span <= 0) return [range.min];

  const exponent = Math.floor(Math.log10(span));
  const fraction = span / Math.pow(10, exponent);

  let niceFraction: number;
  if (fraction < 1.5) niceFraction = 1;
  else if (fraction < 3) niceFraction = 2;
  else if (fraction < 7) niceFraction = 5;
  else niceFraction = 10;

  const tickSpacing = niceFraction * Math.pow(10, exponent - 1);
  const start = Math.ceil(range.min / tickSpacing) * tickSpacing;
  const end = Math.floor(range.max / tickSpacing) * tickSpacing;

  const ticks: number[] = [];
  for (let value = start; value <= end + tickSpacing * 0.5; value += tickSpacing) {
    let val = Math.round(value / tickSpacing) * tickSpacing;
    if (Object.is(val, -0) || Math.abs(val) < 1e-12) val = 0;
    ticks.push(val);
  }

  return ticks;
}

export function formatTickLabel(value: number, scaleType: ScaleType = 'linear'): string {
  if (scaleType === 'logarithmic') {
    const log = Math.log10(value);
    if (Math.abs(log - Math.round(log)) < 1e-5) {
      return `10^${Math.round(log)}`;
    }
    return '';
  }

  if (value === 0) return '0';
  if (Math.abs(value) >= 1e4 || (Math.abs(value) <= 1e-3 && Math.abs(value) > 0)) {
    return value.toExponential(1);
  }

  return Number(value.toPrecision(4)).toString();
}
