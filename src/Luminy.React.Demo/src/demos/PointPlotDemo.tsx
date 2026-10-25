import React, { useState, useMemo } from 'react';
import { Display, PointPlot, AxesStyle } from 'luminy-react';

export const PointPlotDemo: React.FC = () => {
  const [pointCount, setPointCount] = useState<number>(60);
  const [pointSize, setPointSize] = useState<number>(4);
  const [spread, setSpread] = useState<number>(1.2);
  const [separation, setSeparation] = useState<number>(4.0);
  const [showClusterB, setShowClusterB] = useState<boolean>(true);
  const [showGrid, setShowGrid] = useState<boolean>(true);
  const [showLegend, setShowLegend] = useState<boolean>(true);
  const [axesStyle, setAxesStyle] = useState<AxesStyle>('frame');

  const { clusterA, clusterB } = useMemo(() => {
    const xA: number[] = [];
    const yA: number[] = [];
    const xB: number[] = [];
    const yB: number[] = [];

    // Deterministic pseudo-random generation for stable reactivity
    for (let i = 0; i < pointCount; i++) {
      const u1 = ((Math.sin(i * 12.9898 + 1) * 43758.5453) % 1 + 1) % 1;
      const u2 = ((Math.sin(i * 78.233 + 2) * 43758.5453) % 1 + 1) % 1;
      // Box-Muller transform for normal distribution
      const r = Math.sqrt(-2.0 * Math.log(Math.max(1e-6, u1)));
      const theta = 2.0 * Math.PI * u2;
      const z0 = r * Math.cos(theta);
      const z1 = r * Math.sin(theta);

      xA.push(2.5 + z0 * spread);
      yA.push(2.5 + z1 * spread);

      const u3 = ((Math.sin((i + 500) * 12.9898 + 3) * 43758.5453) % 1 + 1) % 1;
      const u4 = ((Math.sin((i + 500) * 78.233 + 4) * 43758.5453) % 1 + 1) % 1;
      const rB = Math.sqrt(-2.0 * Math.log(Math.max(1e-6, u3)));
      const thetaB = 2.0 * Math.PI * u4;
      const z0B = rB * Math.cos(thetaB);
      const z1B = rB * Math.sin(thetaB);

      xB.push(2.5 + separation + z0B * spread);
      yB.push(2.5 + separation * 0.7 + z1B * spread);
    }

    return {
      clusterA: { x: xA, y: yA },
      clusterB: { x: xB, y: yB }
    };
  }, [pointCount, spread, separation]);

  const totalPoints = pointCount * (showClusterB ? 2 : 1);

  return (
    <div className="showcase-grid">
      <aside className="control-panel">
        <h3 className="panel-title">PointPlot Controls</h3>

        <div className="control-group">
          <label>
            <span>Points per Cluster</span>
            <span>{pointCount}</span>
          </label>
          <input
            type="range"
            min="10"
            max="150"
            step="5"
            value={pointCount}
            onChange={(e) => setPointCount(parseInt(e.target.value, 10))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Point Radius (px)</span>
            <span>{pointSize.toFixed(1)} px</span>
          </label>
          <input
            type="range"
            min="1.5"
            max="8.0"
            step="0.5"
            value={pointSize}
            onChange={(e) => setPointSize(parseFloat(e.target.value))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Cluster Dispersion (σ)</span>
            <span>{spread.toFixed(2)}</span>
          </label>
          <input
            type="range"
            min="0.3"
            max="2.5"
            step="0.1"
            value={spread}
            onChange={(e) => setSpread(parseFloat(e.target.value))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Cluster Distance (Δ)</span>
            <span>{separation.toFixed(1)}</span>
          </label>
          <input
            type="range"
            min="1.0"
            max="7.0"
            step="0.5"
            value={separation}
            onChange={(e) => setSeparation(parseFloat(e.target.value))}
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
              checked={showClusterB}
              onChange={(e) => setShowClusterB(e.target.checked)}
            />
            Show Cluster B (Orange)
          </label>
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
          title="Bivariate Normal Clustering"
          xLabel="Feature X₁ (Standard Deviations)"
          yLabel="Feature X₂ (Standard Deviations)"
          showGrid={showGrid}
          showLegend={showLegend}
          axesStyle={axesStyle}
        >
          <PointPlot
            x={clusterA.x}
            y={clusterA.y}
            pointSize={pointSize}
            label="Cluster A (Control)"
            color="#0284c7"
          />
          {showClusterB && (
            <PointPlot
              x={clusterB.x}
              y={clusterB.y}
              pointSize={pointSize}
              label="Cluster B (Treated)"
              color="#f97316"
            />
          )}
        </Display>

        <div className="stats-bar">
          <span>Total Points: <strong>{totalPoints}</strong></span>
          <span>Point Radius: <strong>{pointSize}px</strong></span>
          <span>Clusters: <strong>{showClusterB ? '2 (Bimodal)' : '1 (Unimodal)'}</strong></span>
        </div>
      </section>
    </div>
  );
};
