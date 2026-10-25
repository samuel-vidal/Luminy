import React, { useState, useMemo } from 'react';
import { Display, MatrixPlot, AxesStyle } from 'luminy-react';

interface PaletteOption {
  name: string;
  min: string;
  max: string;
}

const PALETTES: Record<string, PaletteOption> = {
  ocean: { name: 'Ocean Cyan', min: '#f0f9ff', max: '#0284c7' },
  thermal: { name: 'Thermal Flame', min: '#fffbeb', max: '#dc2626' },
  emerald: { name: 'Emerald Forest', min: '#ecfdf5', max: '#047857' },
  sunset: { name: 'Sunset Violet', min: '#fdf4ff', max: '#7c3aed' }
};

export const MatrixPlotDemo: React.FC = () => {
  const [gridSize, setGridSize] = useState<number>(14);
  const [frequency, setFrequency] = useState<number>(2.5);
  const [pattern, setPattern] = useState<'ripple' | 'hotspot' | 'gradient'>('ripple');
  const [paletteKey, setPaletteKey] = useState<string>('ocean');
  const [axesStyle, setAxesStyle] = useState<AxesStyle>('frame');

  const activePalette = PALETTES[paletteKey] || PALETTES.ocean;

  const matrixData = useMemo(() => {
    const grid: number[][] = [];
    const center = (gridSize - 1) / 2;

    for (let i = 0; i < gridSize; i++) {
      const row: number[] = [];
      for (let j = 0; j < gridSize; j++) {
        const di = (i - center) / center;
        const dj = (j - center) / center;
        const dist = Math.sqrt(di * di + dj * dj);

        let val = 0;
        if (pattern === 'ripple') {
          val = Math.cos(dist * frequency * Math.PI) * Math.exp(-dist * 0.8);
        } else if (pattern === 'hotspot') {
          const s1 = Math.exp(-((di - 0.4) ** 2 + (dj - 0.4) ** 2) * 6);
          const s2 = Math.exp(-((di + 0.5) ** 2 + (dj + 0.3) ** 2) * 5) * 0.8;
          val = s1 + s2;
        } else {
          // Linear gradient with harmonics
          val = (di + dj) * 0.5 + Math.sin(di * frequency * 2) * 0.2;
        }

        row.push(val);
      }
      grid.push(row);
    }

    return grid;
  }, [gridSize, frequency, pattern]);

  return (
    <div className="showcase-grid">
      <aside className="control-panel">
        <h3 className="panel-title">MatrixPlot Controls</h3>

        <div className="control-group">
          <label>Wave Pattern</label>
          <select value={pattern} onChange={(e) => setPattern(e.target.value as any)}>
            <option value="ripple">Radial Bessel Ripple</option>
            <option value="hotspot">Thermal Hotspots</option>
            <option value="gradient">Diagonal Gradient</option>
          </select>
        </div>

        <div className="control-group">
          <label>Color Theme</label>
          <select value={paletteKey} onChange={(e) => setPaletteKey(e.target.value)}>
            <option value="ocean">Ocean Cyan</option>
            <option value="thermal">Thermal Flame</option>
            <option value="emerald">Emerald Forest</option>
            <option value="sunset">Sunset Violet</option>
          </select>
        </div>

        <div className="control-group">
          <label>
            <span>Grid Resolution (N × N)</span>
            <span>{gridSize} × {gridSize} ({gridSize * gridSize} cells)</span>
          </label>
          <input
            type="range"
            min="6"
            max="24"
            step="1"
            value={gridSize}
            onChange={(e) => setGridSize(parseInt(e.target.value, 10))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Wave Spatial Frequency</span>
            <span>{frequency.toFixed(1)} rad</span>
          </label>
          <input
            type="range"
            min="1.0"
            max="6.0"
            step="0.2"
            value={frequency}
            onChange={(e) => setFrequency(parseFloat(e.target.value))}
          />
        </div>

        <div className="control-group">
          <label>Axes Style</label>
          <select value={axesStyle} onChange={(e) => setAxesStyle(e.target.value as AxesStyle)}>
            <option value="frame">Frame (Scientific)</option>
            <option value="boxed">Boxed</option>
            <option value="none">None (Clean)</option>
          </select>
        </div>
      </aside>

      <section className="chart-card">
        <Display
          width={720}
          height={460}
          title={`2D Heatmap & Matrix Intensity (${gridSize}×${gridSize})`}
          xLabel="Matrix Column Index (j)"
          yLabel="Matrix Row Index (i)"
          showGrid={false}
          showLegend={false}
          axesStyle={axesStyle}
        >
          <MatrixPlot
            data={matrixData}
            colorMin={activePalette.min}
            colorMax={activePalette.max}
            cellStroke="rgba(255, 255, 255, 0.25)"
            cellStrokeWidth={0.5}
            label="Intensity"
          />
        </Display>

        <div className="stats-bar">
          <span>Dimensions: <strong>{gridSize} × {gridSize}</strong></span>
          <span>Total Cells: <strong>{gridSize * gridSize}</strong></span>
          <span>Theme: <strong>{activePalette.name}</strong></span>
        </div>
      </section>
    </div>
  );
};
