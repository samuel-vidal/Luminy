import React, { useState, useMemo } from 'react';
import { Display, HistogramPlot, AxesStyle } from 'luminy-react';

export const HistogramPlotDemo: React.FC = () => {
  const [sampleCount, setSampleCount] = useState<number>(600);
  const [binCount, setBinCount] = useState<number>(20);
  const [distType, setDistType] = useState<'normal' | 'bimodal' | 'skewed'>('bimodal');
  const [dispersion, setDispersion] = useState<number>(1.2);
  const [showGrid, setShowGrid] = useState<boolean>(true);
  const [showLegend, setShowLegend] = useState<boolean>(true);
  const [axesStyle, setAxesStyle] = useState<AxesStyle>('frame');

  const data = useMemo(() => {
    const values: number[] = [];

    for (let i = 0; i < sampleCount; i++) {
      // Deterministic Box-Muller pseudo-random values
      const u1 = ((Math.sin(i * 12.9898 + 7) * 43758.5453) % 1 + 1) % 1;
      const u2 = ((Math.sin(i * 78.233 + 13) * 43758.5453) % 1 + 1) % 1;
      const r = Math.sqrt(-2.0 * Math.log(Math.max(1e-6, u1)));
      const theta = 2.0 * Math.PI * u2;
      const z0 = r * Math.cos(theta);

      if (distType === 'normal') {
        values.push(10.0 + z0 * dispersion * 2.0);
      } else if (distType === 'bimodal') {
        const isClusterA = i % 2 === 0;
        const center = isClusterA ? 6.0 : 14.0;
        values.push(center + z0 * dispersion * 1.4);
      } else {
        // Skewed / log-normal
        const expVal = Math.exp(z0 * 0.5 * dispersion);
        values.push(expVal * 3.0);
      }
    }

    return values;
  }, [sampleCount, distType, dispersion]);

  return (
    <div className="showcase-grid">
      <aside className="control-panel">
        <h3 className="panel-title">Histogram Controls</h3>

        <div className="control-group">
          <label>Distribution Profile</label>
          <select value={distType} onChange={(e) => setDistType(e.target.value as any)}>
            <option value="bimodal">Bimodal (Dual Gaussian Mix)</option>
            <option value="normal">Normal (Standard Bell Curve)</option>
            <option value="skewed">Log-Normal (Right-Skewed)</option>
          </select>
        </div>

        <div className="control-group">
          <label>
            <span>Sample Points (N)</span>
            <span>{sampleCount}</span>
          </label>
          <input
            type="range"
            min="100"
            max="1500"
            step="50"
            value={sampleCount}
            onChange={(e) => setSampleCount(parseInt(e.target.value, 10))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Bin Count (k)</span>
            <span>{binCount}</span>
          </label>
          <input
            type="range"
            min="6"
            max="45"
            step="1"
            value={binCount}
            onChange={(e) => setBinCount(parseInt(e.target.value, 10))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Dispersion (σ)</span>
            <span>{dispersion.toFixed(2)}</span>
          </label>
          <input
            type="range"
            min="0.4"
            max="2.5"
            step="0.1"
            value={dispersion}
            onChange={(e) => setDispersion(parseFloat(e.target.value))}
          />
        </div>

        <div className="control-group">
          <label>Axes Style</label>
          <select value={axesStyle} onChange={(e) => setAxesStyle(e.target.value as AxesStyle)}>
            <option value="frame">Frame (Scientific)</option>
            <option value="normal">Normal (Center 0,0)</option>
            <option value="none">None (Clean)</option>
          </select>
        </div>

        <div className="control-group">
          <label className="checkbox-label">
            <input
              type="checkbox"
              checked={showGrid}
              onChange={(e) => setShowGrid(e.target.checked)}
            />
            Show Grid
          </label>
          <label className="checkbox-label">
            <input
              type="checkbox"
              checked={showLegend}
              onChange={(e) => setShowLegend(e.target.checked)}
            />
            Show Legend
          </label>
        </div>
      </aside>

      <section className="chart-card">
        <Display
          width={720}
          height={460}
          title={`Statistical Histogram Distribution (N = ${sampleCount})`}
          xLabel="Measured Value (x)"
          yLabel="Frequency Count"
          showGrid={showGrid}
          showLegend={showLegend}
          axesStyle={axesStyle}
        >
          <HistogramPlot
            data={data}
            binCount={binCount}
            color="#8b5cf6"
            stroke="rgba(255, 255, 255, 0.4)"
            strokeWidth={1}
            label="Sample Frequency"
          />
        </Display>

        <div className="stats-bar">
          <span>Samples: <strong>{sampleCount}</strong></span>
          <span>Bins: <strong>{binCount} intervals</strong></span>
          <span>Profile: <strong>{distType.toUpperCase()}</strong></span>
        </div>
      </section>
    </div>
  );
};
