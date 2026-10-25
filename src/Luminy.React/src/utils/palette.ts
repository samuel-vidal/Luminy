export const DEFAULT_PALETTE = [
  '#1f77b4', // Blue
  '#ff7f0e', // Orange
  '#2ca02c', // Green
  '#d62728', // Red
  '#9467bd', // Purple
  '#8c564b', // Brown
  '#e377c2', // Pink
  '#7f7f7f', // Gray
  '#bcbd22', // Yellow-Green
  '#17becf', // Cyan
];

/**
 * Parses a hex (#fff, #ffffff) or rgb(r, g, b) string into [r, g, b] channels.
 */
export function parseColor(color: string): [number, number, number] {
  if (!color) return [0, 0, 0];
  const trimmed = color.trim().toLowerCase();

  if (trimmed.startsWith('#')) {
    let hex = trimmed.slice(1);
    if (hex.length === 3) {
      hex = hex.split('').map((c) => c + c).join('');
    }
    const num = parseInt(hex, 16);
    if (!Number.isNaN(num) && hex.length === 6) {
      return [(num >> 16) & 255, (num >> 8) & 255, num & 255];
    }
  } else if (trimmed.startsWith('rgb')) {
    const match = trimmed.match(/\d+/g);
    if (match && match.length >= 3) {
      return [parseInt(match[0], 10), parseInt(match[1], 10), parseInt(match[2], 10)];
    }
  }

  return [0, 0, 0];
}

/**
 * Linearly interpolates between two colors by factor t (0.0 to 1.0).
 */
export function interpolateColor(colorA: string, colorB: string, t: number): string {
  const [r1, g1, b1] = parseColor(colorA);
  const [r2, g2, b2] = parseColor(colorB);
  const clampedT = Math.max(0, Math.min(1, t));
  const r = Math.round(r1 + (r2 - r1) * clampedT);
  const g = Math.round(g1 + (g2 - g1) * clampedT);
  const b = Math.round(b1 + (b2 - b1) * clampedT);
  return `rgb(${r}, ${g}, ${b})`;
}
